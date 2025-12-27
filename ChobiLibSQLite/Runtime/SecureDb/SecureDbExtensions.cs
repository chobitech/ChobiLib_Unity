using System.Collections.Generic;
using System.Linq;
using SqlCipher4Unity3D;

namespace ChobiLib.Unity.SQLite.SecureDb
{
    public static class SecureDbExtensions
    {
        public static SecureDbContentData ConvertToSecureDbContentData(this object obj, string contentId = null, string tagString = null, int? tagInt = null)
        {
            return SecureDbContentDataDao.CreateContentData(obj, contentId, tagString, tagInt);
        }

        public static T ConvertTo<T>(this SecureDbContentData cData)
        {
            return SecureDbContentDataDao.InstantiateFromContentData<T>(cData);
        }

        public static Dictionary<string, bool> SaveSecureDbContentData(this SQLiteConnection con, IEnumerable<SecureDbContentData> cData)
        {
            return SecureDbContentDataDao.SaveToDb(con, cData.ToArray());
        }

        public static SecureDbContentData SaveAsSecureDbContentData(this SQLiteConnection con, object obj, string contentId = null, string tagString = null, int? tagInt = null)
        {
            var scData = obj.ConvertToSecureDbContentData(contentId, tagString, tagInt);
            var res = con.SaveSecureDbContentData(new SecureDbContentData[] { scData });
            if (!res.TryGetValue(scData.ContentId, out var b))
            {
                b = false;
            }
            return b ? scData : null;
        }

        public static T LoadFromSecureDbContentData<T>(this SQLiteConnection con, string contentId)
        {
            return SecureDbContentDataDao.InstantiateFromDb<T>(con, contentId);
        }
    }
}