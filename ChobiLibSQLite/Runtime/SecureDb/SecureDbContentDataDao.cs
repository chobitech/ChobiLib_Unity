using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ChobiLib.Security;
using SqlCipher4Unity3D;
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

        public static SecureDbContentData CreateContentData(object obj, string contentId = null, string tagString = null, int? tagInt = null)
        {
            return CreateContentData( JsonUtility.ToJson(obj), contentId, tagString, tagInt);
        }

        public static SecureDbContentData CreateContentData(string json, string contentId = null, string tagString = null, int? tagInt = null)
        {
            var hk = ChobiLib.GenerateRandomBytes(32);

            var tOfs = DateTimeOffset.UtcNow;
            var cid = contentId ?? Guid.NewGuid().ToString();
            var j = $"{cid}\n{tOfs.Ticks}\n{Convert.ToBase64String(hk)}\n{json}";

            var hData = new HMACSHA256(hk).ComputeHash(j.ConvertToByteArray());

            return new()
            {
                ContentId = cid,
                Content = json,
                TagString = tagString,
                TagInt = tagInt,
                HKey = hk,
                HData = hData,
                CreateTimeOffsetUtc = tOfs,
            };
        }

        public static string GetJsonFromContentData(SecureDbContentData cData)
        {
            if (cData == null)
            {
                return null;
            }

            var r = $"{cData.ContentId}\n{cData.CreateTimeOffsetUtc.Ticks}\n{Convert.ToBase64String(cData.HKey)}\n{cData.Content}";

            var ch = new ChobiHash(cData.HKey);
            if (!ch.CompareHash(r.ConvertToByteArray(), cData.HData))
            {
                Debug.LogWarning("SecureDbContentData hash value is not match");
                return null;
            }

            return cData.Content;
        }

        public static S InstantiateFromContentData<S>(SecureDbContentData cData)
        {
            var json = GetJsonFromContentData(cData);
            if (json == null)
            {
                return default;
            }

            return JsonUtility.FromJson<S>(json);
        }

        public static Dictionary<string, bool> SaveToDb(SQLiteConnection con, params SecureDbContentData[] cData)
        {
            var res = new Dictionary<string, bool>();

            try
            {
                foreach (var d in cData)
                {
                    var r = con.InsertOrReplace(d);
                    res[d.ContentId] = r == 1;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                res = new();
            }

            return res;
        }

        public static S InstantiateFromDb<S>(SQLiteConnection con, string contentId)
        {
            var cData = con.Find<SecureDbContentData>(contentId);
            return InstantiateFromContentData<S>(cData);
        }

        public async Task<Dictionary<string, bool>> SaveToDbAsync(params SecureDbContentData[] cData)
        {
            return await Db.WithTransactionAsyncInBackground(db =>
            {
                try
                {
                    return SaveToDb(db, cData);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    return new();
                }
            });
        }

        public async Task<S> InstantiateFromDbAsync<S>(string contentId)
        {
            return await Db.WithTransactionAsyncInBackground(db => InstantiateFromDb<S>(db, contentId));
        }

    }
}