using UnityEngine;

namespace ChobiLib.Unity.Realm
{
    using System;
    using System.Collections;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Realms;
    using UnityEngine.Events;

    public static class ChobiLibRealm
    {
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


        public static async Task<T> WithTransactionAsync<T>(this Realm realm, Func<Realm, Task<T>> asyncFunc)
        {
            if (realm.IsInTransaction)
            {
                return await asyncFunc(realm);
            }

            var tr = await realm.BeginWriteAsync();

            try
            {
                var result = await asyncFunc(realm);
                await tr.CommitAsync();
                return result;
            }
            catch
            {
                tr.Rollback();
                throw;
            }
        }

        public static async Task WithTransactionAsync(this Realm realm, Func<Realm, Task> asyncAction)
        {
            await realm.WithTransactionAsync<object>(
                async r =>
                {
                    await asyncAction(r);
                    return null;
                }
            );
        }


        public static void RemoveCascade<T>(this Realm realm, T data) where T : IRealmObject => realm.WithTransaction(r =>
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

                if (value is IRealmObject rObj)
                {
                    r.RemoveCascade(rObj);
                }
                else if (value is IList list)
                {
                    foreach (var v in list)
                    {
                        if (v is IRealmObject ro)
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