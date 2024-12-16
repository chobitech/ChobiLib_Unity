using System;
using System.Collections;
using System.Collections.Generic;
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
    }
}