using System.Threading.Tasks;
using SqlCipher4Unity3D;
using UnityEngine;
using System;
using System.IO;
using ChobiLib.Security;
using UnityEngine.Events;
using System.Collections.Generic;

namespace ChobiLib.Unity.SQLite.SecureDb
{
    public abstract class AbsChobiSecureSQLiteMonoBehaviour : AbsChobiSQLiteMonoBehaviour
    {
        public bool IsInitialized { get; private set; }

        private readonly Queue<UnityAction> _afterInitializedActionQueue = new();

        public void RunAfterInitialized(UnityAction action)
        {
            if (IsInitialized)
            {
                action();
                return;
            }

            _afterInitializedActionQueue.Enqueue(action);
        }

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
            if (!IsInitialized)
            {
                throw new Exception("DB not initialized");
            }

            if (HKey ==  null || HSeed == null)
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
            if (IsInitialized)
            {
                return;
            }

            try
            {
                HKey = await LoadHKeyAsync();

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

                IsInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        protected async Task Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            try
            {
                await LoadKeys();
                IsInitialized = true;
                while (_afterInitializedActionQueue.TryDequeue(out var act))
                {
                    act?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        
    }
}