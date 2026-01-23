using System;
using System.Collections.Generic;
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

                return res;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                throw ex;
            }
        }

        public static S InstantiateFromDb<S>(SQLiteConnection con, string contentId)
        {
            var cData = con.Find<SecureDbContentData>(contentId);
            return (cData != null) ? cData.ConvertTo<S>() : default;
        }
    }
}