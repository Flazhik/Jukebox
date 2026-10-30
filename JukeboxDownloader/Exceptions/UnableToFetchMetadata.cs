using JukeboxCore.Exceptions;

namespace JukeboxDownloader.Exceptions;

public class UnableToFetchMetadata(string message) : JukeboxException
{
    public override string Message => "Unable to fetch metadata: " + message;
}