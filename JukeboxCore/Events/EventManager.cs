using System;
using System.Collections.Generic;

namespace JukeboxCore.Events;

public abstract class EventManager<T>
{
    private readonly Dictionary<string, T> states = new();
    private readonly Dictionary<string, Action<T>> listeners = new();
    private readonly object @lock = new();

    public void Register(string id)
    {
        if (!states.ContainsKey(id))
            states.Add(id, DefaultState(id));
    }

    public void Subscribe(string id, Action<T> listener)
    {
        lock (@lock)
        {
            if (!listeners.ContainsKey(id))
                listeners[id] = static delegate { };

            listeners[id] += listener;
        }
    }

    public void Unsubscribe(string id, Action<T> listener)
    {
        lock (@lock)
        {
            if (listeners.ContainsKey(id))
                listeners[id] -= listener;
        }
    }

    public void Update(string id, Action<T> mutate)
    {
        if (!states.TryGetValue(id, out var state))
            return;

        mutate(state);

        Action<T> action;
        lock (@lock)
        {
            listeners.TryGetValue(id, out action);
        }

        action?.Invoke(state);
    }

    public void Map(Action<KeyValuePair<string, T>> action)
    {
        foreach (var state in states)
            action(state);
    }

    public T StateOf(string id) => states.GetValueOrDefault(id);

    public void Clear()
    {
        states.Clear();
        lock (@lock)
        {
            listeners.Clear();
        }
    }

    protected abstract T DefaultState(string id);
}
