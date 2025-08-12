using System;
using System.Collections.Generic;

namespace JukeboxCore.Events
{
    public abstract class EventManager<T>
    {
        private readonly EventStore<T> events = new();
        private readonly Dictionary<string, T> states = new();

        public void Register(string id)
        {
            if (!states.ContainsKey(id))
                states.Add(id, DefaultState(id));
        }

        public T StateOf(string id) => states.TryGetValue(id, out var state) ? state : default;

        public void Subscribe(string id, Action<JukeboxEvent<T>> action) => events.Subscribe(id, action);
        
        public void Unsubscribe(string id, Action<JukeboxEvent<T>> action) => events.Unsubscribe(id, action);

        public void Invoke(string id, JukeboxEvent<T> e)
        {
            if (states.TryGetValue(id, out var state))
                e.Mutate(state);
            
            events.Invoke(id, e);
        }

        public void Map(Action<KeyValuePair<string, T>> action)
        {
            foreach (var state in states)
                action(state);
        }

        public void Clear()
        {
            states.Clear();
            events.Clear();
        }

        protected abstract T DefaultState(string id);
    }
}