using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ChobiLib.Unity
{
    public static class ChobiColorExtension
    {
        private static readonly Regex hexColorRegex = new(@"^#([0-9a-f]{3,8})$", RegexOptions.IgnoreCase);

        public static int ToColorInt(this string hex) => Math.Clamp(int.Parse(hex, System.Globalization.NumberStyles.HexNumber), 0, 255);
        public static int ToColorInt(this float f) => (int)(Mathf.Clamp01(f) * 255);
        public static float ToColorFloat(this int i) => Math.Clamp(i / 255f, 0f, 1f);
        public static float ToColorFloat(this string hex) => hex.ToColorInt().ToColorFloat();

        public static string ToHex(this int i) => i.ToString("X2");
        public static string ToHex(this float f) => f.ToColorInt().ToHex();

        public static Color ParseHexToColor(this string hex)
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

            throw new FormatException($"Invalid color hex format: \"{hex}\"");
        }

        public static bool TryParseHexToColor(this string hex, out Color color)
        {
            try
            {
                color = hex.ParseHexToColor();
                return true;
            }
            catch
            {
                color = new();
                return false;
            }
        }

        public static string ToHex(this Color color, bool addAlpha = false)
        {
            var alpha = addAlpha ? color.a.ToHex() : "";
            return $"#{alpha}{color.r.ToHex()}{color.g.ToHex()}{color.b.ToHex()}";
        }
    }
}