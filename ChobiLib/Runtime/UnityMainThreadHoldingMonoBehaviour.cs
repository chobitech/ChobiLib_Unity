using System.Threading;
using UnityEngine.Events;

namespace ChobiLib.Unity
{
    public class UnityMainThreadHoldingMonoBehaviour : SelfObjectHoldingMonoBehaviour
    {
        public SynchronizationContext UnityMainContext { get; private set; }

        public void RunOnUnityThread(UnityAction action)
        {
            if (action != null)
            {
                UnityMainContext?.Post(_ => action(), null);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            UnityMainContext = SynchronizationContext.Current;
        }
    }
}