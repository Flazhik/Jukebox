using System.Collections.Generic;
using System.Threading.Tasks;
using JukeboxCore;
using JukeboxCore.Events;
using JukeboxCore.Models.Downloader;
using UnityEngine;
using static JukeboxCore.Utils.NetworkUtils;

namespace JukeboxDownloader.Service;

public class DownloaderUIEventManager : EventManager<DownloadableEntityState>
{
    private static readonly Dictionary<string, Sprite> ThumbnailCache = new();
    private static readonly object CacheLock = new();

    public async Task<Sprite> DownloadThumbnail(string id, string thumbnailUrl)
    {
        lock (CacheLock)
        {
            if (ThumbnailCache.TryGetValue(id, out var sprite))
                return sprite;
        }

        var spriteLoaded = await DownloadSprite(thumbnailUrl);
        lock (CacheLock)
        {
            ThumbnailCache.TryAdd(id, spriteLoaded);
        }

        MainThreadDispatcher.Instance?.Enqueue(() => Update(id, s => s.thumbnail = spriteLoaded));

        return spriteLoaded;
    }

    private static async Task<Sprite> DownloadSprite(string url)
    {
        var tex = await DownloadImage(url);
        var size = Mathf.Min(tex.width, tex.height);

        var spriteRect = new Rect((tex.width - size) / 2f, (tex.height - size) / 2f, size, size);
        return Sprite.Create(tex, spriteRect, new Vector2(0.5f, 0.5f));
    }

    protected override DownloadableEntityState DefaultState(string id)
    {
        lock (CacheLock)
        {
            return new DownloadableEntityState
            {
                state = TrackDownloadingState.None,
                thumbnail = ThumbnailCache.GetValueOrDefault(id)
            };
        }
    }
}
