using System;
using System.IO;
using System.Threading.Tasks;
using SqlCipher4Unity3D;
using UnityEngine;
using UnityEngine.Events;

namespace ChobiLib.Unity.SQLite
{
    public abstract class AbsChobiSQLiteMonoBehaviour : MonoBehaviour, ChobiSQLite.ISQLiteInitializer
    {

        public abstract string DbFilePath { get; }
        public abstract int DbVersion { get; }

        public virtual string WorkerThreadName { get; }

        public virtual string DbPassword { get; } = null;

        public virtual bool EnableForeignKey => true;

        public UnityAction<SQLiteConnection> onAppQuitProcessInBackground;


        public virtual void OnCreate(SQLiteConnection con)
        {

        }

        private ChobiSQLite _db;
        public ChobiSQLite Db => _db ??= GenerateDb();

        public virtual void DeleteDbFile()
        {
            _db?.Dispose();
            _db = null;

            var path = DbFilePath;
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        protected virtual ChobiSQLite GenerateDb() => new(DbFilePath, DbVersion, DbPassword, EnableForeignKey, this, WorkerThreadName);

        public async Task<T> WithAsyncInBackground<T>(Func<SQLiteConnection, T> func) => await Db.WithAsyncInBackground(func);
        public async Task WithAsyncInBackground(UnityAction<SQLiteConnection> action)
        {
            await WithAsyncInBackground(c =>
            {
                action(c);
                return false;
            });
        }

        public async Task<T> WithTransactionAsyncInBackground<T>(Func<SQLiteConnection, T> func)
        {
            return await Db.WithTransactionAsyncInBackground(func);
        }

        public async Task WithTransactionAsyncInBackground(UnityAction<SQLiteConnection> action)
        {
            await Db.WithTransactionAsyncInBackground(action);
        }

        protected virtual void OnApplicationQuit()
        {
            if (_db != null)
            {
                _ = _db.WithTransactionAsyncInBackground(con =>
                {
                    onAppQuitProcessInBackground?.Invoke(con);
                });

                _db.Dispose(500);
                _db = null;
            }
        }
    }
}