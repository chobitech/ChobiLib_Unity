using System.Collections.Generic;
using System.Linq;
using SqlCipher4Unity3D;
using UnityEngine;

namespace ChobiLib.Unity.SQLite.SecureDb
{
    public static class SecureDbExtensions
    {
        public static Dictionary<string, bool> SaveSecureDbContentData(this SQLiteConnection con, IEnumerable<SecureDbContentData> cData)
        {
            return SecureDbContentDataDao.SaveToDb(con, cData.ToArray());
        }

        public static bool SaveSecureDbContentData(this SQLiteConnection con, SecureDbContentData scData)
        {
            var res = SecureDbContentDataDao.SaveToDb(con, new SecureDbContentData[] { scData });
            return res.IsNotEmpty() && res.First().Value;
        }
        
        // string json, string contentId = null, string tagString = null, int? tagInt = null
        public static SecureDbContentData SaveJsonAsSecureDbContentData(this SQLiteConnection con, string json, string contentId = null, string tagString = null, int? tagInt = null)
        {
            var scData = SecureDbContentData.CreateContentDataFromJson(json, contentId, tagString, tagInt);
            return con.SaveSecureDbContentData(scData) ? scData : null;
        }

        public static SecureDbContentData SaveJsonableAsSecureDbContentData(this SQLiteConnection con, IJsonable jsonable, string contentId = null, string tagString = null, int? tagInt = null)
        {
            return con.SaveJsonAsSecureDbContentData(jsonable.ToJson(), contentId, tagString, tagInt);
        }

        public static SecureDbContentData SaveSerializableAsSecureDbContentData(this SQLiteConnection con, object obj, string contentId = null, string tagString = null, int? tagInt = null)
        {
            return con.SaveJsonAsSecureDbContentData(JsonUtility.ToJson(obj), contentId, tagString, tagInt);
        }



        public static T LoadFromSecureDbContentData<T>(this SQLiteConnection con, string contentId)
        {
            return SecureDbContentDataDao.InstantiateFromDb<T>(con, contentId);
        }
    }
}