using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace JukeboxDownloader
{
    public class QueueManager : IDisposable
    {
        private readonly Channel<Func<CancellationToken, Task>> channel;
        private readonly List<Task> workers;
        private CancellationTokenSource token;
        private readonly object @lock = new();
        private bool disposed;

        public QueueManager(int workersCount)
        {
            channel = Channel.CreateUnbounded<Func<CancellationToken, Task>>(new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            });

            token = new CancellationTokenSource();
            workers = Enumerable.Range(0, workersCount)
                .Select(_ => Task.Run(WorkerLoop))
                .ToList();
        }
        
        public void Enqueue(Func<CancellationToken, Task> taskFactory)
        {
            if (!channel.Writer.TryWrite(taskFactory))
                throw new InvalidOperationException("Queue is closed.");
        }
        
        public void CancelAll()
        {
            lock (@lock)
            {
                token.Cancel(); 
                token.Dispose();
                token = new CancellationTokenSource();

                while (channel.Reader.TryRead(out _)) { }
            }
        }

        private async Task WorkerLoop()
        {
            while (await channel.Reader.WaitToReadAsync())
            {
                if (!channel.Reader.TryRead(out var taskFactory))
                    continue;
                
                CancellationToken ct;
                lock (@lock)
                {
                    ct = token.Token;
                }

                try
                {
                    await taskFactory(ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
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
        }
    }
}
