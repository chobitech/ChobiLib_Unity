using System.Security.Cryptography;
using SqlCipher4Unity3D;
using UnityEngine;

namespace ChobiLib.Unity.SQLite.SecureDb
{
    public static class SecureDbExtensions
    {
        //--- control SecureDbContentData

        //--- insert
        public static SecureDbContentData InsertJsonAsSecureDbContentData<T>(this SQLiteConnection con, string json, string contentId = null, bool insertOrReplace = true)
        {
            var scd = SecureDbContentData.CreateContentDataFromJson(json, contentId);
            var res = insertOrReplace ? con.InsertOrReplace(scd) : con.Insert(scd);
            return (res > 0) ? scd : throw SQLiteException.New(SQLite3.Result.Error, "SecureDbContentData insert failed");
        }

        public static SecureDbContentData InsertSerializableAsSecureDbContentData<T>(this SQLiteConnection con, object obj, string contentId = null, bool insertOrReplace = true)
        {
            return con.InsertJsonAsSecureDbContentData<T>(JsonUtility.ToJson(obj), contentId, insertOrReplace);
        }

        //--- update
        public static bool UpdateJsonAsSecureDbContentData(this SQLiteConnection con, string json, string contentId = null)
        {
            var scd = SecureDbContentData.CreateContentDataFromJson(json, contentId);
            return con.Update(scd) > 0;
        }

        public static bool UpdateSerializableAsSecureDbContentData(this SQLiteConnection con, object obj, string contentId = null)
        {
            return con.UpdateJsonAsSecureDbContentData(JsonUtility.ToJson(obj), contentId);
        }

        //--- delete
        public static bool DeleteSecureDbContentData(this SQLiteConnection con, string contentId)
        {
            return con.Delete<SecureDbContentData>(contentId) > 0;
        }



        //--- load
        public static SecureDbContentData LoadSecureDbContentData(this SQLiteConnection con, string contentId)
        {
            var scd = con.Find<SecureDbContentData>(contentId);
            if (scd.CheckIsValidData())
            {
                return scd;
            }
            else
            {
                throw new CryptographicException("SecureDbContentData hash is invalid");
            }
        }

        public static bool TryLoadSecureDbContentData(this SQLiteConnection con, string contentId, out SecureDbContentData scData)
        {
            try
            {
                scData = con.LoadSecureDbContentData(contentId);
                return true;
            }
            catch
            {
                scData = null;
                return false;
            }
        }


        public static T LoadFromSecureDbContentData<T>(this SQLiteConnection con, string contentId)
        {
            var scd = con.LoadSecureDbContentData(contentId);
            return JsonUtility.FromJson<T>(scd.Content);
        }

        public static bool TryLoadFromSecureDbContentData<T>(this SQLiteConnection con, string contentId, out T outData)
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




        /*
        public static Dictionary<string, bool> SaveSecureDbContentData(this SQLiteConnection con, IEnumerable<SecureDbContentData> cData)
        {
            return SecureDbContentDataDao.SaveToDb(con, cData.ToArray());
        }

        public static bool SaveSecureDbContentData(this SQLiteConnection con, SecureDbContentData scData)
        {
            var res = SecureDbContentDataDao.SaveToDb(con, new SecureDbContentData[] { scData });
            return res.IsNotEmpty() && res.First().Value;
        }
        
        public static SecureDbContentData SaveJsonAsSecureDbContentData(this SQLiteConnection con, string json, string contentId = null)
        {
            var scData = SecureDbContentData.CreateContentDataFromJson(json, contentId);
            return con.SaveSecureDbContentData(scData) ? scData : null;
        }

        public static SecureDbContentData SaveJsonableAsSecureDbContentData(this SQLiteConnection con, IUnityJsonable jsonable, string contentId = null)
        {
            return con.SaveJsonAsSecureDbContentData(jsonable.ToJson(), contentId);
        }

        public static SecureDbContentData SaveSerializableAsSecureDbContentData(this SQLiteConnection con, object obj, string contentId = null)
        {
            return con.SaveJsonAsSecureDbContentData(JsonUtility.ToJson(obj), contentId);
        }



        public static T LoadFromSecureDbContentData<T>(this SQLiteConnection con, string contentId)
        {
            return SecureDbContentDataDao.InstantiateFromDb<T>(con, contentId);
        }


        public static bool TryLoadFromSecureContentData<T>(this SQLiteConnection con, string contentId, out T value)
        {
            value = SecureDbContentDataDao.InstantiateFromDb<T>(con, contentId);
            return !EqualityComparer<T>.Default.Equals(value, default);
        }
        */
    }
}