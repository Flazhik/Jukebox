using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JukeboxCore.Models.Downloader;
using JukeboxDownloader.Models;

namespace JukeboxDownloader.Service
{
    public abstract class AbstractDownloaderClient
    {
        public abstract bool SupportsUrl(string url);
        
        public abstract Task<List<DownloadableSongMetadata>> GetMetadata(string url, CancellationToken token);
        
        public abstract Task Download(string url, string playlist, IProgress<DownloadProgress> progress, CancellationToken token);
    }
}