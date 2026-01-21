using System.Threading.Tasks;
using SqlCipher4Unity3D;
using UnityEngine;
using System;
using System.IO;
using ChobiLib.Security;
using System.Threading;
using Unity.Android.Gradle;


namespace ChobiLib.Unity.SQLite.SecureDb
{
    public abstract class AbsChobiSecureSQLiteMonoBehaviour : AbsChobiSQLiteMonoBehaviour
    {
        private readonly TaskCompletionSource<bool> _initTcs = new();

        public async Task<bool> WaitForSQLiteInitialized()
        {
            var res = await _initTcs.Task;
            if (!res)
            {
                ChobiSQLite.LogWarning("SQlite Initialize failed");
            }
            return res;
        }

        public bool NoEncrypt { get; protected set; } = false;

        public async Task<bool> WaitForSQLiteInitialized(int timeoutMs)
        {
            var checkTask = _initTcs.Task;
            var toTask = Task.Delay(timeoutMs);
            var complete = await Task.WhenAny(checkTask, toTask);
            if (complete == checkTask)
            {
                var res = checkTask.Result;
                if (!res)
                {
                    ChobiSQLite.LogWarning("SQlite Initialize failed");
                }
                return res;
            }
            else
            {
                ChobiSQLite.LogWarning("SQLite init wait timeout");
                return false;
            }
        }

        private bool _keyInitializeFinished;
        private bool _isRunningInitDb;

        private bool IsInitDbNotExecute => !_keyInitializeFinished && !_isRunningInitDb;

        //public bool KeyAvailable => HKey != null && HSeed != null;

        public bool KeyAvailable => KeyData != null;

        protected abstract string HSeedFilePath { get; }

        protected abstract Task<string> LoadHKeyAsync(CancellationToken token = default);

        protected ChobiSQLiteKey KeyData { get; private set; }

        //protected string HKey { get; private set; }
        //protected string HSeed { get; private set; }

        protected virtual Type[] GetAdditionalTableSchemes() => Array.Empty<Type>();

        public override void OnCreate(SQLiteConnection con)
        {
            con.CreateTable<SecureDbContentData>();

            foreach (var t in GetAdditionalTableSchemes())
            {
                con.CreateTable(t);
            }
        }


        protected string GenDbPw()
        {
            if (NoEncrypt)
            {
                return null;
            }
            
            if (!KeyAvailable)
            {
                var ex = new Exception("DB password is null");
                ChobiSQLite.LogException(ex);
                throw ex;
            }

            var p = KeyData.GetKeyString();
            Debug.Log($"p = {p}");

            return p;
        }

        public override string DbPassword => GenDbPw();

        private SecureDbContentDataDao _contentDataDao;
        public SecureDbContentDataDao ContentDataDao => _contentDataDao ??= new(this);

        protected async Task LoadKeys(CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                var hk = await LoadHKeyAsync(token);
                Debug.Log($"hk = {hk}");
                
                token.ThrowIfCancellationRequested();

                var hsPath = HSeedFilePath;

                token.ThrowIfCancellationRequested();

                var hSeed = await ChobiSQLiteKey.LoadSKey(hsPath, token);
                Debug.Log($"hSeed = {hSeed}");

                token.ThrowIfCancellationRequested();

                KeyData = new(hk, hSeed);
            }
            catch (Exception ex)
            {
                ChobiSQLite.LogException(ex);
                throw ex;
            }
        }

        protected async Task InitDb(bool withOpenDb = true)
        {
            if (_keyInitializeFinished)
            {
                return;
            }

            _isRunningInitDb = true;

            try
            {
                if (!NoEncrypt)
                {
                    await LoadKeys();
                }
                
                if (withOpenDb)
                {
                    _ = Db;
                }

                _initTcs.TrySetResult(true);
            }
            catch (Exception ex)
            {
                _initTcs.TrySetResult(false);
                ChobiSQLite.LogException(ex);
                throw ex;
            }
            finally
            {
                _isRunningInitDb = false;
                _keyInitializeFinished = true;
            }
        }

        private async Task ExecInitDb()
        {
            if (IsInitDbNotExecute)
            {
                ChobiSQLite.LogWarning("ChobiSecureSQLite not initialized, so run InitDb()");
                await InitDb();
            }
        }

        public override async Task<T> WithAsyncInBackgroundThread<T>(
            Func<SQLiteConnection, T> func,
            CancellationToken token = default
        )
        {
            await ExecInitDb();

            if (!await WaitForSQLiteInitialized())
            {
                return default;
            }

            return await base.WithAsyncInBackgroundThread(func, token);
        }


        public override async Task<T> WithTransactionAsyncInBackgroundThread<T>(
            Func<SQLiteConnection, T> func,
            CancellationToken token = default
        )
        {
            await ExecInitDb();

            if (!await WaitForSQLiteInitialized())
            {
                return default;
            }

            return await base.WithTransactionAsyncInBackgroundThread(func, token);
        }        
    }
}