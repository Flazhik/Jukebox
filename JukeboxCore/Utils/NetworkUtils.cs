using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JukeboxCore.Exceptions;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Networking.UnityWebRequestTexture;
using static System.TimeSpan;

namespace JukeboxCore.Utils
{
    public static class NetworkUtils
    {
        private const int RetryAttempts = 3;
        private static readonly HttpClient Client = new();
        
        static NetworkUtils()
        {
            Client.Timeout = FromSeconds(5);
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client.DefaultRequestHeaders.UserAgent.TryParseAdd("com.google.android.youtube/17.36.4 (Linux; U; Android 12; GB) gzip");
        }

        public static async Task<string> GetRaw(string url)
        {
            for (var i = 0; i < RetryAttempts; i++)
            {
                try
                {
                    var response = await Client.GetAsync(url);
                    var raw = await response.Content.ReadAsStringAsync();
                    if (raw == default)
                        continue;

                    return raw;
                }
                catch (Exception)
                {
                    if (i == RetryAttempts - 1)
                        throw;
                }
            }
            throw new HttpRequestException();
        }

        public static async Task<Texture2D> DownloadImage(string url) 
        {
            using var request = GetTexture(url);
            await request.SendWebRequest();

            if (request.error != null)
                throw new ImageDownloadFailedException(url, request.error);

            return DownloadHandlerTexture.GetContent(request);
        }
    }
}