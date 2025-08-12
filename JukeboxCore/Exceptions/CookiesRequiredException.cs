namespace JukeboxCore.Exceptions
{
    public class CookiesRequiredException : JukeboxException
    {
        public override string Message => "Cookies required";
    }
}