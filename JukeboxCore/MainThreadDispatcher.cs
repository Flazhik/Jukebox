using System;
using System.Collections.Generic;

namespace JukeboxCore
{
    [ConfigureSingleton(SingletonFlags.HideAutoInstance)]
    public class MainThreadDispatcher : MonoSingleton<MainThreadDispatcher>
    {
        private static readonly Queue<Action> ExecutionQueue = new();

        public void Enqueue(Action action)
        {
            lock (ExecutionQueue)
            {
                ExecutionQueue.Enqueue(action);
            }
        }

        private void Update()
        {
            lock (ExecutionQueue)
            {
                while (ExecutionQueue.Count > 0)
                {
                    ExecutionQueue.Dequeue()?.Invoke();
                }
            }
        }
    }
}