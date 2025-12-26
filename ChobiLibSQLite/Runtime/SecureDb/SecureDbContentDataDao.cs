using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ChobiLib.Security;
using UnityEngine;

namespace ChobiLib.Unity.SQLite.SecureDb
{
    public class SecureDbContentDataDao
    {
        public AbsChobiSecureSQLiteMonoBehaviour Db { get; private set; }

        public SecureDbContentDataDao(AbsChobiSecureSQLiteMonoBehaviour db)
        {
            Db = db;
        }

        public SecureDbContentData CreateContentData(object obj, string contentId = null, string tagString = null, int? tagInt = null)
        {
            var json = JsonUtility.ToJson(obj);
            var hk = ChobiLib.GenerateRandomBytes(32);
            var hData = new HMACSHA256(hk).ComputeHash(json.ConvertToByteArray());

            var cid = contentId ?? Guid.NewGuid().ToString();

            return new()
            {
                ContentId = cid,
                Content = json,
                TagString = tagString,
                TagInt = tagInt,
                HKey = hk,
                HData = hData,
                CreateTimeOffsetUtc = DateTimeOffset.UtcNow
            };
        }

        public S InstantiateFromContentData<S>(SecureDbContentData cData)
        {
            if (cData == null)
            {
                return default;
            }

            var ch = new ChobiHash(cData.HKey);
            if (!ch.CompareHash(cData.Content.ConvertToByteArray(), cData.HData))
            {
                return default;
            }

            return JsonUtility.FromJson<S>(cData.Content);
        }

        public async Task<bool> SaveToDbAsync(params SecureDbContentData[] cData)
        {
            return await Db.WithTransactionAsyncInBackground(db =>
            {
                try
                {
                    foreach (var d in cData)
                    {
                        db.InsertOrReplace(d);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    return false;
                }
            });
        }

        public async Task<S> InstantiateFromDb<S>(string contentId)
        {
            var cData = await Db.WithTransactionAsyncInBackground(db => db.Find<SecureDbContentData>(contentId));
            return InstantiateFromContentData<S>(cData);
        }

    }
}