using System.Collections.Generic;
using UnityEngine;

namespace Chobitech.Realm
{
    using System;
    using System.IO;
    using Realms;
    using Realms.Schema;
    using UnityEngine.Events;

    public abstract class AbsRealmMonoBehaviour : MonoBehaviour
    {
        public delegate void OnAppQuitEventHandler(Realm realm);
        
        public event OnAppQuitEventHandler OnAppQuit;


        public abstract string RealmFileName { get; }

        public virtual string GetRealmFileFullPath() => Path.Join(Application.persistentDataPath, RealmFileName);
        
        public virtual ulong SchemeVersion { get; } = 1;
        public virtual Type[] SchemeTypes { get; } = null;

        protected virtual RealmConfiguration CreateConfiguration()
        {
            var config = new RealmConfiguration(GetRealmFileFullPath())
            {
                SchemaVersion = SchemeVersion,
            };

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

}
