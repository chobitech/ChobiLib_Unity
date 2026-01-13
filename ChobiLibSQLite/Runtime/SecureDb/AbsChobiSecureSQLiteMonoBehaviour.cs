using System.Threading.Tasks;
using SqlCipher4Unity3D;
using UnityEngine;
using System;
using System.IO;
using ChobiLib.Security;


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
        public bool KeyAvailable => HKey != null && HSeed != null;

        protected abstract string HSeedFilePath { get; }

        protected virtual int HSeedByteSize { get; } = 48;

        protected abstract Task<string> LoadHKeyAsync();

        protected string HKey { get; private set; }
        protected string HSeed { get; private set; }

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

            var h = new ChobiHash(Convert.FromBase64String(HKey)).CalcHash(Convert.FromBase64String(HSeed));
            return Convert.ToBase64String(h);
        }

        public override string DbPassword => GenDbPw();

        private SecureDbContentDataDao _contentDataDao;
        public SecureDbContentDataDao ContentDataDao => _contentDataDao ??= new(this);

        protected async Task LoadKeys()
        {
            try
            {
                HKey = await LoadHKeyAsync();

                var cToken = destroyCancellationToken;

                var hsPath = HSeedFilePath;

                await Task.Run(async () =>
                {
                    if (!File.Exists(hsPath))
                    {
                        var bArr = ChobiLib.GenerateRandomBytes(HSeedByteSize);
                        HSeed = Convert.ToBase64String(bArr);
                        await File.WriteAllTextAsync(hsPath, HSeed);
                    }
                    else
                    {
                        HSeed = await File.ReadAllTextAsync(hsPath);
                    }
                }, cToken);
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

            try
            {
                if (NoEncrypt)
                {
                    _initTcs.TrySetResult(true);
                    return;
                }

                await LoadKeys();
                
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
                _keyInitializeFinished = true;
            }
        }

        public override async Task<T> WithAsyncInBackground<T>(Func<SQLiteConnection, T> func)
        {
            if (!await WaitForSQLiteInitialized())
            {
                return default;
            }

            return await base.WithAsyncInBackground(func);
        }

        public override async Task<T> WithTransactionAsyncInBackground<T>(Func<SQLiteConnection, T> func)
        {
            if (!await WaitForSQLiteInitialized())
            {
                return default;
            }

            return await base.WithTransactionAsyncInBackground(func);
        }
        
    }
}