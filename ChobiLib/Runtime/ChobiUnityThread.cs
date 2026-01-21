using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace ChobiLib.Unity
{
    public static class ChobiUnityThread
    {
        private static int _mainThreadId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public static bool IsInMainThread => Thread.CurrentThread.ManagedThreadId == _mainThreadId;

        private static void CheckCancelToken(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
        }


        //--- run in main thread
        public static async Task<T> RunOnMainThreadAsync<T>(
            this Task task,
            Func<Task<T>> asyncFunc,
            CancellationToken token = default
        )
        {
            if (IsInMainThread)
            {
                CheckCancelToken(token);
                return await asyncFunc();
            }
            else
            {
                await Awaitable.MainThreadAsync();

                try
                {
                    CheckCancelToken(token);
                    return await asyncFunc();
                }
                finally
                {
                    await Awaitable.BackgroundThreadAsync();
                }
            }
        }


        public static async Task RunOnMainThreadAsync(
            this Task task,
            Func<Task> asyncFunc,
            CancellationToken token = default
        )
        {
            _ = await task.RunOnMainThreadAsync<bool>(async () =>
            {
                await asyncFunc();
                return false;
            }, token);
        }

        public static async Task<T> RunOnMainThreadAsync<T>(
            this Task task,
            Func<T> func,
            CancellationToken token = default
        ) => await task.RunOnMainThreadAsync(async () => func(), token);
        
        public static async Task RunOnMainThreadAsync(
            this Task task,
            UnityAction action,
            CancellationToken token = default
        ) => await task.RunOnMainThreadAsync(async () => action(), token);
        
        public static Task<T> RunOnMainThreadAsync<T>(Func<Task<T>> asyncFunc, CancellationToken token = default)
            => Task.CompletedTask.RunOnMainThreadAsync(asyncFunc, token);
        
        public static Task RunOnMainThreadAsync(Func<Task> asyncFunc, CancellationToken token = default)
            => Task.CompletedTask.RunOnMainThreadAsync(asyncFunc, token);

        public static Task<T> RunOnMainThreadAsync<T>(Func<T> func, CancellationToken token = default)
            => RunOnMainThreadAsync(async () => func(), token);
        public static Task RunOnMainThreadAsync(UnityAction action, CancellationToken token = default)
            => RunOnMainThreadAsync(async () => action(), token);


        //--- run on background thread
        public static async Task<T> RunOnBackgroundThreadAsync<T>(
            this Task task,
            Func<Task<T>> asyncFunc,
            CancellationToken token = default
        )
        {
            if (IsInMainThread)
            {
                await Awaitable.BackgroundThreadAsync();
                
                try
                {
                    CheckCancelToken(token);
                    return await asyncFunc();
                }
                finally
                {
                    await Awaitable.MainThreadAsync();
                }
            }
            else
            {
                CheckCancelToken(token);
                return await asyncFunc();
            }
        }
        
        public static async Task RunOnBackgroundThreadAsync(
            this Task task,
            Func<Task> asyncTask,
            CancellationToken token = default
        )
        {
            _ = await task.RunOnBackgroundThreadAsync<bool>(async () =>
            {
                await asyncTask();
                return false;
            }, token);
        }

        public static async Task<T> RunOnBackgroundThreadAsync<T>(this Task task, Func<T> func, CancellationToken token = default)
            => await task.RunOnBackgroundThreadAsync(async () => func(), token);

        public static async Task RunOnBackgroundThreadAsync(this Task task, UnityAction action, CancellationToken token = default)
            => await task.RunOnBackgroundThreadAsync(async () => action(), token);

        public static Task<T> RunOnBackgroundThreadAsync<T>(Func<Task<T>> asyncFunc, CancellationToken token = default)
            => Task.CompletedTask.RunOnBackgroundThreadAsync(asyncFunc, token);
        
        public static Task RunOnBackgroundThreadAsync(Func<Task> asyncFunc, CancellationToken token = default)
            => Task.CompletedTask.RunOnBackgroundThreadAsync(asyncFunc, token);
        
        public static Task<T> RunOnBackgroundThreadAsync<T>(Func<T> func, CancellationToken token = default)
            => RunOnBackgroundThreadAsync(async () => func(), token);
        public static Task RunOnBackgroundThreadAsync(UnityAction action, CancellationToken token = default)
            => RunOnBackgroundThreadAsync(async () => action(), token);
    }
}