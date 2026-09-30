using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using BepInEx.Logging;

namespace JukeboxCore.Logging;

public class FileLogAppender : ILogListener
{
    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(3);
    private static readonly DateTime LogsLifetimeThreshold = DateTime.UtcNow.AddDays(-3);

    private readonly object @lock = new();
    private readonly ManualLogSource source;
    private readonly StreamWriter writer;
    private readonly Timer flushTimer;
    private bool disposed;

    public FileLogAppender(ManualLogSource source, string logsDirectory)
    {
        this.source = source;

        var logsDir = Directory.CreateDirectory(logsDirectory);
        
        foreach (var file in logsDir.GetFiles().Where(static file => file.LastWriteTimeUtc < LogsLifetimeThreshold))
        {
            try
            {
                file.Delete();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        var logsFile = Path.Combine(logsDirectory, $"{source.SourceName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");
        writer = new StreamWriter(logsFile, true, Encoding.UTF8);
        flushTimer = new Timer(static state => ((FileLogAppender)state).Flush(), this, FlushInterval, FlushInterval);
    }

    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        if (!ReferenceEquals(eventArgs.Source, source))
            return;

        lock (@lock)
        {
            if (disposed)
                return;

            writer.WriteLine($"[{DateTime.Now.ToUniversalTime():O}] {eventArgs}");
        }
    }

    private void Flush()
    {
        lock (@lock)
        {
            if (disposed)
                return;

            writer.Flush();
        }
    }

    public void Dispose()
    {
        flushTimer.Dispose();

        lock (@lock)
        {
            if (disposed)
                return;

            disposed = true;
            writer.Flush();
            writer.Dispose();
        }
    }
}
