using JukeboxCore.Exceptions;

namespace JukeboxDownloader.Exceptions
{
    public class UrlIsNotSupported : JukeboxException
    {
        public override string Message => "URL is not supported";
    }
}