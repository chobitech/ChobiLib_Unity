using UnityEngine;

namespace ChobiLib.Unity.Realm
{
    using System;
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


        protected virtual ChobiRealm ChobiRealmCreator() => new(RealmFileName, SchemeVersion, SchemeTypes, EncryptKey, ChobiRealmProcess);

        private ChobiRealm _chobiRealm;
        public virtual ChobiRealm ChobiRealm => _chobiRealm ??= ChobiRealmCreator();

        public RealmConfiguration Configuration => ChobiRealm.CreateConfiguration();

        public T With<T>(Func<Realm, T> func) => ChobiRealm.WithTransaction(func);
        public void With(UnityAction<Realm> action) => ChobiRealm.With(action);

        public T WithTransaction<T>(Func<Realm, T> func) => ChobiRealm.WithTransaction(func);
        public void WithTransaction(UnityAction<Realm> action) => ChobiRealm.WithTransaction(action);


        public string GetRealmFileFullPath() => ChobiRealm.GetRealmFileFullPath();


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
