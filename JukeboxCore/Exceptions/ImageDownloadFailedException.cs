namespace JukeboxCore.Exceptions;

public class ImageDownloadFailedException(string url, string reason) : JukeboxException
{
    public override string Message => $"An error has occured while downloading image {url}: {reason}";
}