namespace ChobiLib.Unity.Realm
{
    using System;
    using System.IO;
    using Realms;
    using Realms.Schema;
    using UnityEngine;
    using UnityEngine.Events;

    public class DefaultChobiRealmProcess : ChobiRealm.IChobiRealmProcess
    {
        public string RealmFileName { get; }
        public ulong SchemeVersion { get; } = 1;
        public Type[] SchemeTypes { get; } = null;
        public byte[] EncryptKey { get; } = null;

        public DefaultChobiRealmProcess(string realmFileName, ulong schemeVersion, Type[] schemeTypes, byte[] encryptKey)
        {
            RealmFileName = realmFileName;
            SchemeVersion = schemeVersion;
            SchemeTypes = schemeTypes;
            EncryptKey = encryptKey;

        }


        private Realm _realm;
        public virtual Realm Realm => _realm ??= Realm.GetInstance(CreateConfiguration());

        private RealmConfiguration _configuration;

        public virtual RealmConfiguration CreateConfiguration()
        {
            return _configuration ??= new RealmConfiguration(GetRealmFileFullPath(RealmFileName))
            {
                SchemaVersion = SchemeVersion,
            }.Also(config =>
            {
                if (EncryptKey != null)
                {
                    config.EncryptionKey = EncryptKey;
                }

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
            });
        }

        public void DeleteAllRealm()
        {
            Dispose();
            Realm.DeleteRealm(CreateConfiguration());
        }

        public void Dispose()
        {
            _realm?.Dispose();
            _realm = null;
        }

        public virtual string GetRealmFileFullPath(string realmFileName)
        {
            return Path.Join(Application.persistentDataPath, realmFileName);
        }

        public T With<T>(Func<Realm, T> func) => func(Realm);

        public void With(UnityAction<Realm> action) => action(Realm);

        public T WithTransaction<T>(Func<Realm, T> func) => Realm.WithTransaction(func);
        public void WithTransaction(UnityAction<Realm> action) => Realm.WithTransaction(action);
    }
}