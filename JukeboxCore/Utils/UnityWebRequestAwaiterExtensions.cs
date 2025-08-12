using UnityEngine.Networking;

namespace JukeboxCore.Utils
{
    public static class UnityWebRequestAwaiterExtensions
    {
        public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
        {
            return new UnityWebRequestAwaiter(asyncOp);
        }
    }
}