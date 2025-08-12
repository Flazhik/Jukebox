namespace JukeboxCore.Exceptions
{
    public class InvalidCookiesFileException : JukeboxException
    {
        public override string Message => "Invalid cookies.txt file";
    }
}