using System.Security.Cryptography;
using SqlCipher4Unity3D;
using UnityEngine;
using System.Linq;

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




        //--- vacuum unused SecureDbContentData
        public static int VacuumUnusedSecureDbContentData<T>(this SQLiteConnection con) where T : AbsSecureDbContentData, new()
        {
            var sql = $"DELETE FROM {nameof(SecureDbContentData)} WHERE {nameof(SecureDbContentData.Content)} IS NULL AND {nameof(SecureDbContentData.ContentId)} NOT IN (SELECT {AbsSecureDbContentData.ContentIdColumnName} FROM {con.GetMapping<T>().TableName})";
            return con.Execute(sql);
        }
    }
}