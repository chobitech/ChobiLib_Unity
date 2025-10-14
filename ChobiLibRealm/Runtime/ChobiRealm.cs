namespace ChobiLib.Unity.Realm
{
    using System;
    using System.IO;
    using Realms;
    using System.Security.Cryptography;
    using UnityEngine.Events;
    using System.Threading.Tasks;
    using UnityEngine.Video;

    public class ChobiRealm
    {
        public const int RealmEncryptionKeyBytes = 64;

        public static byte[] LoadRealmEncryptionKey(string filePath, bool createNewIfNotExists = true)
        {
            byte[] data = null;

            try
            {
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
            catch
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
            Realm Realm { get; }

            string GetRealmFileFullPath(string realmFileName);

            RealmConfiguration CreateConfiguration();
            T With<T>(Func<Realm, T> func);
            void With(UnityAction<Realm> action);

            T WithTransaction<T>(Func<Realm, T> func);

            Task<T> WithTransactionAsync<T>(Func<Realm, Task<T>> asyncFunc);

            void DeleteAllRealm();


            public virtual void WithTransaction(UnityAction<Realm> action) => WithTransaction<object>(r =>
            {
                action(r);
                return null;
            });

            public async virtual Task WithTransactionAsync(Func<Realm, Task> asyncAction)
            {
                await WithTransactionAsync<object>(
                    async r =>
                    {
                        await asyncAction(r);
                        return null;
                    }
                );
            }
        }

        public readonly string realmFileName;
        public readonly ulong schemeVersion;
        public readonly Type[] schemeTypes;
        public readonly byte[] encryptKey;

        public readonly IChobiRealmProcess process;

        public ChobiRealm(
            string realmFileName,
            ulong schemeVersion = 1,
            Type[] schemeTypes = null,
            byte[] encryptKey = null,
            IChobiRealmProcess process = null
        )
        {
            this.realmFileName = realmFileName;
            this.schemeVersion = schemeVersion;
            this.schemeTypes = schemeTypes;
            this.encryptKey = encryptKey;
            this.process = process ?? new DefaultChobiRealmProcess(
                realmFileName,
                schemeVersion,
                schemeTypes,
                encryptKey
            );
        }


        public virtual Realm Realm => process.Realm;

        public virtual string GetRealmFileFullPath() => process.GetRealmFileFullPath(realmFileName);

        public virtual RealmConfiguration CreateConfiguration() => process.CreateConfiguration();

        public virtual T With<T>(Func<Realm, T> func) => process.With(func);

        public virtual void With(UnityAction<Realm> action) => process.With(action);

        public virtual T WithTransaction<T>(Func<Realm, T> func) => process.WithTransaction(func);
        public void WithTransaction(UnityAction<Realm> action) => process.WithTransaction(action);

        public async virtual Task<T> WithTransactionAsync<T>(Func<Realm, Task<T>> asyncFunc)
        {
            return await process.WithTransactionAsync(asyncFunc);
        }

        public async Task WithTransactionAsync(Func<Realm, Task> asyncAction)
        {
            await process.WithTransactionAsync(asyncAction);
        }

        public virtual void Dispose() => process.Dispose();
        public virtual void DeleteAllRealm() => process.DeleteAllRealm();
    }

}