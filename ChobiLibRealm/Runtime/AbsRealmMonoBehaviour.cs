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

        private SynchronizationContext _mainThreadContext;

        protected virtual ChobiRealm ChobiRealmCreator() => new(RealmFileName, SchemeVersion, SchemeTypes, EncryptKey, ChobiRealmProcess);

        private ChobiRealm _chobiRealm;
        public virtual ChobiRealm ChobiRealm => _chobiRealm ??= ChobiRealmCreator();

        public RealmConfiguration Configuration => ChobiRealm.CreateConfiguration();

        public T With<T>(Func<Realm, T> func) => ChobiRealm.WithTransaction(func);
        public void With(UnityAction<Realm> action) => ChobiRealm.With(action);

        public void WithAsync<T>(Func<Realm, T> func, UnityAction<T> onFinished = null)
        {
            Task.Run(() =>
            {
                lock (ChobiRealm)
                {
                    var result = func(ChobiRealm.Realm);
                    if (onFinished != null)
                    {
                        _mainThreadContext?.Post(_ => onFinished(result), null);
                    }
                }
            });
        }

        public void WithAsync(UnityAction<Realm> action, UnityAction onFinished = null)
        {
            Task.Run(() =>
            {
                lock (ChobiRealm)
                {
                    action(ChobiRealm.Realm);
                    if (onFinished != null)
                    {
                        _mainThreadContext?.Post(_ => onFinished(), null);
                    }
                }
            });
        }

        public T WithTransaction<T>(Func<Realm, T> func) => ChobiRealm.WithTransaction(func);
        public void WithTransaction(UnityAction<Realm> action) => ChobiRealm.WithTransaction(action);

        public void WithTransactionAsync<T>(Func<Realm, T> func, UnityAction<T> onFinished = null)
        {
            Task.Run(() =>
            {
                lock (ChobiRealm)
                {
                    var result = WithTransaction(func);
                    if (onFinished != null)
                    {
                        _mainThreadContext?.Post(_ => onFinished(result), null);
                    }
                }
            });
        }
        
        public void WithTransactionAsync(UnityAction<Realm> action, UnityAction onFinished = null)
        {
            Task.Run(() =>
            {
                lock (ChobiRealm)
                {
                    WithTransaction(action);
                    if (onFinished != null)
                    {
                        _mainThreadContext?.Post(_ => onFinished(), null);
                    }
                }
            });
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
            WithTransaction(r =>
            {
                OnAppQuit?.Invoke(r);
            });
        }

        protected virtual void OnDestroy()
        {
            DisposeRealm();
        }
    }
}
