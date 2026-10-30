using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using static JukeboxCore.Logging.JukeboxLogger;

namespace JukeboxDownloader;

public class QueueManager : IDisposable
{
    private readonly Channel<QueuedTask> channel;
    private readonly object @lock = new();
    private readonly List<CancellationTokenSource> retired = [];
    private readonly List<Task> workers;
    private bool disposed;
    private CancellationTokenSource token;

    public QueueManager(int workersCount)
    {
        channel = Channel.CreateUnbounded<QueuedTask>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });

        token = new CancellationTokenSource();
        workers = Enumerable.Range(0, workersCount)
            .Select(_ => Task.Run(WorkerLoop))
            .ToList();
    }

    public CancellationToken Token
    {
        get
        {
            lock (@lock)
            {
                return token.Token;
            }
        }
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;

        channel.Writer.TryComplete();

        try
        {
            Task.WhenAll(workers).GetAwaiter().GetResult();
        }
        catch
        {
            // ignored
        }

        token.Cancel();
        token.Dispose();

        lock (@lock)
        {
            foreach (var source in retired)
                source.Dispose();

            retired.Clear();
        }
    }

    public void Enqueue(Func<CancellationToken, Task> taskFactory)
    {
        if (!channel.Writer.TryWrite(new QueuedTask(Token, taskFactory)))
            throw new InvalidOperationException("Queue is closed");
    }

    public void CancelAll()
    {
        CancellationTokenSource previous;
        lock (@lock)
        {
            previous = token;
            token = new CancellationTokenSource();
            retired.Add(previous);
        }

        try
        {
            previous.Cancel();
        }
        catch (Exception e)
        {
            Log.LogError(e);
        }

        while (channel.Reader.TryRead(out _))
        {
        }
    }

    private async Task WorkerLoop()
    {
        while (await channel.Reader.WaitToReadAsync())
        {
            if (!channel.Reader.TryRead(out var queued))
                continue;

            if (queued.Token.IsCancellationRequested)
                continue;

            try
            {
                await queued.Factory(queued.Token);
            }
            catch (OperationCanceledException) when (queued.Token.IsCancellationRequested)
            {
            }
            catch (Exception e)
            {
                Log.LogError(e);
            }
        }
    }

    private readonly struct QueuedTask(CancellationToken token, Func<CancellationToken, Task> factory)
    {
        public CancellationToken Token { get; } = token;
        public Func<CancellationToken, Task> Factory { get; } = factory;
    }
}
