using System;
using System.Collections.Generic;

namespace JukeboxCore.Events
{
    public class EventStore<T>
    {
        private readonly Dictionary<string, Action<JukeboxEvent<T>>> listeners = new();

        public void Subscribe(string id, Action<JukeboxEvent<T>> listener)
        {
            if (!listeners.ContainsKey(id))
                listeners[id] = delegate { };

            listeners[id] += listener;
        }

        public void Unsubscribe(string id, Action<JukeboxEvent<T>> listener)
        {
            if (listeners.ContainsKey(id))
                listeners[id] -= listener;
        }

        public void Invoke(string id, JukeboxEvent<T> arg)
        {
            if (listeners.TryGetValue(id, out var action))
                action?.Invoke(arg);
        }

        public void Clear() => listeners.Clear();
    }
}