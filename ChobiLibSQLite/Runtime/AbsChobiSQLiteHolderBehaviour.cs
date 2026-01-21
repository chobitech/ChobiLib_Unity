using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SqlCipher4Unity3D;
using UnityEngine;
using UnityEngine.Events;

namespace ChobiLib.Unity.SQLite
{
    public abstract class AbsChobiSQLiteHolderBehaviour : MonoBehaviour
    {
        protected abstract ChobiSQLite OpenChobiSQLite();

        [SerializeField]
        private bool showDebugLog = true;
        public bool ShowDebugLog => showDebugLog;

        public UnityAction<SQLiteConnection> onAppQuitProcessInBackground;

        public UnityAction<SQLiteConnection> onAppPausedProcessInBackground;

        private ChobiSQLite _db;
        public ChobiSQLite Db => _db ??= OpenChobiSQLite();

        public virtual void DeleteDbFile(string dbFilePath = null)
        {
            var dbPath = dbFilePath ?? _db?.dbFilePath;

            _db?.Dispose();
            _db = null;

            if (dbPath != null && File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }

        public virtual Task<T> WithAsyncInBackgroundThread<T>(
            Func<SQLiteConnection, T> func,
            CancellationToken token = default
        ) => Db.WithAsyncInBackground(func, token);

        public Task WithAsyncInBackgroundThread(
            UnityAction<SQLiteConnection> action,
            CancellationToken token = default
        ) => Db.WithAsyncInBackground(action, token);

        public virtual Task<T> WithTransactionAsyncInBackgroundThread<T>(
            Func<SQLiteConnection, T> func,
            CancellationToken token = default
        ) => Db.WithTransactionAsyncInBackground(func, token);

        public Task WithTransactionAsyncInBackgroundThread(
            UnityAction<SQLiteConnection> action,
            CancellationToken token = default
        ) => Db.WithTransactionAsyncInBackground(action, token);

        public virtual void CloseDb()
        {
            if (_db != null && !_db.IsDisposed)
            {
                _db.Dispose();
                _db = null;
            }
        }

        protected virtual int LockWaitTimeMsOnApplicationQuit => 1000;

        protected virtual void OnApplicationPause(bool pause)
        {
            if (pause && _db != null && !_db.IsDisposed)
            {
                ChobiSQLite.Log($"Enter OnAppPause", showLog: showDebugLog);
                _db.WithTransactionSync(db =>
                {
                    onAppPausedProcessInBackground?.Invoke(db);
                }, LockWaitTimeMsOnApplicationQuit);
                ChobiSQLite.Log($"Exit OnAppPause", showLog: showDebugLog);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            if (_db != null && !_db.IsDisposed)
            {
                ChobiSQLite.Log($"Enter OnAppQuit", showLog: showDebugLog);

                _db.WithTransactionSync(db =>
                {
                    onAppQuitProcessInBackground?.Invoke(db);
                }, LockWaitTimeMsOnApplicationQuit);

                ChobiSQLite.Log($"Exit OnAppQuit", showLog: showDebugLog);

                _db.Dispose();
            }
        }
    }
    
}