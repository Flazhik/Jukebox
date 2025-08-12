using JukeboxCore.Exceptions;

namespace JukeboxDownloader.Exceptions
{
    public class UnableToFetchMetadata : JukeboxException
    {
        private readonly string message;
        
        public UnableToFetchMetadata(string message)
        {
            this.message = message;
        }

        public override string Message => "Unable to fetch metadata: " + message;
    }
}