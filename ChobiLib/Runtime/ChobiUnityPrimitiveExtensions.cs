
using UnityEngine;

namespace ChobiLib.Unity
{
    public static class ChobiUnityPrimitiveExtensions
    {
        public static byte ToByte(this float f)
        {
            return (byte)(byte.MaxValue * Mathf.Clamp01(f));
        }

        public static float ToFloat(this byte b)
        {
            return (float)b / byte.MaxValue;
        }

        public static int ToInt(this bool b) => b ? 1 : 0;
    }
}