using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using NUnit.Framework;
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

        public static IEnumerator FadeAnimationProcess(
            this CanvasGroup canvasGroup,
            bool isFadeIn,
            float durationSec,
            float? startAlpha = null,
            float? endAlpha = null,
            bool useFlexibleDuration = true,
            UnityAction<float> onFading = null,
            UnityAction onFinished = null)
        {
            var curAlpha = canvasGroup.alpha;

            var sAlpha = startAlpha ?? curAlpha;
            var eAlpha = endAlpha ?? (isFadeIn ? 1f : 0f);

            if (sAlpha == eAlpha)
            {
                yield break;
            }

            var gObj = canvasGroup.gameObject;

            gObj.SetActive(true);

            var fadeSec = useFlexibleDuration
                ? durationSec * Mathf.Clamp01(Mathf.Abs(eAlpha - sAlpha))
                : durationSec;

            var curTime = 0f;

            while (curTime < fadeSec)
            {
                var rate = curTime / fadeSec;
                canvasGroup.alpha = Mathf.Lerp(sAlpha, eAlpha, rate);
                onFading?.Invoke(rate);
                curTime += Time.deltaTime;
                yield return null;
            }

            canvasGroup.alpha = eAlpha;

            if (!isFadeIn)
            {
                gObj.SetActive(false);
            }

            onFinished?.Invoke();
        }

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
                canvasGroup.FadeAnimationProcess(
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
    }
}