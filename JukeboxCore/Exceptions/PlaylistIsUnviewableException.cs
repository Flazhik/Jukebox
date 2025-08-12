namespace JukeboxCore.Exceptions
{
    public class PlaylistIsUnviewableException : JukeboxException
    {
        public override string Message => "This type of playlist cannot be downloaded";
    }
}