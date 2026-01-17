using System.Threading;
using UnityEngine;

namespace ChobiLib.Unity
{
    public static class ChobiThreadInfo
    {
        private static int _mainThreadId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public static bool IsInMainThread => Thread.CurrentThread.ManagedThreadId == _mainThreadId;
    }
}