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
                Debug.LogWarning("SQlite Initialize failed");
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
                    Debug.LogWarning("SQlite Initialize failed");
                }
                return res;
            }
            else
            {
                Debug.LogWarning("SQLite init wait timeout");
                return false;
            }
        }

        private bool _keyInitializeFinished;
        public bool KeyAvailable => HKey != null && HSeed != null;

        protected abstract string HSeedFilePath { get; }

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
                throw new Exception("DB password is null");
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

                await Task.Run(async () =>
                {
                    var hsPath = HSeedFilePath;

                    if (!File.Exists(hsPath))
                    {
                        var bArr = ChobiLib.GenerateRandomBytes(32);
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
                Debug.LogException(ex);
            }
        }

        protected async Task Initialize()
        {
            if (_keyInitializeFinished)
            {
                return;
            }

            try
            {
                await LoadKeys();
                _initTcs.TrySetResult(true);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                _initTcs.TrySetResult(false);
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