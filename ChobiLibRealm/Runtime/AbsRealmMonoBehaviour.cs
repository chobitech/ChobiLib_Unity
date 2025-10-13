using UnityEngine;

namespace ChobiLib.Unity.Realm
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Realms;
    using UnityEngine.Events;

    public abstract class AbsRealmMonoBehaviour : MonoBehaviour
    {
        public delegate void OnAppQuitEventHandler(Realm realm);
        public event OnAppQuitEventHandler OnAppQuit;

        public virtual string RealmFileName { get; }
        public virtual ulong SchemeVersion => 1;
        public virtual Type[] SchemeTypes => null;
        public virtual byte[] EncryptKey => null;
        public virtual ChobiRealm.IChobiRealmProcess ChobiRealmProcess => null;


        //--- for async
        private SynchronizationContext _mainThreadContext;
        private readonly SemaphoreSlim _mutex = new(1, 1);
        private async Task MutexProcess<T>(Func<Realm, Task<T>> proc, UnityAction<T> onFinished)
        {
            await _mutex.WaitAsync();
            
            T result;

            try
            {
                result = await proc(ChobiRealm.Realm);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _mutex.Release();
            }

            if (onFinished != null)
            {
                _mainThreadContext?.Post(_ => onFinished(result), null);
            }
        }


        protected virtual ChobiRealm ChobiRealmCreator() => new(RealmFileName, SchemeVersion, SchemeTypes, EncryptKey, ChobiRealmProcess);

        private ChobiRealm _chobiRealm;
        public virtual ChobiRealm ChobiRealm => _chobiRealm ??= ChobiRealmCreator();

        public RealmConfiguration Configuration => ChobiRealm.CreateConfiguration();

        /*
        public T With<T>(Func<Realm, T> func) => ChobiRealm.WithTransaction(func);
        public void With(UnityAction<Realm> action) => ChobiRealm.With(action);
        */

        public void WithAsync<T>(Func<Realm, T> func, UnityAction<T> onFinished = null)
        {
            Task.Run(async () =>
            {
                await MutexProcess(r => Task.Run(async () => func(ChobiRealm.Realm)), onFinished);
            });
        }

        public void WithAsync(UnityAction<Realm> action, UnityAction onFinished = null)
        {
            Task.Run(async () =>
            {
                await MutexProcess<object>(
                r =>
                {
                    action(r);
                    return null;
                },
                (onFinished != null) ? _ => onFinished() : null
                );
            });
        }

        
        public T WithTransaction<T>(Func<Realm, T> func) => ChobiRealm.WithTransaction(func);
        public void WithTransaction(UnityAction<Realm> action) => ChobiRealm.WithTransaction(action);

        private async Task InnerWithTransactionAsync<T>(Func<Realm, Task<T>> func, UnityAction<T> onFinished)
        {
            await MutexProcess<T>(
                async r =>
                {
                    if (r.IsInTransaction)
                    {
                        return await func(r);
                    }

                    using var tr = await r.BeginWriteAsync();

                    try
                    {
                        var result = await func(r);
                        await tr.CommitAsync();
                        return result;
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        throw ex;
                    }
                },
                onFinished
            );
        }

        public async void WithTransactionAsync<T>(Func<Realm, Task<T>> func, UnityAction<T> onFinished = null)
        {
            await InnerWithTransactionAsync(func, onFinished);
        }
        
        public async void WithTransactionAsync(Func<Realm, Task> action, UnityAction onFinished = null)
        {
            await InnerWithTransactionAsync<object>(
                async r =>
                {
                    await action(r);
                    return null;
                },
                (onFinished != null) ? _ => onFinished() : null
            );
        }


        public string GetRealmFileFullPath() => ChobiRealm.GetRealmFileFullPath();

        protected virtual void Awake()
        {
            _mainThreadContext = SynchronizationContext.Current;
        }


        public virtual void DeleteAllRealm()
        {
            ChobiRealm.DeleteAllRealm();
        }

        public virtual void DisposeRealm()
        {
            ChobiRealm.Dispose();
        }

        protected virtual void OnApplicationQuit()
        {
            _mutex.Wait();

            try
            {
                WithTransaction(r =>
                {
                    OnAppQuit?.Invoke(r);
                });
            }
            finally
            {
                _mutex.Release();
            }
        }

        protected virtual void OnDestroy()
        {
            DisposeRealm();
        }
    }
}
