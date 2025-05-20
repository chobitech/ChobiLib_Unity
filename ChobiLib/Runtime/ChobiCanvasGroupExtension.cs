using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace ChobiLib.Unity
{
    public static class ChobiCanvasGroupExtension
    {
        private static void ChangeAlphaOnProgress(CanvasGroup group, float progress, float startAlpha, float valueGap, UnityAction<float> onProgress)
        {
            var alpha = Mathf.Clamp01((progress - startAlpha) / valueGap);
            group.alpha = alpha;
            onProgress?.Invoke(alpha);
        }

        public static IEnumerator FadeAnimationRoutine(
            this CanvasGroup group,
            LerpRoutine lerpRoutine,
            bool isReverse = false,
            UnityAction<float> onProgress = null,
            UnityAction onProcessCompleted = null
        )
        {
            var sv = lerpRoutine.StartValue;
            var valueGap = lerpRoutine.EndValue - sv;

            yield return lerpRoutine.InnerReversibleRun(
                    isReverse,
                    prog => ChangeAlphaOnProgress(group, prog, sv, valueGap, onProgress),
                    onProcessCompleted
                );
        }

        public static IEnumerator FadeAnimationRoutine(
            this CanvasGroup group,
            float startAlpha = 0f,
            float endAlpha = 1f,
            float durationSec = 1f,
            UnityAction<float> onProgress = null,
            UnityAction onProcessCompleted = null
        ) => group.FadeAnimationRoutine(
            new LerpRoutine(startAlpha, endAlpha, durationSec),
            false,
            onProgress,
            onProcessCompleted
        );


        public static IEnumerator FadeInAndOutAnimationRoutine(
            this CanvasGroup group,
            LerpRoutine lerpRoutine,
            UnityAction<bool, float> onProgress,
            IEnumerator routineOnForwardCompleted = null,
            UnityAction onAllProcessCompleted = null
        )
        {
            var startAlpha = lerpRoutine.AnimationCurve.GetStartValue();
            var valueGap = lerpRoutine.AnimationCurve.GetValueGap();

            yield return lerpRoutine.RunPingPong(
                (inBackward, prog) => ChangeAlphaOnProgress(group, prog, startAlpha, valueGap, alpha => onProgress?.Invoke(inBackward, alpha)),
                1,
                routineOnForwardCompleted,
                _ => onAllProcessCompleted?.Invoke()
            );
        }

        public static IEnumerator FadeInAndOutAnimationRoutine(
            this CanvasGroup group,
            LerpRoutine lerpRoutine,
            UnityAction<bool, float> onProgress,
            UnityAction onForwardCompleted = null,
            UnityAction onAllProcessCompleted = null
        ) => group.FadeInAndOutAnimationRoutine(
            lerpRoutine,
            onProgress,
            onForwardCompleted?.ToRoutine(),
            onAllProcessCompleted
        );


        public static IEnumerator FadeInAndOutAnimationRoutine(
            this CanvasGroup group,
            UnityAction<bool, float> onProgress,
            float startAlpha = 0f,
            float endAlpha = 1f,
            float durationSec = 1f,
            IEnumerator routineOnForwardCompleted = null,
            UnityAction onAllProcessCompleted = null
        ) => group.FadeInAndOutAnimationRoutine(
            new LerpRoutine(startAlpha, endAlpha, durationSec),
            onProgress,
            routineOnForwardCompleted,
            onAllProcessCompleted
        );


        public static IEnumerator FadeInAndOutAnimationRoutine(
            this CanvasGroup group,
            UnityAction<bool, float> onProgress,
            float startAlpha = 0f,
            float endAlpha = 1f,
            float durationSec = 1f,
            UnityAction onForwardCompleted = null,
            UnityAction onAllProcessCompleted = null
        ) => group.FadeInAndOutAnimationRoutine(
            onProgress,
            startAlpha,
            endAlpha,
            durationSec,
            onForwardCompleted?.ToRoutine(),
            onAllProcessCompleted
        );
    }
}