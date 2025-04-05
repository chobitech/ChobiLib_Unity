using UnityEngine;

namespace Chobitech.Realm
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Security.Cryptography;
    using Realms;
    using UnityEngine.Events;

    public static class ChobiLibRealm
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


        public static T WithTransaction<T>(this Realm realm, Func<Realm, T> func)
        {
            if (realm.IsInTransaction)
            {
                return func(realm);
            }
            else
            {
                using var tr = realm.BeginWrite();

                try
                {
                    var res = func(realm);
                    tr.Commit();
                    return res;
                }
                catch (Exception ex)
                {
                    tr.Rollback();
                    Debug.LogError($"Realm Transaction Error: {ex}");
                    throw ex;
                }
            }
        }

        public static void WithTransaction(this Realm realm, UnityAction<Realm> action) => realm.WithTransaction<object>(r =>
        {
            action(r);
            return null;
        });


        public static void RemoveCascade<T>(this Realm realm, T data) where T : RealmObject => realm.WithTransaction(r =>
        {
            var tType = typeof(T);
            var props = tType.GetProperties();

            foreach (var p in props)
            {
                if (!Attribute.IsDefined(p, CascadeDeleteAttribute.SelfType))
                {
                    continue;
                }

                var value = p.GetValue(data);

                if (value is RealmObject rObj)
                {
                    r.RemoveCascade(rObj);
                }
                else if (value is IList list)
                {
                    foreach (var v in list)
                    {
                        if (v is RealmObject ro)
                        {
                            r.RemoveCascade(ro);
                        }
                    }
                }
            }

            r.Remove(data);
        });
    }
}