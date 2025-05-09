using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

namespace ChobiLib.Unity
{
    public static class ChobiLibUnity
    {
        //---> 2025/04/05 added
        public static string GetCrossPlatformSavedataPath(string dataDirName = "savedata")
        {
            var userDirPath = Application.persistentDataPath;
            return Path.Combine(userDirPath, dataDirName);
        }
        //<---

        //--- added: 2025/01/29
        private static readonly Regex hexColorRegex = new(@"^#([0-9a-f]{3,8})$", RegexOptions.IgnoreCase);

        public static int ToColorInt(this string hex) => Math.Clamp(int.Parse(hex, System.Globalization.NumberStyles.HexNumber), 0, 255);
        public static float ToColorFloat(this int i) => Math.Clamp(i / 255f, 0f, 1f);
        public static float ToColorFloat(this string hex) => hex.ToColorInt().ToColorFloat();

        public static Color? ParseHexToColor(string hex)
        {
            var m = hexColorRegex.Match(hex);

            if (m.Success)
            {
                var h = m.Groups[1].Value;

                switch (h.Length)
                {
                    case 3:
                        return new(
                            $"{h[0]}{h[0]}".ToColorFloat(),
                            $"{h[1]}{h[1]}".ToColorFloat(),
                            $"{h[2]}{h[2]}".ToColorFloat()
                        );

                    case 6:
                        return new(
                            $"{h[0]}{h[1]}".ToColorFloat(),
                            $"{h[2]}{h[3]}".ToColorFloat(),
                            $"{h[4]}{h[5]}".ToColorFloat()
                        );

                    case 8:
                        return new(
                            $"{h[2]}{h[3]}".ToColorFloat(),
                            $"{h[4]}{h[5]}".ToColorFloat(),
                            $"{h[6]}{h[7]}".ToColorFloat(),
                            $"{h[0]}{h[1]}".ToColorFloat()
                        );
                }
            }

            return null;
        }

        public static Color? ToColor(this string hex) => ParseHexToColor(hex);


        //---> 2025/05/07 added
        public static void SetColor(
            this SpriteRenderer sr,
            float? red = null,
            float? green = null,
            float? blue = null,
            float? alpha = null)
        {
            var c = sr.color;
            sr.color = new(
                red ?? c.r,
                green ?? c.g,
                blue ?? c.b,
                alpha ?? c.a
            );
        }

        public static void MoveBy(
            this Transform tr,
            float dx = 0f,
            float dy = 0f,
            float dz = 0f)
        {
            var pos = tr.position;
            tr.position = pos + new Vector3(dx, dy, dz);
        }

        public static void LocalMoveBy(
            this Transform tr,
            float dx = 0f,
            float dy = 0f,
            float dz = 0f)
        {
            var pos = tr.localPosition;
            tr.localPosition = pos + new Vector3(dx, dy, dz);
        }

        public static void SetMargin(
            this RectTransform rt,
            float? left = null,
            float? top = null,
            float? right = null,
            float? bottom = null)
        {
            var leftBottom = rt.offsetMin;
            rt.offsetMin = new(
                left ?? leftBottom.x,
                bottom ?? leftBottom.y
            );

            var rightTop = rt.offsetMax;
            rt.offsetMax = new(
                -right ?? rightTop.x,
                -top ?? rightTop.y
            );
        }


        //---> revised and added at 2025/05/09
        public static IEnumerator FadeAnimationRoutine(
            this CanvasGroup canvasGroup,
            bool isFadeIn,
            float durationSec,
            float? startAlpha = null,
            float? endAlpha = null,
            bool useFlexibleDuration = true,
            UnityAction<float> onProgress = null,
            UnityAction onRoutineFinished = null)
        {
            var curAlpha = canvasGroup.alpha;

            var sAlpha = startAlpha ?? curAlpha;
            var eAlpha = endAlpha ?? (isFadeIn ? 1f : 0f);

            if (sAlpha == eAlpha)
            {
                onRoutineFinished?.Invoke();
                yield break;
            }

            var fadeSec = useFlexibleDuration
                ? durationSec * Mathf.Clamp01(Mathf.Abs(eAlpha - sAlpha))
                : durationSec;
            
            var gObj = canvasGroup.gameObject;
            gObj.SetActive(true);

            yield return LerpRoutine(
                fadeSec,
                alpha =>{
                    canvasGroup.alpha = alpha;
                    onProgress?.Invoke(alpha);
                },
                sAlpha,
                eAlpha,
                () =>
                {
                    if (!isFadeIn)
                    {
                        gObj.SetActive(false);
                    }
                    onRoutineFinished?.Invoke();
                }
            );
        }

        public static IEnumerator FadeInAndOutRoutine(
            this CanvasGroup canvasGroup,
            float durationSecOnOnWay,
            float? startAlpha = null,
            float? endAlpha = null,
            bool useFlexibleDuration = true,
            UnityAction<bool, float> onProgress = null,
            IEnumerator onForwardCompletedRoutine = null,
            UnityAction onRoutineFinished = null)
        {
            yield return canvasGroup.FadeAnimationRoutine(
                true,
                durationSecOnOnWay,
                startAlpha,
                endAlpha,
                useFlexibleDuration,
                alpha => onProgress?.Invoke(false, alpha)
            );

            yield return onForwardCompletedRoutine;

            yield return canvasGroup.FadeAnimationRoutine(
                false,
                durationSecOnOnWay,
                endAlpha,
                startAlpha,
                useFlexibleDuration,
                alpha => onProgress?.Invoke(true, alpha),
                onRoutineFinished
            );
        }

        public static IEnumerator FadeInAndOutRoutine(
            this CanvasGroup canvasGroup,
            float durationSecOnOnWay,
            float? startAlpha = null,
            float? endAlpha = null,
            bool useFlexibleDuration = true,
            UnityAction<bool, float> onProgress = null,
            UnityAction onForwardCompleted = null,
            UnityAction onRoutineFinished = null) => canvasGroup.FadeInAndOutRoutine(
                durationSecOnOnWay,
                startAlpha,
                endAlpha,
                useFlexibleDuration,
                onProgress,
                onForwardCompleted?.ToRoutine(),
                onRoutineFinished
            );
        //<---

        public static Coroutine StartCanvasGroupFadeCoroutine(
            this MonoBehaviour mb,
            CanvasGroup canvasGroup,
            bool isFadeIn,
            float durationSec,
            float? startAlpha = null,
            float? endAlpha = null,
            bool useFlexibleDuration = true,
            UnityAction<float> onFading = null,
            UnityAction onFinished = null)
        {
            return mb.StartCoroutine(
                canvasGroup.FadeAnimationRoutine(
                    isFadeIn,
                    durationSec,
                    startAlpha,
                    endAlpha,
                    useFlexibleDuration,
                    onFading,
                    onFinished
                )
            );
        }

        public static Coroutine StartCanvasGroupFadeInAndOutCoroutine(
            this MonoBehaviour mb,
            CanvasGroup canvasGroup,
            float durationSecOnOnWay,
            float? startAlpha = null,
            float? endAlpha = null,
            bool useFlexibleDuration = true,
            UnityAction<bool, float> onProgress = null,
            IEnumerator onForwardCompletedRoutine = null,
            UnityAction onRoutineFinished = null
        )
        {
            return mb.StartCoroutine(
                canvasGroup.FadeInAndOutRoutine(
                    durationSecOnOnWay,
                    startAlpha,
                    endAlpha,
                    useFlexibleDuration,
                    onProgress,
                    onForwardCompletedRoutine,
                    onRoutineFinished
                )
            );
        }

        public static Coroutine StartCanvasGroupFadeInAndOutCoroutine(
            this MonoBehaviour mb,
            CanvasGroup canvasGroup,
            float durationSecOnOnWay,
            float? startAlpha = null,
            float? endAlpha = null,
            bool useFlexibleDuration = true,
            UnityAction<bool, float> onProgress = null,
            UnityAction onForwardCompleted = null,
            UnityAction onRoutineFinished = null) => mb.StartCanvasGroupFadeInAndOutCoroutine(
                canvasGroup,
                durationSecOnOnWay,
                startAlpha,
                endAlpha,
                useFlexibleDuration,
                onProgress,
                onForwardCompleted?.ToRoutine(),
                onRoutineFinished
            );
        //<---

        //---> 2025/05/08 added
        public static void SetSize(
            this RectTransform rt,
            float? width = null,
            float? height = null)
        {
            var size = rt.sizeDelta;
            rt.sizeDelta = new Vector2(
                width ?? size.x,
                height ?? size.y
            );
        }
        //<---

        //---> 2025/05/09
        public static IEnumerator ToRoutine(this UnityAction action)
        {
            action();
            yield break;
        }

        public static float GetStartValue(this AnimationCurve curve) => curve.keys[0].value;
        public static float GetEndValue(this AnimationCurve curve) => curve.keys[^1].value;
        public static float GetValueRange(this AnimationCurve curve) => curve.GetEndValue() - curve.GetStartValue();

        public static float GetStartTime(this AnimationCurve curve) => curve.keys[0].time;
        public static float GetEndTime(this AnimationCurve curve) => curve.keys[^1].time;
        public static float GetDurationSec(this AnimationCurve curve) => curve.GetEndTime() - curve.GetStartTime();

        public static bool IsValidAnimation(this AnimationCurve curve) => curve.GetDurationSec() > 0f && (curve.GetStartValue() != curve.GetEndValue());

        private static readonly AnimationCurve DefaultLinearCurve = AnimationCurve.Linear(0, 0, 1, 1);
        private static readonly AnimationCurve DefaultReverseLinearCurve = AnimationCurve.Linear(0, 1, 1, 0);


        public static IEnumerator LerpRoutine(
            UnityAction<float> onProgress,
            AnimationCurve animationCurve = null,
            IEnumerator onFinishedRoutine = null)
        {
            animationCurve ??= DefaultLinearCurve;

            if (!animationCurve.IsValidAnimation())
            {
                yield break;
            }

            var startSec = animationCurve.GetStartTime();
            var endSec = animationCurve.GetEndTime();
            var durationSec = endSec - startSec;

            var startValue = animationCurve.GetStartValue();
            var endValue = animationCurve.GetEndValue();

            var ct = 0f;
            while (ct < durationSec)
            {
                float progress = animationCurve.Evaluate(Mathf.Lerp(startSec, endSec, ct / durationSec));
                onProgress?.Invoke(progress);
                ct += Time.deltaTime;
                yield return null;
            }

            onProgress?.Invoke(endValue);
            
            yield return onFinishedRoutine;
        }
        public static IEnumerator LerpRoutine(
            UnityAction<float> onProgress,
            AnimationCurve animationCurve = null,
            UnityAction onFinished = null) => LerpRoutine(
                onProgress,
                animationCurve,
                onFinished?.ToRoutine()
            );

        public static Coroutine StartLerpRoutine(
            this MonoBehaviour mb,
            UnityAction<float> onProgress,
            AnimationCurve animationCurve = null,
            IEnumerator onFinishedRoutine = null)
        {
            return mb.StartCoroutine(
                LerpRoutine(onProgress, animationCurve, onFinishedRoutine)
            );
        }

        public static Coroutine StartLerpRoutine(
            this MonoBehaviour mb,
            UnityAction<float> onProgress,
            AnimationCurve animationCurve = null,
            UnityAction onFinished = null) => mb.StartLerpRoutine(
                onProgress,
                animationCurve,
                onFinished?.ToRoutine()
            );


        /// <summary>
        /// Linear lerp routine
        /// </summary>
        /// <param name="durationSec"></param>
        /// <param name="onProgress"></param>
        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <param name="onFinished"></param>
        /// <returns></returns>
        public static IEnumerator LerpRoutine(
            float durationSec,
            UnityAction<float> onProgress,
            float startValue = 0f,
            float endValue = 1f,
            IEnumerator onFinishedRoutine = null) => LerpRoutine(
                onProgress,
                AnimationCurve.Linear(0, startValue, durationSec, endValue),
                onFinishedRoutine
            );
        public static IEnumerator LerpRoutine(
            float durationSec,
            UnityAction<float> onProgress,
            float startValue = 0f,
            float endValue = 1f,
            UnityAction onFinished = null) => LerpRoutine(
                durationSec,
                onProgress,
                startValue,
                endValue,
                onFinished?.ToRoutine()
            );
 
        
        public static Coroutine StartLerpRoutine(
            this MonoBehaviour mb,
            float durationSec,
            UnityAction<float> onProgress,
            float startValue = 0f,
            float endValue = 1f,
            IEnumerator onFinishedRoutine = null)
            {
                return mb.StartCoroutine(
                    LerpRoutine(
                        durationSec,
                        onProgress,
                        startValue,
                        endValue,
                        onFinishedRoutine
                    )
                );
            }

        public static Coroutine StartLerpRoutine(
            this MonoBehaviour mb,
            float durationSec,
            UnityAction<float> onProgress,
            float startValue = 0f,
            float endValue = 1f,
            UnityAction onFinished = null) => mb.StartLerpRoutine(
                durationSec,
                onProgress,
                startValue,
                endValue,
                onFinished?.ToRoutine()
            );

        /// <summary>
        /// Executes the ping-pong routine
        /// </summary>
        /// <param name="onProgress">first: true if backward phase, second: progress value</param>
        /// <param name="forwardAnimationCurve"></param>
        /// <param name="backwardAnimationCurve"></param>
        /// <param name="onForwardCompleted"></param>
        /// <param name="onRoutineFinished"></param>
        /// <returns></returns>
        public static IEnumerator PingPongRoutine(
            UnityAction<bool, float> onProgress,
            AnimationCurve forwardAnimationCurve = null,
            AnimationCurve backwardAnimationCurve = null,
            IEnumerator onForwardCompletedRoutine = null,
            UnityAction onRoutineFinished = null)
        {
            forwardAnimationCurve ??= DefaultLinearCurve;
            backwardAnimationCurve ??= DefaultReverseLinearCurve;

            yield return LerpRoutine(
                progress => onProgress?.Invoke(false, progress),
                forwardAnimationCurve,
                onForwardCompletedRoutine
            );

            yield return LerpRoutine(
                progress => onProgress?.Invoke(true, progress),
                backwardAnimationCurve,
                onRoutineFinished
            );
        }

        public static IEnumerator PingPongRoutine(
            UnityAction<bool, float> onProgress,
            AnimationCurve forwardAnimationCurve = null,
            AnimationCurve backwardAnimationCurve = null,
            UnityAction onForwardCompleted = null,
            UnityAction onRoutineFinished = null) => PingPongRoutine(
                onProgress,
                forwardAnimationCurve,
                backwardAnimationCurve,
                onForwardCompleted?.ToRoutine(),
                onRoutineFinished
            );


        public static Coroutine StartPingPongRoutine(
            this MonoBehaviour mb,
            UnityAction<bool, float> onProgress,
            AnimationCurve forwardAnimationCurve = null,
            AnimationCurve backwardAnimationCurve = null,
            IEnumerator onForwardCompletedRoutine = null,
            UnityAction onRoutineFinished = null)
        {
            return mb.StartCoroutine(
                PingPongRoutine(
                    onProgress,
                    forwardAnimationCurve,
                    backwardAnimationCurve,
                    onForwardCompletedRoutine,
                    onRoutineFinished
                )
            );
        }

        public static Coroutine StartPingPongRoutine(
            this MonoBehaviour mb,
            UnityAction<bool, float> onProgress,
            AnimationCurve forwardAnimationCurve = null,
            AnimationCurve backwardAnimationCurve = null,
            UnityAction onForwardCompleted = null,
            UnityAction onRoutineFinished = null) => mb.StartPingPongRoutine(
                onProgress,
                forwardAnimationCurve,
                backwardAnimationCurve,
                onForwardCompleted?.ToRoutine(),
                onRoutineFinished
            );




        public static IEnumerator PingPongRoutine(
            float durationSec,
            UnityAction<bool, float> onProgress,
            float startValue = 0f,
            float endValue = 1f,
            IEnumerator onForwardCompletedRoutine = null,
            UnityAction onRoutineFinished = null) => PingPongRoutine(
                onProgress,
                AnimationCurve.Linear(0, startValue, durationSec, endValue),
                AnimationCurve.Linear(0f, endValue, durationSec, startValue),
                onForwardCompletedRoutine,
                onRoutineFinished
            );
        
        public static IEnumerator PingPongRoutine(
            float durationSec,
            UnityAction<bool, float> onProgress,
            float startValue = 0f,
            float endValue = 1f,
            UnityAction onForwardCompleted = null,
            UnityAction onRoutineFinished = null) => PingPongRoutine(
                durationSec,
                onProgress,
                startValue,
                endValue,
                onForwardCompleted?.ToRoutine(),
                onRoutineFinished
            );
        
        
        public static Coroutine StartPingPongRoutine(
            this MonoBehaviour mb,
            float durationSec,
            UnityAction<bool, float> onProgress,
            float startValue = 0f,
            float endValue = 1f,
            IEnumerator onForwardCompletedRoutine = null,
            UnityAction onRoutineFinished = null)
            {
                return mb.StartCoroutine(
                    PingPongRoutine(
                        onProgress,
                        AnimationCurve.Linear(0, startValue, durationSec, endValue),
                        AnimationCurve.Linear(0f, endValue, durationSec, startValue),
                        onForwardCompletedRoutine,
                        onRoutineFinished
                    )
                );
            }

        public static Coroutine StartPingPongRoutine(
            this MonoBehaviour mb,
            float durationSec,
            UnityAction<bool, float> onProgress,
            float startValue = 0f,
            float endValue = 1f,
            UnityAction onForwardCompleted = null,
            UnityAction onRoutineFinished = null) => mb.StartPingPongRoutine(
                durationSec,
                onProgress,
                startValue,
                endValue,
                onForwardCompleted?.ToRoutine(),
                onRoutineFinished
            );
        //<---
    }
}