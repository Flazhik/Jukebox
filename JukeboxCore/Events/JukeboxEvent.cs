using System;

namespace JukeboxCore.Events
{
    public abstract class JukeboxEvent<T> : EventArgs
    {
        public T newState;
        
        public abstract void Mutate(T state);
    }
}