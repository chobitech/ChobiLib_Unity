using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace ChobiLib.Unity
{
    public class DelayRoutine
    {
        public static IEnumerator RunAfterDelay(WaitForSeconds waitForSeconds, IEnumerator routineAfterDelay)
        {
            yield return waitForSeconds;
            yield return routineAfterDelay;
        }
        public static IEnumerator RunAfterDelay(float waitSeconds, IEnumerator routineAfterDelay) => RunAfterDelay(new WaitForSeconds(waitSeconds), routineAfterDelay);
        public static IEnumerator RunAfterDelay(WaitForSeconds waitForSeconds, UnityAction actionAfterDelay) => RunAfterDelay(waitForSeconds, actionAfterDelay.ToRoutine());
        public static IEnumerator RunAfterDelay(float waitSeconds, UnityAction actionAfterDelay) => RunAfterDelay(new WaitForSeconds(waitSeconds), actionAfterDelay);


        public WaitForSeconds WaitForSeconds { get; }

        public float DelaySeconds { get; }


        public DelayRoutine(float delaySec)
        {
            DelaySeconds = delaySec;
            WaitForSeconds = new(delaySec);
        }

        public IEnumerator RunAfterDelay(IEnumerator routine) => RunAfterDelay(WaitForSeconds, routine);
        public IEnumerator RunAfterDelay(UnityAction action) => RunAfterDelay(WaitForSeconds, action);
    }
}