using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace JukeboxCore.Utils
{
    public class UnityWebRequestAwaiter : INotifyCompletion
    {
        private readonly UnityWebRequestAsyncOperation asyncOp;
        private Action continuation;

        public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp)
        {
            this.asyncOp = asyncOp;
            asyncOp.completed += OnRequestCompleted;
        }

        public bool IsCompleted => asyncOp.isDone;

        public void GetResult() { }

        public void OnCompleted(Action action)
        {
            continuation = action;
        }

        private void OnRequestCompleted(AsyncOperation obj)
        {
            continuation();
        }
    }
}