using System.Linq;
using UnityEngine;

namespace ChobiLib.Unity
{
    public static class ChobiAnimationExtension
    {
        public static float GetStartValue(this AnimationCurve curve) => curve.keys[0].value;
        public static float GetEndValue(this AnimationCurve curve) => curve.keys[^1].value;
        public static float GetValueGap(this AnimationCurve curve) => curve.GetEndValue() - curve.GetStartValue();

        public static float GetStartTime(this AnimationCurve curve) => curve.keys[0].time;
        public static float GetEndTime(this AnimationCurve curve) => curve.keys[^1].time;
        public static float GetDurationSec(this AnimationCurve curve) => curve.GetEndTime() - curve.GetStartTime();

        public static bool IsValidAnimation(this AnimationCurve curve) => curve.GetDurationSec() > 0f && (curve.GetStartValue() != curve.GetEndValue());


        public static AnimationCurve GetReverseAnimationCurve(this AnimationCurve curve)
        {
            var curKeys = curve.keys;
            var sTime = curve.GetStartTime();
            var eTime = curve.GetEndTime();

            var revKeys = curKeys
                .Select(kf => new Keyframe(eTime - (kf.time - sTime), kf.value))
                .Reverse()
                .ToArray();

            return new AnimationCurve(revKeys);
        }
    }
}