using System.Collections.Generic;
using System.Threading.Tasks;
using JukeboxCore.Events;
using JukeboxCore.Models.Downloader;
using JukeboxDownloader.Events;
using UnityEngine;
using static JukeboxCore.Utils.NetworkUtils;

namespace JukeboxDownloader.Service
{
    public class DownloaderUIEventManager : EventManager<DownloadableEntityState>
    {
        private static readonly Dictionary<string, Sprite> ThumbnailCache = new();

        public async Task<Sprite> DownloadThumbnail(string id, string thumbnailUrl)
        {
            if (ThumbnailCache.TryGetValue(id, out var sprite))
                return sprite;

            sprite = await DownloadSprite(thumbnailUrl);
            ThumbnailCache.Add(id, sprite);
            Invoke(id, new ThumbnailLoadedEvent(sprite));
            
            return sprite;
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
            return new DownloadableEntityState
            {
                state = TrackDownloadingState.None,
                thumbnail = ThumbnailCache.TryGetValue(id, out var thumbnail) ? thumbnail : default
            };
        }
    }
}