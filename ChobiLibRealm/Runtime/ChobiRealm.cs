namespace Chobitech.Realm
{
    using System;
    using System.IO;
    using Realms;
    using Realms.Schema;
    using System.Security.Cryptography;
    using UnityEngine;
    using UnityEngine.Events;

    public class ChobiRealm : ChobiRealm.IChobiRealmProcess
    {
        public const int RealmEncryptionKeyBytes = 64;

        public static byte[] LoadRealmEncryptionKey(string filePath, bool createNewIfNotExists = true)
        {
            byte[] data = null;

            try {
                if (File.Exists(filePath))
                {
                    data = File.ReadAllBytes(filePath);
                }
                else
                {
                    if (createNewIfNotExists)
                    {
                        data = GenerateRealmEncryptionKey();
                        File.WriteAllBytes(filePath, data);
                    }
                }
            }
            catch (Exception ex)
            {
                data = null;
            }

            return data;
        }

        public static byte[] GenerateRealmEncryptionKey()
        {
            var key = new byte[RealmEncryptionKeyBytes];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return key;
        }


        public interface IChobiRealmProcess : IDisposable
        {
            string RealmFileName { get; }

            string GetRealmFileFullPath();

            RealmConfiguration CreateConfiguration();
            T With<T>(Func<Realm, T> func);
            void With(UnityAction<Realm> action);

            T WithTransaction<T>(Func<Realm, T> func);
            void WithTransaction(UnityAction<Realm> action);

            void DeleteAllRealm();
        }

        public string RealmFileName { get; private set; }

        public readonly ulong schemeVersion;
        public readonly Type[] schemeTypes;
        public readonly byte[] encryptKey;

        public readonly IChobiRealmProcess process;

        private RealmConfiguration _configuration;
        public RealmConfiguration Configuration => _configuration ??= CreateConfiguration();

        private Realm _realm;
        public Realm Realm => _realm ??= Realm.GetInstance(Configuration);

        public ChobiRealm(
            string realmFileName,
            ulong schemeVersion = 1,
            Type[] schemeTypes = null,
            byte[] encryptKey = null,
            IChobiRealmProcess process = null
        )
        {
            RealmFileName = realmFileName;
            this.schemeVersion = schemeVersion;
            this.schemeTypes = schemeTypes;
            this.encryptKey = encryptKey;
            this.process = process ?? this;
        }


        public string GetRealmFileFullPath() => Path.Join(Application.persistentDataPath, RealmFileName);

        public RealmConfiguration CreateConfiguration()
        {
            var config = new RealmConfiguration(GetRealmFileFullPath())
            {
                SchemaVersion = schemeVersion,
            };

            if (encryptKey != null)
            {
                config.EncryptionKey = encryptKey;
            }

            if (schemeTypes != null && schemeTypes.Length > 0)
            {
                var sBuilder = new RealmSchema.Builder();
                var roType = typeof(RealmObject);

                foreach (var t in schemeTypes)
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

        public T With<T>(Func<Realm, T> func) => func(Realm);

        public void With(UnityAction<Realm> action) => action(Realm);

        public T WithTransaction<T>(Func<Realm, T> func) => Realm.WithTransaction(func);
        public void WithTransaction(UnityAction<Realm> action) => Realm.WithTransaction(action);

        public void Dispose()
        {
            _realm?.Dispose();
            _realm = null;
        }

        public void DeleteAllRealm()
        {            
            Dispose();
            Realm.DeleteRealm(Configuration);
            _configuration = null;
        }

    }

}