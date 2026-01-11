using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ChobiLib.Unity
{
    public static class ChobiVertexHelperExtensions
    {
        public static void UpdateVerticesInTriangle(this VertexHelper vh, Func<int, UIVertex, UIVertex> updater)
        {
            var vList = new List<UIVertex>();
            vh.GetUIVertexStream(vList);

            for (var i = 0; i < vList.Count; i++)
            {
                var v = vList[i];
                v = updater(i, vList[i]);
                vList[i] = v;
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(vList);
        }
    }
}