using UnityEngine;


namespace Chobitech.Realm
{
    using System;
    using Realms;
    using UnityEngine.Events;

    public abstract class AbsRealmMonoBehaviour : MonoBehaviour
    {
        public delegate void OnAppQuitEventHandler(Realm realm);
        public event OnAppQuitEventHandler OnAppQuit;

        public abstract string RealmFileName { get; }
        public virtual ulong SchemeVersion => 1;
        public virtual Type[] SchemeTypes => null;
        public virtual byte[] EncryptKey => null;
        public virtual ChobiRealm.IChobiRealmProcess ChobiRealmProcess => null;

        private ChobiRealm _chobiRealm;
        public virtual ChobiRealm ChobiRealm => _chobiRealm ??= new(RealmFileName, SchemeVersion, SchemeTypes, EncryptKey, ChobiRealmProcess);

        public RealmConfiguration Configuration => ChobiRealm.Configuration;

        public T With<T>(Func<Realm, T> func) => ChobiRealm.WithTransaction(func);
        public void With(UnityAction<Realm> action) => ChobiRealm.With(action);

        public T WithTransaction<T>(Func<Realm, T> func) => ChobiRealm.WithTransaction(func);
        public void WithTransaction(UnityAction<Realm> action) => ChobiRealm.WithTransaction(action);


        public virtual void DeleteAllRealm()
        {
            _chobiRealm?.DeleteAllRealm();
        }

        public virtual void DisposeRealm()
        {
            _chobiRealm?.Dispose();
            _chobiRealm = null;
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



        /*
        public virtual string RealmFileFullPath => Path.Join(Application.persistentDataPath, RealmFileName);


        private Realm _realm;
        public Realm Realm
        {
            get
            {
                _realm ??= Realm.GetInstance(Configuration);
                return _realm;
            }
        }

        public virtual RealmConfiguration CreateConfiguration()
        {
            return ((IChobiRealm)this).CreateConfiguration();
        }



        private RealmConfiguration _configuration;
        public RealmConfiguration Configuration
        {
            get
            {
                _configuration ??= CreateConfiguration();
                return _configuration;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
        */
    }

    /*
    public abstract class AbsRealmMonoBehaviour : MonoBehaviour
    {
        public delegate void OnAppQuitEventHandler(Realm realm);
        
        public event OnAppQuitEventHandler OnAppQuit;


        public abstract string RealmFileName { get; }

        public virtual string GetRealmFileFullPath() => Path.Join(Application.persistentDataPath, RealmFileName);
        
        public virtual ulong SchemeVersion { get; } = 1;
        public virtual Type[] SchemeTypes { get; } = null;

        //---> 2025/04/05 added
        protected virtual byte[] GetEncryptionKey() => null;
        //<---

        protected virtual RealmConfiguration CreateConfiguration()
        {
            var config = new RealmConfiguration(GetRealmFileFullPath())
            {
                SchemaVersion = SchemeVersion,
            };

            //---> 2025/04/05 added
            var encKey = GetEncryptionKey();
            if (encKey != null && encKey.Length == 64)
            {
                config.EncryptionKey = encKey;
            }
            //<---

            if (SchemeTypes != null && SchemeTypes.Length > 0)
            {
                var sBuilder = new RealmSchema.Builder();
                var roType = typeof(RealmObject);

                foreach (var t in SchemeTypes)
                {
                    if (t.IsSubclassOf(roType))
                    {
                        sBuilder.Add(t);
                    }
                }

                if (sBuilder.Count > 0)
                {
                    config.Schema = sBuilder.Build();
                }
            }

            return config;
        }


        private RealmConfiguration configuration;
        public RealmConfiguration Configuration
        {
            get
            {
                configuration ??= CreateConfiguration();
                return configuration;
            }
        }


        private Realm realm;
        public Realm Realm
        {
            get
            {
                realm ??= Realm.GetInstance(Configuration);
                return realm;
            }
        }

        public T With<T>(Func<Realm, T> func) => func(Realm);
        public void With(UnityAction<Realm> action) => action(Realm);

        public T WithTransaction<T>(Func<Realm, T> func) => Realm.WithTransaction(func);
        public void WithTransaction(UnityAction<Realm> action) => Realm.WithTransaction(action);

        public void DeleteAllRealm()
        {
            DisposeRealm();
            Realm.DeleteRealm(Configuration);
        }


        public void DisposeRealm()
        {
            realm?.Dispose();
            realm = null;
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
    */

}
