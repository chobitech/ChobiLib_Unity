using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Codice.Client.BaseCommands.BranchExplorer;
using Codice.CM.Common.Tree;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

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



        public static Coroutine DelayStartCoroutine(this MonoBehaviour mb, WaitForSeconds waitForSeconds, IEnumerator routine)
        {
            return mb.StartCoroutine(DelayRoutine.RunAfterDelay(waitForSeconds, routine));
        }
        public static Coroutine DelayStartCoroutine(this MonoBehaviour mb, float waitSeconds, IEnumerator routine) => mb.DelayStartCoroutine(new WaitForSeconds(waitSeconds), routine);
        public static Coroutine DelayStartCoroutine(this MonoBehaviour mb, WaitForSeconds waitForSeconds, UnityAction action) => mb.DelayStartCoroutine(waitForSeconds, action.ToRoutine());
        public static Coroutine DelayStartCoroutine(this MonoBehaviour mb, float waitSeconds, UnityAction action) => mb.DelayStartCoroutine(new WaitForSeconds(waitSeconds), action);

        public static Task RunCoroutineAsTask(this MonoBehaviour mb, IEnumerator proc, CancellationToken token = default)
        {
            var cToken = (token != default) ? token : mb.destroyCancellationToken;

            var tcs = new TaskCompletionSource<bool>();

            if (!mb.gameObject.activeInHierarchy)
            {
                tcs.TrySetResult(false);
                return tcs.Task;
            }

            mb.StartCoroutine(mb.CoroutineToTaskWrapper(proc, tcs, cToken));
            return tcs.Task;
        }


        private static IEnumerator CoroutineToTaskWrapper(this MonoBehaviour mb, IEnumerator proc, TaskCompletionSource<bool> tcs, CancellationToken token = default)
        {
            using (token.Register(() => tcs.TrySetCanceled(token)))
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        tcs.TrySetCanceled(token);
                        yield break;
                    }

                    if (mb == null)
                    {
                        tcs.TrySetException(new MissingReferenceException($"MonoBehaviour is null"));
                        yield break;
                    }

                    if (!mb.isActiveAndEnabled)
                    {
                        tcs.TrySetException(new InvalidOperationException("MonoBehaviour is not active"));
                        yield break;
                    }

                    bool hasNext;

                    try
                    {
                        hasNext = proc.MoveNext();
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                        yield break;
                    }

                    if (!hasNext)
                    {
                        break;
                    }

                    yield return proc.Current;
                }

                tcs.TrySetResult(true);
            }

        }




        public static async Task RunWithCancellationTokens(this MonoBehaviour mb, Func<CancellationToken, Task> proc, UnityAction<CancellationToken> onCanceled, params CancellationToken[] customTokens)
        {
            var dToken = mb.destroyCancellationToken;

            CancellationTokenSource cts = null;

            if (customTokens.IsNotEmpty())
            {
                var tokenList = new List<CancellationToken>()
                {
                    dToken
                };
                tokenList.AddRange(customTokens);
                cts = CancellationTokenSource.CreateLinkedTokenSource(tokenList.ToArray());
            }

            var procToken = cts?.Token ?? dToken;

            try
            {
                await (proc?.Invoke(procToken) ?? Task.CompletedTask);
            }
            catch (OperationCanceledException)
            {
                if (mb == null || dToken.IsCancellationRequested)
                {
                    return;
                }

                foreach (var token in customTokens)
                {
                    if (token.IsCancellationRequested)
                    {
                        onCanceled?.Invoke(token);
                        break;
                    }
                }
            }
            finally
            {
                cts?.Dispose();
            }
        }

        public static async Task RunWithCancellationTokens(this MonoBehaviour mb, Func<CancellationToken, Task> proc, params CancellationToken[] customTokens)
        {
            await mb.RunWithCancellationTokens(proc, null, customTokens);
        }


        public static async Task<T> RunOnMainThread<T>(this object obj, Func<Task<T>> asyncProc)
        {
            await Awaitable.MainThreadAsync();
            try
            {
                return await asyncProc();
            }
            finally
            {
                await Awaitable.BackgroundThreadAsync();
            }
        }

        public static async Task RunOnMainThread(this object obj, Func<Task> asyncProc)
        {
            _ = await obj.RunOnMainThread(async () =>
            {
                await asyncProc();
                return false;
            });
        }

    }
}