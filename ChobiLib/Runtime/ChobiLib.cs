using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

namespace Chobitech
{
    public static class ChobiLib
    {
        public static R Let<T, R>(this T t, Func<T, R> func) => func(t);

        public static T Also<T>(this T t, UnityAction<T> action) 
        {
            action(t);
            return t;
        }

        public static int LastIndex<T>(this IList<T> list) => list.Count - 1;
        public static bool IsNotEmpty<T>(this IList<T> list) => list.Count > 0;

        public static bool IsNotEmpty(this StringBuilder sb) => sb.Length > 0;


        public static List<string> ToStringList(this string s) => s.ToCharArray().Select(c => c.ToString()).ToList();

        public static string JoinToString<T>(this IList<T> list, string joint = "", Func<T, string> stringConverter = null)
        {
            var converter = stringConverter ?? new(t => t.ToString());
            var sb = new StringBuilder();

            foreach (var v in list)
            {
                if (joint != "" && sb.IsNotEmpty())
                {
                    sb.Append(joint);
                }
                sb.Append(converter(v));
            }

            return sb.ToString();
        }

        public static void ForLoopIndexed<T>(this IList<T> list, UnityAction<int, T> action, int step = 1)
        {
            for (var i = 0; i < list.Count; i += step)
            {
                action(i, list[i]);
            }
        }

        public static List<R> MapIndexed<T, R>(this IList<T> list, Func<int, T, R> converter, int step = 1, bool exceptDefaultValue = false)
        {
            var res = new List<R>();

            for (var i = 0; i < list.Count; i += step)
            {
                var v = converter(i, list[i]);
                if (!exceptDefaultValue || !v.Equals(default))
                {
                    res.Add(v);
                }
            }

            return res;
        }
        public static List<R> Map<T, R>(this IList<T> list, Func<T, R> converter, int step = 1, bool exceptDefaultValue = false) => list.MapIndexed((_, t) => converter(t), step, exceptDefaultValue);


        public static R FoldIndexed<T, R>(this IList<T> list, R initialValue, Func<int, T, R, R> func, int step = 1)
        {
            var res = initialValue;
            for (var i = 0; i < list.Count; i += step)
            {
                res = func(i, list[i], res);
            }
            return res;   
        }
        public static R Fold<T, R>(this IList<T> list, R initialValue, Func<T, R, R> func, int step = 1) => list.FoldIndexed(initialValue, (_, t, r) => func(t, r), step);


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
    }
}