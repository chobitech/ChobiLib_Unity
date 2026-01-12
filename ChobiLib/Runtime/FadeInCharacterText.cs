using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ChobiLib.Unity
{
    internal static class ChobiInnerTMPTextInfoExtensions
    {
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

            colors[vIndex].a = alpha;
            colors[vIndex + 1].a = alpha;
            colors[vIndex + 2].a = alpha;
            colors[vIndex + 3].a = alpha;
        }

        public static void ChangeAlphaOfCharAt(this TMP_TextInfo ti, int index, float alpha01)
        {
            ti.ChangeAlphaOfCharAt(index, (byte)(byte.MaxValue * alpha01));
        }
    }

    [RequireComponent(typeof(TMP_Text))]
    public class FadeInCharacterText : MonoBehaviour
    {
        private TMP_Text _text;

        [SerializeField]
        private string text;

        public string Text
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value;
                    _text.text = value;
                }
            }
        }

        private Coroutine _textFadeInCoroutine;

        private readonly List<Coroutine> _charsCoroutine = new();

        private void SetAllCharacterAlpha(float alpha)
        {
            _text.ForceMeshUpdate();
            var textInfo = _text.textInfo;

            var bAlpha = (byte)(alpha * byte.MaxValue);

            for (var i = 0; i < textInfo.characterCount; i++)
            {
                textInfo.ChangeAlphaOfCharAt(i, bAlpha);
            }

            _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }


        public void SkipCharacterFading()
        {
            if (_charsCoroutine.IsNotEmpty())
            {
                foreach (var c in _charsCoroutine)
                {
                    StopCoroutine(c);
                }
                _charsCoroutine.Clear();
            }

            if (_textFadeInCoroutine != null)
            {
                StopCoroutine(_textFadeInCoroutine);
                _textFadeInCoroutine = null;
            }

            SetAllCharacterAlpha(1);
        }


        private IEnumerator TextFadeInRoutine(float durationSecPerChar, float startAlpha, float endAlpha)
        {
            _charsCoroutine.Clear();
            _text.ForceMeshUpdate();

            var textInfo = _text.textInfo;
            var totalChars = textInfo.characterCount;

            SetAllCharacterAlpha(0);

            for (var i = 0; i < totalChars; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                {
                    continue;
                }
                var coroutine = StartCoroutine(FadeCharacter(i, durationSecPerChar, startAlpha, endAlpha));
                _charsCoroutine.Add(coroutine);
                yield return coroutine;
                _charsCoroutine.Remove(coroutine);
            }

            _textFadeInCoroutine = null;
        }

        private IEnumerator FadeCharacter(int charIndex, float durationSec, float startAlpha, float endAlpha)
        {
            var textInfo = _text.textInfo;

            var cInfo = textInfo.characterInfo[charIndex];

            var matIndex = cInfo.materialReferenceIndex;
            var vIndex = cInfo.vertexIndex;
            var colors = textInfo.meshInfo[matIndex].colors32;

            var ct = 0f;
            while (ct < durationSec)
            {
                ct += Time.deltaTime;
                var a = (byte)(Mathf.Lerp(startAlpha, endAlpha, ct / durationSec) * 255);
                colors[vIndex].a = a;
                colors[vIndex + 1].a = a;
                colors[vIndex + 2].a = a;
                colors[vIndex + 3].a = a;
                _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                yield return null;
            }
        }

        public void StartCharacterFading(float durationSecPerChar = 0.2f, float startAlpha = 0f, float endAlpha = 1f)
        {
            StartCharacterFading(null, durationSecPerChar);
        }

        public void StartCharacterFading(string text, float durationSecPerChar = 0.2f, float startAlpha = 0f, float endAlpha = 1f)
        {
            if (_textFadeInCoroutine != null)
            {
                StopCoroutine(_textFadeInCoroutine);
                _textFadeInCoroutine = null;
            }

            if (text != null && text.IsNotEmpty())
            {
                _text.text = text;
            }

            _textFadeInCoroutine = StartCoroutine(TextFadeInRoutine(durationSecPerChar, startAlpha, endAlpha));
        }


        void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }
    }
}