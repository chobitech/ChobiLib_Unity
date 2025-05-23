using UnityEngine;

namespace ChobiLib.Unity
{
    public static class ChobiTransformExtension
    {
        public static void SetSize(
            this RectTransform rt,
            float? width = null,
            float? height = null)
        {
            var size = rt.sizeDelta;
            rt.sizeDelta = new Vector2(
                width ?? size.x,
                height ?? size.y
            );
        }

        public static void SetMargin(
            this RectTransform rt,
            float? left = null,
            float? top = null,
            float? right = null,
            float? bottom = null)
        {
            var leftBottom = rt.offsetMin;
            rt.offsetMin = new(
                left ?? leftBottom.x,
                bottom ?? leftBottom.y
            );

            var rightTop = rt.offsetMax;
            rt.offsetMax = new(
                -right ?? rightTop.x,
                -top ?? rightTop.y
            );
        }

        public static void LocalMoveBy(
            this Transform tr,
            float dx = 0f,
            float dy = 0f,
            float dz = 0f)
        {
            var pos = tr.localPosition;
            tr.localPosition = pos + new Vector3(dx, dy, dz);
        }

        public static void MoveBy(
            this Transform tr,
            float dx = 0f,
            float dy = 0f,
            float dz = 0f)
        {
            var pos = tr.position;
            tr.position = pos + new Vector3(dx, dy, dz);
        }

        public static void SetLocalScale(
            this Transform tr,
            float? scaleX = null,
            float? scaleY = null,
            float? scaleZ = null)
        {
            var scale = tr.localScale;
            tr.localScale = new(
                scaleX ?? scale.x,
                scaleY ?? scale.y,
                scaleZ ?? scale.z
            );
        }


        public static T FindAndGetComponent<T>(this Transform tr, string name) where T : Object
        {
            var child = tr.Find(name);
            if (child != null)
            {
                return child.GetComponent<T>();
            }
            return null;
        }
    }
}