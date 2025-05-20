using UnityEngine;
using UnityEngine.Events;

namespace ChobiLib.Unity
{
    public static class ChobiMaterialPropertyBlockExtension
    {
        public static void SetBool(this MaterialPropertyBlock mpb, string nameId, bool b) => mpb.SetFloat(nameId, b ? 1 : 0);

        public static bool GetBool(this MaterialPropertyBlock mpb, string nameId) => mpb.GetFloat(nameId) > 0;

        private static bool SetMpbValueItUpdated<T>(T current, T updated, UnityAction<T> setter) where T : struct
        {
            if (!updated.Equals(current))
            {
                setter(updated);
                return true;
            }
            return false;
        }

        public static bool SetColorIfUpdated(this MaterialPropertyBlock mpb, string nameId, Color current, Color updated) => SetMpbValueItUpdated(current, updated, (c) => mpb.SetColor(nameId, c));
        public static bool SetVectorIfUpdated(this MaterialPropertyBlock mpb, string nameId, Vector4 current, Vector4 updated) => SetMpbValueItUpdated(current, updated, v => mpb.SetVector(nameId, v));
        public static bool SetFloatIfUpdated(this MaterialPropertyBlock mpb, string nameId, float current, float updated) => SetMpbValueItUpdated(current, updated, f => mpb.SetFloat(nameId, f));
        public static bool SetIntIfUpdated(this MaterialPropertyBlock mpb, string nameId, int current, int updated) => SetMpbValueItUpdated(current, updated, i => mpb.SetInteger(nameId, i));
        public static bool SetBoolIfUpdated(this MaterialPropertyBlock mpb, string nameId, bool current, bool updated) => SetMpbValueItUpdated(current, updated, b => mpb.SetBool(nameId, b));

        private static bool SetMpbValueIfNotNull<T>(T? value, UnityAction<T> setter) where T : struct
        {
            if (value != null)
            {
                setter(value.Value);
                return true;
            }
            return false;
        }

        public static bool SetColorIfNotNull(this MaterialPropertyBlock mpb, string nameId, Color? color) => SetMpbValueIfNotNull(color, c => mpb.SetColor(nameId, c));
        public static bool SetVectorIfNotNull(this MaterialPropertyBlock mpb, string nameId, Vector4? vector) => SetMpbValueIfNotNull(vector, v => mpb.SetVector(nameId, v));
        public static bool SetFloatIfNotNull(this MaterialPropertyBlock mpb, string nameId, float? value) => SetMpbValueIfNotNull(value, f => mpb.SetFloat(nameId, f));
        public static bool SetIntIfNotNull(this MaterialPropertyBlock mpb, string nameId, int? value) => SetMpbValueIfNotNull(value, i => mpb.SetInteger(nameId, i));
        public static bool SetBoolIfNotNull(this MaterialPropertyBlock mpb, string nameId, bool? value) => SetMpbValueIfNotNull(value, b => mpb.SetBool(nameId, b));
    }
}