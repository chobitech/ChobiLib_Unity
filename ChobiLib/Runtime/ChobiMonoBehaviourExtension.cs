using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace ChobiLib.Unity
{
    public static class ChobiMonoBehaviourExtension
    {
        public static Coroutine StartLerpRun(this MonoBehaviour mb, LerpRoutine lerpRoutine, UnityAction<float> onProgress, UnityAction onProcessCompleted = null)
        {
            if (lerpRoutine == null)
            {
                return null;
            }

            return mb.StartCoroutine(
                    lerpRoutine.Run(onProgress, onProcessCompleted)
                );
        }

        public static Coroutine StartLerpRun(
            this MonoBehaviour mb,
            float startValue,
            float endValue,
            float durationSec,
            UnityAction<float> onProgress,
            UnityAction onProcessCompleted = null
        ) => mb.StartLerpRun(
            new LerpRoutine(startValue, endValue, durationSec),
            onProgress,
            onProcessCompleted
        );

        public static Coroutine StartPingPong(
            this MonoBehaviour mb,
            LerpRoutine lerpRoutine,
            UnityAction<bool, float> onProgress,
            int repeatCount = 1,
            IEnumerator routineOnForwardCompleted = null,
            UnityAction<int> onPingPongCompleted = null)
        {
            if (lerpRoutine == null)
            {
                return null;
            }

            return mb.StartCoroutine(
                    lerpRoutine.RunPingPong(
                        onProgress,
                        repeatCount,
                        routineOnForwardCompleted,
                        onPingPongCompleted
                    )
                );
        }

        public static Coroutine StartPingPong(
            this MonoBehaviour mb,
            LerpRoutine lerpRoutine,
            UnityAction<bool, float> onProgress,
            int repeatCount = 1,
            UnityAction onForwardCompleted = null,
            UnityAction<int> onPingPongCompleted = null
        ) => mb.StartPingPong(
            lerpRoutine,
            onProgress,
            repeatCount,
            onForwardCompleted?.ToRoutine(),
            onPingPongCompleted
        );

        public static Coroutine StartPingPong(
            this MonoBehaviour mb,
            UnityAction<bool, float> onProgress,
            float startValue = 0f,
            float endValue = 1f,
            float durationSec = 1f,
            int repeatCount = 1,
            IEnumerator routineOnForwardCompleted = null,
            UnityAction<int> onPingPongCompleted = null
        ) => mb.StartPingPong(
            new LerpRoutine(startValue, endValue, durationSec),
            onProgress,
            repeatCount,
            routineOnForwardCompleted,
            onPingPongCompleted
        );

        public static Coroutine StartPingPong(
            this MonoBehaviour mb,
            UnityAction<bool, float> onProgress,
            float startValue = 0f,
            float endValue = 1f,
            float durationSec = 1f,
            int repeatCount = 1,
            UnityAction onForwardCompleted = null,
            UnityAction<int> onPingPongCompleted = null
        ) => mb.StartPingPong(
            onProgress,
            startValue,
            endValue,
            durationSec,
            repeatCount,
            onForwardCompleted?.ToRoutine(),
            onPingPongCompleted
        );
    }
}