using TMPro;
using UnityEngine;
using System.Collections;

namespace ChobiLib.Unity
{
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

        private void SetAllCharacterAlpha(float alpha)
        {
            _text.ForceMeshUpdate();
            var textInfo = _text.textInfo;

            var bAlpha = (byte)(alpha * byte.MaxValue);

            for (var i = 0; i < textInfo.characterCount; i++)
            {
                var cInfo = textInfo.characterInfo[i];

                if (!cInfo.isVisible)
                {
                    continue;
                }

                var matIndex = cInfo.materialReferenceIndex;
                var vIndex = cInfo.vertexIndex;
                var colors = textInfo.meshInfo[matIndex].colors32;
                for (var k = 0; k < 4; k++)
                {
                    colors[vIndex + k].a = bAlpha;
                }
            }

            _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }


        public void SkipCharacterFading()
        {
            if (_textFadeInCoroutine != null)
            {
                StopCoroutine(_textFadeInCoroutine);
                _textFadeInCoroutine = null;
            }

            SetAllCharacterAlpha(1);
        }


        private IEnumerator TextFadeInRoutine(float durationSecPerChar)
        {
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
                yield return StartCoroutine(FadeCharacter(i, durationSecPerChar));
            }

            _textFadeInCoroutine = null;
        }

        private IEnumerator FadeCharacter(int charIndex, float durationSec)
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
                var a = (byte)Mathf.Lerp(0, 255, ct / durationSec);
                colors[vIndex].a = a;
                colors[vIndex + 1].a = a;
                colors[vIndex + 2].a = a;
                colors[vIndex + 3].a = a;
                _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                yield return null;
            }
        }

        public void StartCharacterFading(float durationSecPerChar = 0.2f)
        {
            StartCharacterFading(null, durationSecPerChar);
        }

        public void StartCharacterFading(string text, float durationSecPerChar = 0.2f)
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

            _textFadeInCoroutine = StartCoroutine(TextFadeInRoutine(durationSecPerChar));
        }


        void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }
    }
}