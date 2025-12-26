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

        public static string GetDbPathInPersistentData(string fileName) => Path.Join(Application.persistentDataPath, fileName);

        private SQLiteConnection _con;

        public readonly string dbFilePath;
        public readonly int dbVersion;

        private readonly SemaphoreSlim _dbLock = new(1, 1);

        public volatile bool _isDisposed;
        public bool IsDisposed => _isDisposed;

        public bool IsOpened => _con != null;


        public ChobiSQLite(string dbFilePath, int dbVersion, string password = null, bool enableForeignKey = true, ISQLiteInitializer initializer = null)
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
        }


        private void CheckIsDisposed()
        {
            if (IsDisposed)
            {
                throw new InvalidOperationException("This instance is already disposed");
            }
        }
        
        public async Task<T> WithAsyncInBackground<T>(Func<SQLiteConnection, T> func)
        {
            CheckIsDisposed();

            if (func == null)
            {
                return default;
            }

            await _dbLock.WaitAsync();
            try
            {
                CheckIsDisposed();
                return await Task.Run(() => func(_con));
            }
            finally
            {
                _dbLock.Release();
            }
        }

        public async Task WithAsyncInBackground(UnityAction<SQLiteConnection> action)
        {
            await WithAsyncInBackground(db =>
            {
                action?.Invoke(db);
                return false;
            });
        }

        private T InnerTransactionProcess<T>(Func<SQLiteConnection, T> func)
        {
            if (func == null)
            {
                return default;
            }

            if (_con.IsInTransaction)
            {
                return func(_con);
            }

            _con.BeginTransaction();

            try
            {
                var result = func(_con);
                _con.Commit();
                return result;
            }
            catch
            {
                _con.Rollback();
                throw;
            }
        }

        public async Task<T> WithTransactionAsyncInBackground<T>(Func<SQLiteConnection, T> func)
        {
            CheckIsDisposed();

            await _dbLock.WaitAsync();

            try
            {
                CheckIsDisposed();
                return await Task.Run(() => InnerTransactionProcess(func));
            }
            finally
            {
                _dbLock.Release();
            }
        }

        public async Task WithTransactionAsyncInBackground(UnityAction<SQLiteConnection> action)
        {
            await WithTransactionAsyncInBackground(db =>
            {
                action?.Invoke(db);
                return false;
            });
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            _con?.Close();
            _con?.Dispose();
            _con = null;

            _dbLock.Dispose();
        }

        internal T WithTransactionSync<T>(Func<SQLiteConnection, T> func, int waitTimeMs)
        {
            CheckIsDisposed();

            if(!_dbLock.Wait(waitTimeMs))
            {
                return default;
            }

            try
            {
                CheckIsDisposed();
                return InnerTransactionProcess(func);
            }
            finally
            {
                _dbLock.Release();
            }
        }

        internal void WithTransactionSync(UnityAction<SQLiteConnection> action, int waitTimeMs) => WithTransactionSync(db =>
        {
            action?.Invoke(db);
            return false;
        }, waitTimeMs);
    }
}
