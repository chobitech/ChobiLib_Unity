using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ChobiLib.Security;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ChobiLib.Unity.SQLite
{
    public class ChobiSQLiteKey
    {
        private const int hSeedByteSize = 48;

        public static async Task<string> LoadHKeyFromTextAsset(string taAddr, CancellationToken token = default)
        {
            return await ChobiUnityThread.RunOnMainThreadAsync(async () =>
            {
                string k = null;
                var handler = Addressables.LoadAssetAsync<TextAsset>(taAddr);
                var ta = await handler.Task;
                if (ta != null)
                {
                    k = ta.text;
                }
                handler.Release();
                return k;
            }, token);
        }

        public static async Task<string> LoadSKey(string filePath, CancellationToken token = default)
        {
            return await ChobiUnityThread.RunOnBackgroundThreadAsync(async () =>
            {
                if (!File.Exists(filePath))
                {
                    var bArr = ChobiLib.GenerateRandomBytes(hSeedByteSize);
                    var s = bArr.ConvertToBase64String();
                    await File.WriteAllTextAsync(filePath, s);
                    return s;
                }
                else
                {
                    return await File.ReadAllTextAsync(filePath);
                }
            }, token);
        }

        public static async Task<ChobiSQLiteKey> LoadKeyData(string hkAddr, string hsFilePath, CancellationToken token = default)
        {
            return await ChobiUnityThread.RunOnBackgroundThreadAsync(async () =>
            {
                var hk = await LoadHKeyFromTextAsset(hkAddr, token);
                var ss = await LoadSKey(hsFilePath);
                return new ChobiSQLiteKey(hk, ss);
            }, token);
        }

        private readonly string _hKey;
        private readonly string _sKey;

        public ChobiSQLiteKey(string hKey, string sKey)
        {
            _hKey = hKey;
            _sKey = sKey;
        }

        public byte[] GetKey()
        {
            var hk = _hKey.ConvertFromBase64();
            var ss = _sKey.ConvertFromBase64();
            return new ChobiHash(hk).CalcHash(ss);
        }

        public string GetKeyString() => GetKey().ConvertToBase64String();
    }
}