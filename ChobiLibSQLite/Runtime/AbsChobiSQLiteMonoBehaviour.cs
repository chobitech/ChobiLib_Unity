using System;
using System.IO;
using System.Threading.Tasks;
using SqlCipher4Unity3D;
using UnityEngine;
using UnityEngine.Events;

namespace ChobiLib.Unity.SQLite
{
    public abstract class AbsChobiSQLiteMonoBehaviour : AbsChobiSQLiteHolderBehaviour, ChobiSQLite.ISQLiteInitializer
    {

        public abstract string DbFilePath { get; }
        public abstract int DbVersion { get; }

        public virtual string DbPassword { get; } = null;

        public virtual bool EnableForeignKey => true;

        protected override ChobiSQLite OpenChobiSQLite()
        {
            return new ChobiSQLite(DbFilePath, DbVersion, DbPassword, EnableForeignKey, this, ShowDebugLog);
        }


        public virtual void OnCreate(SQLiteConnection con)
        {

        }

        public override void DeleteDbFile(string dbFilePath = null)
        {
            base.DeleteDbFile(DbFilePath ?? dbFilePath);
        }
    }
}