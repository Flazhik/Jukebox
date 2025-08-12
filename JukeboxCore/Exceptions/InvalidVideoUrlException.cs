namespace JukeboxCore.Exceptions
{
    public class InvalidVideoUrlException : JukeboxException
    {
        public override string Message => "Invalid URL or track is not available";
    }
}