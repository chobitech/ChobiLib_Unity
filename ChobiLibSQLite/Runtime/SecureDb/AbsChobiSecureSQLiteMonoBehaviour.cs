using System.Threading.Tasks;
using SqlCipher4Unity3D;
using UnityEngine;
using System;
using System.Threading;


namespace ChobiLib.Unity.SQLite.SecureDb
{
    public abstract class AbsChobiSecureSQLiteMonoBehaviour : AbsChobiSQLiteMonoBehaviour
    {
        public bool NoEncrypt { get; protected set; } = false;

        public bool KeyAvailable => KeyData != null;

        public override bool IsDbInitialized => base.IsDbInitialized && KeyAvailable;

        protected abstract string HSeedFilePath { get; }

        protected abstract Task<string> LoadHKeyAsync(CancellationToken token = default);

        protected ChobiSQLiteKey KeyData { get; private set; }

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

            return p;
        }

        public override string DbPassword => GenDbPw();

        protected async Task LoadKeys(CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                var hk = await LoadHKeyAsync(token);
                token.ThrowIfCancellationRequested();
                var hsPath = HSeedFilePath;

                token.ThrowIfCancellationRequested();
                var hSeed = await ChobiSQLiteKey.LoadSKey(hsPath, token);
                token.ThrowIfCancellationRequested();

                KeyData = new(hk, hSeed);
            }
            catch (Exception ex)
            {
                ChobiSQLite.LogException(ex);
                throw ex;
            }
        }

        protected override async Task<ChobiSQLite> OpenChobiSQLite(CancellationToken token = default)
        {
            await LoadKeys(token);
            return await base.OpenChobiSQLite(token);
        }
    }
}