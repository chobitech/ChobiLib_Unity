using UnityEngine;

namespace ChobiLib.Unity
{
    public static class ChobiSpriteRendererExtension
    {
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
    }
}