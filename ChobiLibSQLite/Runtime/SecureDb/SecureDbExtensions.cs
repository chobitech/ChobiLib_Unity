using System.Collections.Generic;
using System.Linq;
using SqlCipher4Unity3D;

namespace ChobiLib.Unity.SQLite.SecureDb
{
    public static class SecureDbExtensions
    {

        public static Dictionary<string, bool> SaveSecureDbContentData(this SQLiteConnection con, IEnumerable<SecureDbContentData> cData)
        {
            return SecureDbContentDataDao.SaveToDb(con, cData.ToArray());
        }

        public static T LoadFromSecureDbContentData<T>(this SQLiteConnection con, string contentId)
        {
            return SecureDbContentDataDao.InstantiateFromDb<T>(con, contentId);
        }
    }
}