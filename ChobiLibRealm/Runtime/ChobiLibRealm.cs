using UnityEngine;

namespace Chobitech.Realm
{
    using System;
    using System.Collections;
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