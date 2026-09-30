using System.IO;
using BepInEx.Logging;
using JukeboxCore.Utils;
namespace JukeboxCore.Logging;

public static class JukeboxLogger
{
    private static FileLogAppender _appender;
    public static ManualLogSource Log { get; private set; }
    
    public static void Initialize(ManualLogSource source)
    {
        Log = source;
        _appender = new FileLogAppender(source, Path.Combine(PathsUtils.LogsPath, source.SourceName));
        Logger.Listeners.Add(_appender);
    }
    
    public static void Shutdown()
    {
        if (_appender == null)
            return;
        Logger.Listeners.Remove(_appender);
        
        _appender.Dispose();
        _appender = null;
    }
}