using System;
using System.Threading;
using System.Threading.Tasks;
using SqlCipher4Unity3D;
using UnityEngine;
using UnityEngine.Events;

namespace ChobiLib.Unity.SQLite
{
    public class ChobiSQLite : IDisposable
    {
        private const string DefaultTag = "ChobiSQLite";

        internal static void Log(object o, string tag = DefaultTag, bool showLog = true)
        {
            if (showLog)
            {
                Debug.Log($"[{tag}] {o}");
            }
        }

        internal static void LogWarning(object o, string  tag = DefaultTag, bool showLog = true)
        {
            if (showLog)
            {
                Debug.LogWarning($"[{tag}] {o}");
            }
        }

        internal static void LogError(object o, string  tag = DefaultTag, bool showLog = true)
        {
            if (showLog)
            {
                Debug.LogError($"[{tag}] {o}");
            }
        }

        internal static void LogException(Exception ex, string  tag = DefaultTag, bool showLog = true)
        {
            if (showLog)
            {
                Debug.LogException(ex);
            }
        }

        public interface ISQLiteInitializer
        {
            void OnCreate(SQLiteConnection connection) { }
            public virtual void OnUpgrade(SQLiteConnection con, int oldVersion, int newVersion) { }

            public virtual void OnOpen(SQLiteConnection connection) { }
        }

        
        private SQLiteConnection _con;

        public readonly string dbFilePath;
        public readonly int dbVersion;

        private readonly SemaphoreSlim _dbLock = new(1, 1);

        public volatile bool _isDisposed;
        public bool IsDisposed => _isDisposed;

        public bool IsOpened => _con != null;

        public bool showDebugLog = true;

        public readonly bool enableForeignKey;

        public readonly ISQLiteInitializer initializer;


        public ChobiSQLite(
            string dbFilePath,
            int dbVersion,
            string password = null,
            bool enableForeignKey = true,
            ISQLiteInitializer initializer = null,
            bool showDebugLog = true
        )
        {
            this.dbFilePath = dbFilePath;
            this.dbVersion = dbVersion;
            this.showDebugLog = showDebugLog;
            this.enableForeignKey = enableForeignKey;
            this.initializer = initializer;

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

            Log($"Create and open \"{dbFilePath}\"", showLog: showDebugLog);
        }


        private void CheckIsDisposed()
        {
            if (IsDisposed)
            {
                var ex = new InvalidOperationException("This instance is already disposed");
                LogException(ex, showLog: showDebugLog);
                throw ex;
            }
        }

        private async Task<T> RunOnBgThread<T>(Func<SQLiteConnection, T> func, CancellationToken token)
        {
            if (ChobiUnityThread.IsInMainThread)
            {
                return await ChobiUnityThread.RunOnBackgroundThreadAsync(() => func(_con), token);
            }
            else
            {
                return func(_con);
            }
        }
        
        public async Task<T> WithAsyncInBackground<T>(
            Func<SQLiteConnection, T> func,
            CancellationToken token = default
        )
        {
            CheckIsDisposed();

            Log("Enter Async Process", showLog: showDebugLog);

            if (func == null)
            {
                return default;
            }

            await _dbLock.WaitAsync(token);
            
            try
            {
                CheckIsDisposed();
                return await RunOnBgThread(func, token);
            }
            finally
            {
                _dbLock.Release();
                Log("Exit Async Process", showLog: showDebugLog);
            }
        }

        public async Task WithAsyncInBackground(
            UnityAction<SQLiteConnection> action,
            CancellationToken token = default
        )
        {
            await WithAsyncInBackground<bool>(db =>
            {
                action?.Invoke(db);
                return false;
            }, token);
        }

        private T InnerTransactionProcess<T>(Func<SQLiteConnection, T> func, CancellationToken token)
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

            Log($"Enter Transaction Process", showLog: showDebugLog);

            try
            {
                token.ThrowIfCancellationRequested();

                var result = func(_con);

                token.ThrowIfCancellationRequested();
                
                _con.Commit();
                return result;
            }
            catch (Exception ex)
            {
                _con.Rollback();
                LogException(ex, showLog: showDebugLog);
                throw ex;
            }
            finally
            {
                Log($"Exit Transaction Process", showLog: showDebugLog);
            }
        }

        public async Task<T> WithTransactionAsyncInBackground<T>(
            Func<SQLiteConnection, T> func,
            CancellationToken token = default
        )
        {
            CheckIsDisposed();

            await _dbLock.WaitAsync(token);

            Log($"Start Async Transaction Process", showLog: showDebugLog);

            try
            {
                CheckIsDisposed();
                return await RunOnBgThread(_ => InnerTransactionProcess(func, token), token);
            }
            finally
            {
                _dbLock.Release();
                Log($"Finish Async Transaction Process", showLog: showDebugLog);
            }
        }

        public async Task WithTransactionAsyncInBackground(
            UnityAction<SQLiteConnection> action,
            CancellationToken token = default
        )
        {
            await WithTransactionAsyncInBackground<bool>(db =>
            {
                action?.Invoke(db);
                return false;
            }, token);
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

            Log($"Disposed", showLog: showDebugLog);
        }

        internal T WithTransactionSync<T>(
            Func<SQLiteConnection, T> func,
            int waitTimeMs,
            CancellationToken token = default
        )
        {
            CheckIsDisposed();

            if(!_dbLock.Wait(waitTimeMs, token))
            {
                LogWarning("DB lock timeout", showLog: showDebugLog);
                return default;
            }

            Log($"Start Sync Transaction Process", showLog: showDebugLog);

            try
            {
                CheckIsDisposed();
                return InnerTransactionProcess(func, token);
            }
            catch (Exception ex)
            {
                LogException(ex, showLog: showDebugLog);
                return default;
            }
            finally
            {
                _dbLock.Release();
                Log($"Finish Sync Transaction Process", showLog: showDebugLog);
            }
        }

        internal void WithTransactionSync(
            UnityAction<SQLiteConnection> action,
            int waitTimeMs,
            CancellationToken token = default
        ) => WithTransactionSync<bool>(db =>
        {
            action?.Invoke(db);
            return false;
        }, waitTimeMs, token);
    }
}
