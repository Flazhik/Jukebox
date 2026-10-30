using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

namespace JukeboxCore;

[ConfigureSingleton(SingletonFlags.HideAutoInstance)]
public class MainThreadDispatcher : MonoSingleton<MainThreadDispatcher>
{
    private static readonly ConcurrentQueue<Action> ExecutionQueue = new();

    private void Update()
    {
        while (ExecutionQueue.TryDequeue(out var action))
            action();
    }

    public void Enqueue(Action action) => ExecutionQueue.Enqueue(action);
    
    public void RunAsync(Func<Task> task, Action<Exception> onError = null)
    {
        Task.Run(task).ContinueWith(t =>
        {
            if (!t.IsFaulted)
                return;

            if (onError != null)
                Enqueue(() => onError(t.Exception));
            else
                Enqueue(() => Debug.LogException(t.Exception));
        });
    }
}