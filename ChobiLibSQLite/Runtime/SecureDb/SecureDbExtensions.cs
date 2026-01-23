using SqlCipher4Unity3D;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace ChobiLib.Unity.SQLite.SecureDb
{
    public static class SecureDbExtensions
    {
        //--- control SecureDbContentData

        //--- insert
        public static SecureDbContentData InsertJsonAsSecureDbContentData(this SQLiteConnection con, string json, string contentId = null, bool insertOrReplace = true)
        {
            var scd = SecureDbContentData.CreateContentDataFromJson(json, contentId, true);
            var res = insertOrReplace ? con.InsertOrReplace(scd) : con.Insert(scd);
            return (res > 0) ? scd : throw SQLiteException.New(SQLite3.Result.Error, "SecureDbContentData insert failed");
        }

        public static SecureDbContentData InsertSerializableAsSecureDbContentData(this SQLiteConnection con, object obj, string contentId = null, bool insertOrReplace = true)
        {
            return con.InsertJsonAsSecureDbContentData(JsonUtility.ToJson(obj), contentId, insertOrReplace);
        }

        //--- update
        public static bool UpdateJsonAsSecureDbContentData(this SQLiteConnection con, string json, string contentId)
        {
            var scd = SecureDbContentData.CreateContentDataFromJson(json, contentId, true);
            return con.Update(scd) > 0;
        }

        public static bool UpdateSerializableAsSecureDbContentData(this SQLiteConnection con, object obj, string contentId)
        {
            return con.UpdateJsonAsSecureDbContentData(JsonUtility.ToJson(obj), contentId);
        }

        //--- delete
        public static bool DeleteSecureDbContentData(this SQLiteConnection con, string contentId)
        {
            return con.Delete<SecureDbContentData>(contentId) > 0;
        }



        //--- load
        public static List<SecureDbContentData> SelectSecureDbContentData(this SQLiteConnection con, IEnumerable<string> contentIds, bool ignoreHashNotMatched = true)
        {
            var idArr = contentIds.ToArray();
            var res = con.Table<SecureDbContentData>()
                .Where(x => idArr.Contains(x.ContentId))
                .ToList();
            
            if (ignoreHashNotMatched)
            {
                res = res.Where(x => x.CheckIsValidData()).ToList();
            }

            return res;
        }

        public static List<T> SelectSerializablesFromSecureDbContentData<T>(this SQLiteConnection con, IEnumerable<string> contentIds, bool ignoreHashNotMatched = true)
        {
            return con.SelectSecureDbContentData(contentIds)
                .Select(x => JsonUtility.FromJson<T>(x.Content))
                .ToList();
        }

        public static SecureDbContentData SelectSecureDbContentData(this SQLiteConnection con, string contentId)
        {
            return con.Find<SecureDbContentData>(contentId);
        }

        public static T SelectSerializableFromSecureDbContentData<T>(this SQLiteConnection con, string contentId)
        {
            var res = con.SelectSecureDbContentData(contentId);
            return (res != null) ? JsonUtility.FromJson<T>(res.Content) : default;
        }



        public static List<AbsSecureDbContentDataHolder<T>> ToAbsSecureDbContentDataHolders<T>(this TableQuery<T> query) where T : AbsSecureDbContentData, new()
        {
            return query.Connection.Table<SecureDbContentData>()
                .Join(
                    query,
                    scd => scd.ContentId,
                    t => t.SecureDbContentDataId,
                    (scd, t) => new { scd, t }
                )
                .ToArray()
                .Select(v =>
                {
                    return new AbsSecureDbContentDataHolder<T>(v.t, v.scd.CheckIsValidDataWithContent(v.t.ToJson()));
                })
                .ToList();
        }

        public static List<AbsSecureDbContentDataHolder<T>> SelectAbsSecureDbContentData<T>(this SQLiteConnection c, IEnumerable<string> contentIds) where T : AbsSecureDbContentData, new()
        {
            var cidArr = contentIds.ToArray();
            return c.Table<T>()
                .Where(t => cidArr.Contains(t.SecureDbContentDataId))
                .ToAbsSecureDbContentDataHolders();
        }

        public static AbsSecureDbContentDataHolder<T> SelectAbsSecureDbContentData<T>(this SQLiteConnection c, string contentId) where T : AbsSecureDbContentData, new()
        {
            return c.SelectAbsSecureDbContentData<T>(new string[] { contentId }).FirstOrDefault();
        }

        /*
        public static SecureDbContentData LoadSecureDbContentData(this SQLiteConnection con, string contentId)
        {
            var scd = con.Find<SecureDbContentData>(contentId);
            if (scd?.CheckIsValidData() == true)
            {
                return scd;
            }
            return null;
        }



        public 

        public static SecureDbContentData LoadSecureDbContentData<T>(this SQLiteConnection con, string contentId) where T : AbsSecureDbContentData, new()
        {
            var srcData = con.Table<T>()
                .Where(t => t.SecureDbContentDataId == contentId)
                .FirstOrDefault() ?? throw new System.Exception($"The data has SecureDbContentId = {contentId} is not found");

            var scd = con.Find<SecureDbContentData>(contentId);
            if (scd.CheckIsValidDataWithContent(srcData.ToJson()))
            {
                return scd;
            }
            else
            {
                throw new CryptographicException("SecureDbContentData hash is invalid");
            }
        }

        public static bool TryLoadSecureDbContentData<T>(this SQLiteConnection con, string contentId, out SecureDbContentData scData) where T : AbsSecureDbContentData, new()
        {
            try
            {
                scData = con.LoadSecureDbContentData<T>(contentId);
                return true;
            }
            catch
            {
                scData = null;
                return false;
            }
        }


        public static T LoadFromSecureDbContentData<T>(this SQLiteConnection con, string contentId) where T : AbsSecureDbContentData, new()
        {
            var scd = con.LoadSecureDbContentData<T>(contentId);
            return JsonUtility.FromJson<T>(scd.Content);
        }

        public static bool TryLoadFromSecureDbContentData<T>(this SQLiteConnection con, string contentId, out T outData) where T : AbsSecureDbContentData, new()
        {
            try
            {
                outData = con.LoadFromSecureDbContentData<T>(contentId);
                return true;
            }
            catch
            {
                outData = default;
                return false;
            }
        }



        public static T LoadSerializableFromSecureDbContentData<T>(this SQLiteConnection con, string contentId)
        {
            
        }
        */




        //--- vacuum unused SecureDbContentData
        public static int VacuumUnusedSecureDbContentData<T>(this SQLiteConnection con) where T : AbsSecureDbContentData, new()
        {
            var sql = $"DELETE FROM {nameof(SecureDbContentData)} WHERE {nameof(SecureDbContentData.Content)} IS NULL AND {nameof(SecureDbContentData.ContentId)} NOT IN (SELECT {AbsSecureDbContentData.ContentIdColumnName} FROM {con.GetMapping<T>().TableName})";
            return con.Execute(sql);
        }
    }
}