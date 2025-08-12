namespace JukeboxCore.Exceptions
{
    public class ImageDownloadFailedException : JukeboxException
    {
        private readonly string url;
        private readonly string reason;
        
        public override string Message => $"An error has occured while downloading image {url}: {reason}";
        
        public ImageDownloadFailedException(string url, string reason)
        {
            this.url = url;
            this.reason = reason;
        }
    }
}