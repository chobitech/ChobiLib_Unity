using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SqlCipher4Unity3D;
using UnityEngine;
using UnityEngine.Events;

namespace ChobiLib.Unity.SQLite
{
    public class ChobiSQLite : IDisposable
    {
        public interface ISQLiteInitializer
        {
            void OnCreate(SQLiteConnection connection) { }
            public virtual void OnUpgrade(SQLiteConnection con, int oldVersion, int newVersion) { }

            public virtual void OnOpen(SQLiteConnection connection) { }
        }

        public const string WorkerThreadNamePrefix = "ChobiSQLiteWorker";

        public static string GetDbPathInPersistentData(string fileName) => Path.Join(Application.persistentDataPath, fileName);

        private SQLiteConnection _con;

        public readonly string dbFilePath;
        public readonly int dbVersion;

        public readonly ChobiBackgroundWorker backgroundWorker;


        public ChobiSQLite(string dbFilePath, int dbVersion, string password = null, bool enableForeignKey = true, ISQLiteInitializer initializer = null, string workerThreadName = null)
        {
            this.dbFilePath = dbFilePath;
            this.dbVersion = dbVersion;
            

            _con = new SQLiteConnection(
                databasePath: dbFilePath,
                password: password
            );

            if (enableForeignKey)
            {
                _con.Execute("PRAGMA foreign_keys = ON;");
            }

            var currentVer = _con.ExecuteScalar<int>("PRAGMA user_version;");

            var execOnCreate = currentVer == 0;
            var isSameVersion = currentVer == dbVersion;

            if (!isSameVersion)
            {
                _con.Execute($"PRAGMA user_version = {dbVersion};");
            }

            if (initializer != null)
            {
                initializer.OnOpen(_con);

                if (execOnCreate)
                {
                    initializer.OnCreate(_con);
                }

                if (!isSameVersion)
                {
                    initializer.OnUpgrade(_con, currentVer, dbVersion);
                }

            }

            var thName = workerThreadName ?? $"{WorkerThreadNamePrefix}-{RandomString.GetCharDigitsRandomString().GetRandomString(8)}";
            backgroundWorker = new(thName);
        }
        
        public async Task<T> WithAsyncInBackground<T>(Func<SQLiteConnection, T> func)
        {
            T result = default;
            await backgroundWorker.RunInBackground(() =>
            {
                result = func(_con);
            });
            return result;
        }

        public async Task WithAsyncInBackground(UnityAction<SQLiteConnection> action)
        {
            await WithAsyncInBackground(db =>
            {
                action(db);
                return false;
            });
        }

        public async Task<T> WithTransactionAsyncInBackground<T>(Func<SQLiteConnection, T> func)
        {
            T result = default;

            await backgroundWorker.RunInBackground(() =>
            {
                if (_con.IsInTransaction)
                {
                    result = func(_con);
                    return;
                }

                try
                {
                    _con.BeginTransaction();
                    result = func(_con);
                    _con.Commit();
                }
                catch (Exception ex)
                {
                    _con.Rollback();
                    Debug.LogException(ex);
                    result = default;
                }
            });

            return result;
        }

        public async Task WithTransactionAsyncInBackground(UnityAction<SQLiteConnection> action)
        {
            await WithTransactionAsyncInBackground(db =>
            {
                action?.Invoke(_con);
                return false;
            });
        }

        public void Dispose(int workerWaitMs)
        {
            backgroundWorker.Dispose(workerWaitMs);
            _con?.Close();
            _con?.Dispose();
            _con = null;
        }

        public void Dispose() => Dispose(500);
    }
}
