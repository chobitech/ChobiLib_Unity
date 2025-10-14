
/*
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Events;

public sealed class AsyncableObjectLocker<T>
{
    private T _obj;
    private readonly AsyncLock _lock = new();

    public AsyncableObjectLocker(T obj)
    {
        _obj = obj;
    }

    public R With<R>(Func<T, R> func)
    {
        using (_lock.Lock())
        {
            return func(_obj);
        }
    }

    public void With(UnityAction<T> action) => With<object>(o =>
    {
        action(o);
        return null;
    });


    public void WithAsync<R>(Func<T, Task<R>> asyncFunc, UnityAction<R> onFinished = null, SynchronizationContext onFinishedContext = null)
    {
        Task.Run(async () =>
        {
            using (await _lock.LockAsync())
            {
                var res = await asyncFunc(_obj);
                if (onFinished != null)
                {
                    if (onFinishedContext != null)
                    {
                        onFinishedContext.Post(_ => onFinished(res), null);
                    }
                    else
                    {
                        onFinished(res);
                    }
                }
            }
        });
    }

    public void WithAsync(Func<T, Task> asyncFunc, UnityAction onFinished = null, SynchronizationContext onFinishedContext = null)
    {
        WithAsync<object>(
            async o =>
            {
                await asyncFunc(o);
                return null;
            },
            (onFinished != null) ? _ => onFinished() : null,
            onFinishedContext
        );
    }
}
*/
