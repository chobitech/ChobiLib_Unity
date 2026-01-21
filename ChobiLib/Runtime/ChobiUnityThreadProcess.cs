
/*
using UnityEngine.Events;

namespace ChobiLib.Unity
{
    public sealed class ChobiUnityThreadProcess : UnityMainThreadHoldingMonoBehaviour
    {
        private static ChobiUnityThreadProcess instance;

        public static new void RunOnUnityThread(UnityAction action)
        {
            if (instance != null && action != null)
            {
                instance.UnityMainContext.Post(_ => action(), null);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            instance = this;
        }
    }
}
*/
