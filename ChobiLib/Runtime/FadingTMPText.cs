using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace ChobiLib.Unity
{
    internal static class ChobiInnerTMPTextInfoExtensions
    {

        public static void SetVertexColor(this Color32[] cArr, int vIndex, Color color)
        {
            Color32 c32 = color;

            cArr[vIndex] = c32;
            cArr[vIndex + 1] = c32;
            cArr[vIndex +2] = c32;
            cArr[vIndex + 3] = c32;
        }


        public static void SetVertexAlpha(this Color32[] cArr, int vIndex, byte alpha)
        {
            cArr[vIndex].a = alpha;
            cArr[vIndex + 1].a = alpha;
            cArr[vIndex + 2].a = alpha;
            cArr[vIndex + 3].a = alpha;
        }
        public static void SetVertexAlpha(this Color32[] cArr, int vIndex, float alpha)
        {
            cArr.SetVertexAlpha(vIndex, alpha.ToByte());
        }

        public static void ChangeAlphaOfCharAt(this TMP_TextInfo ti, int index, byte alpha)
        {
            var cInfo = ti.characterInfo[index];
            if (!cInfo.isVisible)
            {
                return;
            }

            var matIndex = cInfo.materialReferenceIndex;
            var vIndex = cInfo.vertexIndex;
            var colors = ti.meshInfo[matIndex].colors32;
            
            colors.SetVertexAlpha(vIndex, alpha);
        }

        public static void ChangeAlphaOfCharAt(this TMP_TextInfo ti, int index, float alpha01)
        {
            ti.ChangeAlphaOfCharAt(index, (byte)(byte.MaxValue * alpha01));
        }

        public static void ChangeAlphaOfText(this TMP_TextInfo ti, byte alpha)
        {
            var cCount = ti.characterCount;
            for (var i = 0; i < cCount; i++)
            {
                ti.ChangeAlphaOfCharAt(i, alpha);
            }
        }

        public static void ChangeAlphaOfText(this TMP_TextInfo ti, float alpha)
        {
            ti.ChangeAlphaOfText(alpha.ToByte());
        }

    }

    [RequireComponent(typeof(TMP_Text))]
    public class FadingTMPText : MonoBehaviour
    {
        public TMP_Text TMPText { get; private set; }

        public string Text
        {
             get => TMPText.text;
             set
             {
                if (TMPText.text != value)
                {
                    return;
                }

                TMPText.text = value;

                if (TMPText.gameObject.activeInHierarchy)
                {
                    TMPText.ForceMeshUpdate();
                }
             }
        }

        private Coroutine _textFadeInCoroutine;

        private long _charCoroutineIdSeq = 0;
        private readonly Dictionary<long, Coroutine> _charsCoroutineMap = new();


        private void StopAllFadingCoroutines()
        {
            StopAllCoroutines();
            _charsCoroutineMap.Clear();
            _textFadeInCoroutine = null;
        }


        private void SetAllCharacterAlpha(TMP_TextInfo ti, float alpha)
        {
            ti.ChangeAlphaOfText(alpha);
            TMPText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        public void SetAllCharacterAlpha(float alpha)
        {
            SetAllCharacterAlpha(TMPText.textInfo, alpha);
        }


        public void CancelCharacterFading(float textAlphaOnCanceled = 1f)
        {
            StopAllFadingCoroutines();
            SetAllCharacterAlpha(textAlphaOnCanceled);
        }


        private IEnumerator InnerCharFadingRoutineAt(TMP_TextInfo ti, int charIndex, float durationSec, float startAlpha, float endAlpha)
        {
            var cInfo = ti.characterInfo[charIndex];
            var matIndex = cInfo.materialReferenceIndex;
            var vIndex = cInfo.vertexIndex;
            var colors = ti.meshInfo[matIndex].colors32;

            var sAlpha = startAlpha.ToByte();
            var eAlpha = endAlpha.ToByte();

            var ct = 0f;
            while (ct < durationSec)
            {
                ct += Time.deltaTime;
                var a = (byte)Mathf.Lerp(sAlpha, eAlpha, ct / durationSec);
                
                colors.SetVertexAlpha(vIndex, a);
                TMPText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                yield return null;
            }

            colors.SetVertexAlpha(vIndex, eAlpha);
            TMPText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        private IEnumerator WaitCharRoutine(long id, Coroutine c)
        {
            var nid = ++_charCoroutineIdSeq;
            _charsCoroutineMap[nid] = c;
            yield return c;
            _charsCoroutineMap.Remove(nid);
            _charsCoroutineMap.Remove(id);
        }


        public IEnumerator TextSequentialFadingRoutine(float durationSecPerChar, float startAlpha, float endAlpha, float startDelaySec = 0f)
        {
            TMPText.ForceMeshUpdate();

            var textInfo = TMPText.textInfo;
            var totalChars = textInfo.characterCount;

            SetAllCharacterAlpha(textInfo, startAlpha);

            WaitForSeconds waitSec = (startDelaySec > 0f) ? new WaitForSeconds(startDelaySec) : null;

            for (var i = 0; i < totalChars; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                {
                    continue;
                }

                var rId = ++_charCoroutineIdSeq;
                var routine = StartCoroutine(InnerCharFadingRoutineAt(textInfo, i, durationSecPerChar, startAlpha, endAlpha));

                if (waitSec != null)
                {
                    _charsCoroutineMap[rId] = StartCoroutine(WaitCharRoutine(rId, routine));
                    yield return waitSec;
                }
                else
                {
                    _charsCoroutineMap[rId] = routine;
                    yield return routine;
                    _charsCoroutineMap.Remove(rId);
                }
            }

            while (_charsCoroutineMap.IsNotEmpty())
            {
                yield return null;
            }
        }

        public IEnumerator CharacterFadingRoutineAt(int charIndex, float durationSec, float startAlpha, float endAlpha)
        {
            yield return InnerCharFadingRoutineAt(TMPText.textInfo, charIndex, durationSec, startAlpha, endAlpha);
        }

        void Awake()
        {
            TMPText = GetComponent<TMP_Text>();
        }


        private IEnumerator InnerTextFadingRoutine(float durationSecPerChar, float startAlpha, float endAlpha, bool pingPong = false, int loops = 1, float startDelaySec = 0f, float pingPongIntervalSec = 2f)
        {
            var times = 0;

            WaitForSeconds pingPongWait = (pingPongIntervalSec > 0) ? new(pingPongIntervalSec) : null;

            while (loops <= 0 || times < loops)
            {
                yield return TextSequentialFadingRoutine(durationSecPerChar, startAlpha, endAlpha, startDelaySec);

                if (pingPong)
                {
                    yield return pingPongWait;
                    yield return TextSequentialFadingRoutine(durationSecPerChar, endAlpha, startAlpha, startDelaySec);
                }

                times++;
            }

            _textFadeInCoroutine = null;
        }

        public void StopTextFading()
        {
            StopAllFadingCoroutines();
        }

        public void StartTextFading(float durationSecPerChar, float startAlpha, float endAlpha, bool pingPong = false, int loops = 1, float startDelaySec = 0f, float pingPongIntervalSec = 2f)
        {
            if (_textFadeInCoroutine != null)
            {
                return;
            }

            _textFadeInCoroutine = StartCoroutine(InnerTextFadingRoutine(durationSecPerChar, startAlpha, endAlpha, pingPong, loops, startDelaySec, pingPongIntervalSec));
        }
    }
}