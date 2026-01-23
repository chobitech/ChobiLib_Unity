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
        protected abstract Task<ChobiSQLite> OpenChobiSQLite(CancellationToken token = default);

        [SerializeField]
        private bool showDebugLog = true;
        public bool ShowDebugLog => showDebugLog;

        public UnityAction<SQLiteConnection> onAppQuitProcessInBackground;

        public UnityAction<SQLiteConnection> onAppPausedProcessInBackground;

        public ChobiSQLite Db { get; private set; }

        private TaskCompletionSource<bool> _runningInitDbCompletion;
        private bool IsRunningInitDb => _runningInitDbCompletion?.Task.IsCompleted == false;
        public virtual bool IsDbInitialized => Db != null;

        private async Task<bool> InnerWaitForInitDbFinished(CancellationToken token = default)
        {
            if (IsRunningInitDb)
            {
                //--- wait for InitDb() finished
                ChobiSQLite.LogWarning($"InitDb() is already running");
                token.ThrowIfCancellationRequested();
                await _runningInitDbCompletion?.Task;
                token.ThrowIfCancellationRequested();
                return true;
            }

            return false;
        }

        public async Task<bool> WaitForInitializeFinished(CancellationToken token = default)
        {
            _ = await InnerWaitForInitDbFinished(token);
            return IsDbInitialized;
        }

        protected async Task InitializeSQLiteAsync(CancellationToken token = default)
        {
            if (Db != null)
            {
                ChobiSQLite.LogWarning($"Db is already opened");
                return;
            }

            if (await InnerWaitForInitDbFinished(token))
            {
                return;
            }

            _runningInitDbCompletion = new();

            try
            {
                token.ThrowIfCancellationRequested();
                Db = await OpenChobiSQLite(token);
                token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                _runningInitDbCompletion?.TrySetCanceled(token);
                _runningInitDbCompletion = null;
            }
            catch (Exception ex)
            {
                _runningInitDbCompletion?.TrySetException(ex);
                _runningInitDbCompletion = null;
            }
            finally
            {
                _runningInitDbCompletion?.TrySetResult(true);
            }
        }


        public virtual void DeleteDbFile(string dbFilePath = null)
        {
            var dbPath = dbFilePath ?? Db?.dbFilePath;

            Db?.Dispose();
            Db = null;

            if (dbPath != null && File.Exists(dbPath))
            {
                File.Delete(dbPath);
                ChobiSQLite.Log($"Db file deleted");
            }

            _runningInitDbCompletion = null;
        }
        

        private async Task<T> RunWithDb<T>(Func<ChobiSQLite, Task<T>> func, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                if (!IsDbInitialized)
                {
                    ChobiSQLite.LogWarning("Running InitDB() because this is not initialized");
                    await InitializeSQLiteAsync(token);
                }
                if (Db == null)
                {
                    throw new NullReferenceException("Db is null");
                }
                token.ThrowIfCancellationRequested();
                var res = await func(Db);
                token.ThrowIfCancellationRequested();
                return res;
            }
            catch (Exception ex)
            {
                ChobiSQLite.LogException(ex);
                throw;
            }

        }

        public virtual Task<T> WithAsyncInBackgroundThread<T>(
            Func<SQLiteConnection, T> func,
            CancellationToken token = default
        ) => RunWithDb(db => db.WithAsyncInBackground(func, token), token);

        public Task WithAsyncInBackgroundThread(
            UnityAction<SQLiteConnection> action,
            CancellationToken token = default
        ) => WithAsyncInBackgroundThread<bool>(
            con =>
            {
                action?.Invoke(con);
                return false;
            },
            token
        );

        public virtual Task<T> WithTransactionAsyncInBackgroundThread<T>(
            Func<SQLiteConnection, T> func,
            CancellationToken token = default
        ) => RunWithDb(db => db.WithTransactionAsyncInBackground(func, token), token);

        public Task WithTransactionAsyncInBackgroundThread(
            UnityAction<SQLiteConnection> action,
            CancellationToken token = default
        ) => WithTransactionAsyncInBackgroundThread<bool>(con =>
        {
            action?.Invoke(con);
            return false;
        }, token);

        public virtual void CloseDb()
        {
            if (Db != null && !Db.IsDisposed)
            {
                Db.Dispose();
                Db = null;
                ChobiSQLite.Log($"Db Closed");
            }
        }

        protected virtual int LockWaitTimeMsOnApplicationQuit => 1000;

        protected virtual void OnApplicationPause(bool pause)
        {
            if (pause && Db != null && !Db.IsDisposed)
            {
                ChobiSQLite.Log($"Enter OnAppPause", showLog: showDebugLog);
                Db.WithTransactionSync(db =>
                {
                    onAppPausedProcessInBackground?.Invoke(db);
                }, LockWaitTimeMsOnApplicationQuit);
                ChobiSQLite.Log($"Exit OnAppPause", showLog: showDebugLog);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            if (Db != null && !Db.IsDisposed)
            {
                ChobiSQLite.Log($"Enter OnAppQuit", showLog: showDebugLog);

                Db.WithTransactionSync(db =>
                {
                    onAppQuitProcessInBackground?.Invoke(db);
                }, LockWaitTimeMsOnApplicationQuit);

                ChobiSQLite.Log($"Exit OnAppQuit", showLog: showDebugLog);

                CloseDb();
            }
        }

        protected virtual void OnDestroy()
        {
            CloseDb();
        }
    }
    
}