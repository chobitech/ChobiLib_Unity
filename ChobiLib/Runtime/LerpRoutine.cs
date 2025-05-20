using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace ChobiLib.Unity
{
    public class LerpRoutine
    {
        private static readonly AnimationCurve DefaultLinearCurve = AnimationCurve.Linear(0, 0, 1, 1);

        internal static IEnumerator InnerRun(AnimationCurve curve, UnityAction<float> onProgress, UnityAction onProcessCompleted = null)
        {
            if (curve == null)
            {
                throw new System.Exception("AnimationCurve is null");
            }

            var sv = curve.GetStartValue();
            var ev = curve.GetEndValue();
            var sTime = curve.GetStartTime();
            var eTime = curve.GetEndTime();
            var durSec = eTime - sTime;

            if (Mathf.Approximately(sv, ev) || durSec <= 0)
            {
                onProgress?.Invoke(ev);
                onProcessCompleted?.Invoke();
                yield break;
            }

            var curTime = 0f;
            var alreadyReachedEnd = false;

            while (curTime < durSec)
            {
                curTime += Time.deltaTime;
                var rate = Mathf.Clamp01(curTime / durSec);
                onProgress?.Invoke(curve.Evaluate(Mathf.Lerp(sTime, eTime, rate)));
                alreadyReachedEnd = rate >= 1f;
                yield return null;
            }

            if (!alreadyReachedEnd)
            {
                onProgress?.Invoke(ev);
            }

            onProcessCompleted?.Invoke();
        }

        public AnimationCurve AnimationCurve { get; }

        public float StartValue => AnimationCurve.GetStartValue();
        public float StartTime => AnimationCurve.GetStartTime();

        public float EndValue => AnimationCurve.GetEndValue();
        public float EndTime => AnimationCurve.GetEndTime();

        public float ValueRange => EndValue - StartValue;
        public float DurationSec => EndTime - StartTime;

        private AnimationCurve reverseAnimationCurve;

        public AnimationCurve ReverseAnimationCurve => reverseAnimationCurve ??= AnimationCurve.GetReverseAnimationCurve();



        internal IEnumerator InnerReversibleRun(bool isReverse, UnityAction<float> onProgress, UnityAction onProcessCompleted = null)
        {
            return InnerRun(
                isReverse ? ReverseAnimationCurve : AnimationCurve,
                onProgress,
                onProcessCompleted
            );
        }


        public LerpRoutine(AnimationCurve animationCurve)
        {
            AnimationCurve = animationCurve ?? DefaultLinearCurve;
        }

        public LerpRoutine(float startValue = 0, float endValue = 1, float durationSec = 1)
        {
            AnimationCurve = AnimationCurve.Linear(0, startValue, durationSec, endValue);
        }

        public IEnumerator Run(
            UnityAction<float> onProgress,
            UnityAction onProcessCompleted = null) => InnerReversibleRun(false, onProgress, onProcessCompleted);

        public IEnumerator RunReverse(
            UnityAction<float> onProgress,
            UnityAction onProcessCompleted = null) => InnerReversibleRun(true, onProgress, onProcessCompleted);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onProgress">first: true if backward phase, second: progress value</param>
        /// <param name="onForwardCompleted"></param>
        /// <param name="onAllCompleted"></param>
        /// <returns></returns>
        public IEnumerator RunPingPong(
            UnityAction<bool, float> onProgress,
            int repeatCount = 1,
            IEnumerator routineOnForwardCompleted = null,
            UnityAction<int> onPingPongCompleted = null)
        {
            var count = 0;
            while (repeatCount <= 0 || count < repeatCount)
            {
                yield return Run(prog => onProgress?.Invoke(false, prog));
                yield return routineOnForwardCompleted;
                yield return RunReverse(prog => onProgress?.Invoke(true, prog), () => onPingPongCompleted?.Invoke(++count));
            }
        }

        public IEnumerator RunPingPong(
            UnityAction<bool, float> onProgress,
            int repeatCount = 1,
            UnityAction onForwardCompleted = null,
            UnityAction<int> onPingPongCompleted = null) => RunPingPong(
                onProgress,
                repeatCount,
                onForwardCompleted?.ToRoutine(),
                onPingPongCompleted
            );

        public static implicit operator LerpRoutine(AnimationCurve curve) => new(curve);
    }
}