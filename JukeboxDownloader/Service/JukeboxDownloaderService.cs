using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Configuration;
using JukeboxCore;
using JukeboxCore.Exceptions;
using JukeboxCore.Models.Downloader;
using JukeboxCore.Utils;
using JukeboxDownloader.Events;
using JukeboxDownloader.Exceptions;
using JukeboxDownloader.Models;
using JukeboxDownloader.Service.YouTube;
using UnityEngine;
using static JukeboxDownloader.Utils.ThirdPartySoftwareUtils;
using static YoutubeDLSharp.Utils;
using static JukeboxCore.Utils.ConfigUtils;

namespace JukeboxDownloader.Service
{
    public class JukeboxDownloaderService : MonoSingleton<JukeboxDownloaderService>
    {
        private readonly ConfigEntry<int> queueSize = BepInExConfig.Bind("Concurrency",
            "DownloaderQueueSize",
            3,
            "The size of the downloader queue");
        
        private static readonly List<AbstractDownloaderClient> Clients = new()
        {
            new YoutubeClient()
        };

        public int TotalDownloaded
        {
            get => mTotalDownloaded;
            private set => mTotalDownloaded = value;
        }

        public int TotalFailed
        {
            get => mTotalFailed;
            private set => mTotalFailed = value;
        }
        
        public int TotalEnqueued
        {
            get => mTotalEnqueued;
            private set => mTotalEnqueued = value;
        }

        public JukeboxDownloaderState State
        {
            get => state;
            set
            {
                state = value;
                mainThread.Enqueue(() => OnStateChanged?.Invoke(value));
            }
        }
        
        public ThirdPartyExecsState ThirdPartySoftwareState
        {
            get => thirdPartySoftwareState;
            set
            {
                thirdPartySoftwareState = value;
                mainThread.Enqueue(() => OnThirdPartySoftwareChanged?.Invoke(value));
            }
        }
        
        public DownloaderUIEventManager UIEventManager { get; } = new();
        public List<DownloadableSongMetadata> Metadata { get; private set; } = new();
        
        public event Action<ThirdPartyExecsState> OnThirdPartySoftwareChanged;
        public event Action<JukeboxDownloaderState> OnStateChanged;
        public event Action<List<DownloadableSongMetadata>> OnMetadataChanged;
        
        private QueueManager queue;
        private CancellationTokenSource tokenSource = new();
        private ThirdPartyExecsState thirdPartySoftwareState = new();
        private MainThreadDispatcher mainThread;

        private int mTotalDownloaded;
        private int mTotalFailed;
        private int mTotalEnqueued;
        
        private JukeboxDownloaderState state = JukeboxDownloaderState.Idle;

        protected override void Awake()
        {
            base.Awake();
            queue = new QueueManager(queueSize.Value);
            mainThread = MainThreadDispatcher.Instance;
        }

        public static bool SupportsUrl(string url) => Clients.Any(client => client.SupportsUrl(url));
        
        public void Clear() => UIEventManager.Clear();

        public void CancelEverything()
        {
            tokenSource.Cancel();
            queue.CancelAll();
            ResetCounters();
            UIEventManager.Map(kv =>
            {
                var s = kv.Value.state;
                if (s.HasValue && !s.IsOneOf(TrackDownloadingState.Failed, TrackDownloadingState.Success))
                    UIEventManager.Invoke(kv.Key, new DownloadCanceledEvent());
            });
        }

        public async Task LoadMetadata(string url)
        {
            try
            {
                queue.CancelAll();
                State = JukeboxDownloaderState.LoadingMetadata;
                Metadata = await GetClientFor(url).GetMetadata(url, RegenerateTokenSource().Token);
                foreach (var track in Metadata)
                    UIEventManager.Register(track.Id);
                mainThread.Enqueue(() => OnMetadataChanged?.Invoke(Metadata));
            }
            catch (JukeboxException e)
            {
                mainThread.Enqueue(() => Debug.LogError($"Unable to retrieve metadata: {e.Message}"));
                throw;
            }
            finally
            {
                State = JukeboxDownloaderState.Idle;
            }
        }

        public void EnqueueDownload(string id, string url, string playlist, bool decrementFailed = false)
        {
            Interlocked.Increment(ref mTotalEnqueued);
            if (decrementFailed)
                Interlocked.Decrement(ref mTotalFailed);
            
            UIEventManager.Invoke(id, new TrackEnqueuedEvent());
            queue.Enqueue(token =>
            {
                var progressHandler = new Progress<DownloadProgress>(p =>
                {
                    if (token.IsCancellationRequested)
                        return;
                    
                    switch (p.State)
                    {
                        case TrackDownloadingState.Success:
                            Interlocked.Decrement(ref mTotalEnqueued);
                            Interlocked.Increment(ref mTotalDownloaded);
                            break;
                        case TrackDownloadingState.Failed:
                            Interlocked.Decrement(ref mTotalEnqueued);
                            Interlocked.Increment(ref mTotalFailed);
                            break;
                    }
                    UIEventManager.Invoke(id, new ProgressUpdatedEvent(p.State, p.Progress, p.Eta));
                });
                return GetClientFor(url).Download(url, playlist, progressHandler, token);
            });
        }

        public async Task DownloadThirdPartySoftware()
        {
            try
            {
                await DownloadSoftwarePiece("ffmpeg", FfmpegBinaryName, DownloadFfmpeg);
                await DownloadSoftwarePiece("ffprobe", FfprobeBinaryName, DownloadFfprobe);
                await DownloadSoftwarePiece("yt-dlp", YtDlpBinaryName, DownloadYtDlp);
                CleanUp();
                
                ThirdPartySoftwareState = ThirdPartyExecsState.Present();
            }
            catch (Exception)
            {
                ThirdPartySoftwareState = ThirdPartyExecsState.Failed();
            }

            async Task DownloadSoftwarePiece(string softwareTitle, string filename, Func<WebClient, Task> downloadRoutine)
            {
                try
                {
                    if (ThirdPartySoftwarePresent(filename) && ValidateBinaryIntegrity(filename))
                        return;

                    ThirdPartySoftwareState = ThirdPartyExecsState.DownloadingStarted();
                    using var client = new WebClient();
                    client.DownloadProgressChanged += (_, e) =>
                        ThirdPartySoftwareState =
                            ThirdPartyExecsState.DownloadingProgress(softwareTitle, e.ProgressPercentage / 100f);
                
                    await downloadRoutine(client);
                }
                catch (Exception e)
                {
                    mainThread.Enqueue(() =>
                        Debug.LogError($"An error has occured while downloading 3rd-party software: {e.Message}"));
                    ThirdPartySoftwareState = ThirdPartyExecsState.Failed();
                    throw;
                }
            }
        }

        private void ResetCounters()
        {
            TotalDownloaded = 0;
            TotalFailed = 0;
            TotalEnqueued = 0;
        }

        private CancellationTokenSource RegenerateTokenSource()
        {
            tokenSource.Cancel();
            tokenSource = new CancellationTokenSource();
            return tokenSource;
        }

        private static AbstractDownloaderClient GetClientFor(string url)
        {
            var result = Clients.FirstOrDefault(client => client.SupportsUrl(url));
            if (result == default)
                throw new UrlIsNotSupported();
            
            return result;
        }

        protected override void OnDestroy()
        {
            CancelEverything();
            base.OnDestroy();
        }
    }
}