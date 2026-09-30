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
using JukeboxDownloader.Exceptions;
using JukeboxDownloader.Models;
using JukeboxDownloader.Service.YouTube;
using static JukeboxDownloader.Utils.ThirdPartySoftwareUtils;
using static YoutubeDLSharp.Utils;
using static JukeboxCore.Utils.ConfigUtils;
using static SingletonFlags;
using static JukeboxCore.Logging.JukeboxLogger;

namespace JukeboxDownloader.Service;

[ConfigureSingleton(HideAutoInstance)]
public class JukeboxDownloaderService : MonoSingleton<JukeboxDownloaderService>
{
    private static readonly List<AbstractDownloaderClient> Clients = [new YoutubeClient()];

    private readonly ConfigEntry<int> queueSize = BepInExConfig.Bind("Concurrency",
        "DownloaderQueueSize",
        3,
        "The size of the downloader queue");
    
    private MainThreadDispatcher mainThread;
    private QueueManager queue;

    private int mTotalDownloaded;
    private int mTotalEnqueued;
    private int mTotalFailed;

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
        get;
        set
        {
            field = value;
            mainThread.Enqueue(() => OnStateChanged?.Invoke(value));
        }
    } = JukeboxDownloaderState.Idle;

    public ThirdPartyExecsState ThirdPartySoftwareState
    {
        get;
        set
        {
            field = value;
            mainThread.Enqueue(() => OnThirdPartySoftwareChanged?.Invoke(value));
        }
    } = new();

    public DownloaderUIEventManager UIEventManager { get; } = new();
    public List<DownloadableSongMetadata> Metadata { get; private set; } = [];

    protected void Start()
    {
        queue = new QueueManager(queueSize.Value);
        mainThread = MainThreadDispatcher.Instance;
    }

    protected void OnDestroy()
    {
        CancelEverything();
    }

    public event Action<ThirdPartyExecsState> OnThirdPartySoftwareChanged;
    public event Action<JukeboxDownloaderState> OnStateChanged;
    public event Action<List<DownloadableSongMetadata>> OnMetadataChanged;

    public static bool SupportsUrl(string url)
    {
        return Clients.Any(client => client.SupportsUrl(url));
    }

    public void Clear()
    {
        UIEventManager.Clear();
    }

    public void CancelEverything()
    {
        try
        {
            queue.CancelAll();
        }
        catch (Exception e)
        {
            Log.LogError(e);
        }

        ResetCounters();
        UIEventManager.Map(kv =>
        {
            var s = kv.Value.state;
            if (s.HasValue && !s.IsOneOf(TrackDownloadingState.Failed, TrackDownloadingState.Success))
                UIEventManager.Update(kv.Key, static s => s.state = TrackDownloadingState.None);
        });
    }

    public async Task LoadMetadata(string url)
    {
        try
        {
            Log.LogInfo($"Loading metadata for URL: {url}");
            queue.CancelAll();
            State = JukeboxDownloaderState.LoadingMetadata;
            var fetchedMetadata = await GetClientFor(url).GetMetadata(url, queue.Token);
            lock (Metadata)
            {
                Metadata = fetchedMetadata;
            }

            foreach (var track in Metadata)
                UIEventManager.Register(track.Id);
            
            Log.LogInfo($"Metadata loaded successfully. Total tracks: {Metadata.Count}");

            mainThread.Enqueue(() => OnMetadataChanged?.Invoke(Metadata));
        }
        catch (JukeboxException e)
        {
            Log.LogError($"Unable to retrieve metadata: {e.Message}");
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

        UIEventManager.Update(id, static s => s.state = TrackDownloadingState.Enqueued);
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

                mainThread.Enqueue(() => UIEventManager.Update(id, s =>
                {
                    s.state = p.State;
                    s.progress = p.Progress;
                    s.eta = p.Eta;
                }));
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
            await UpdateYtDlp();
            CleanUp();

            ThirdPartySoftwareState = ThirdPartyExecsState.Present();
        }
        catch (Exception)
        {
            ThirdPartySoftwareState = ThirdPartyExecsState.Failed();
        }
    }

    public async Task UpdateYtDlp()
    {
        try
        {
            await DownloadSoftwarePiece("yt-dlp", YtDlpBinaryName, DownloadYtDlp);
            CleanUp();

            ThirdPartySoftwareState = ThirdPartyExecsState.Present();
        }
        catch (Exception)
        {
            ThirdPartySoftwareState = ThirdPartyExecsState.Failed();
        }
    }

    private async Task DownloadSoftwarePiece(string softwareTitle, string filename, Func<WebClient, Task> downloadRoutine)
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
            Log.LogError($"An error has occured while downloading 3rd-party software: {e.Message}");
            ThirdPartySoftwareState = ThirdPartyExecsState.Failed();
            throw;
        }
    }

    private void ResetCounters()
    {
        TotalDownloaded = 0;
        TotalFailed = 0;
        TotalEnqueued = 0;
    }

    private static AbstractDownloaderClient GetClientFor(string url)
    {
        var result = Clients.FirstOrDefault(client => client.SupportsUrl(url));
        return result ?? throw new UrlIsNotSupported();
    }
}