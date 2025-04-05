
using System;
using System.IO;
using System.Security.Cryptography;

namespace Chobitech
{
    public static class ChobiEncrypt
    {
        public static byte[] GenerateByteData(int sizeInByte)
        {
            var data = new byte[sizeInByte];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(data);
            }
            return data;
        }

        public static string GetHexString(byte[] data)
        {
            return data.JoinToString("", b => b.ToString("X2"));
        }


        public const int Aes256KeySizeBit = 256;
        public const int Aes256BlockSizeBit = 128;
        public const int Aes256IvSizeByte = 16;

        public static byte[] GenerateAes256Key() => GenerateByteData(Aes256KeySizeBit / 8);


        private static T WithAes256<T>(byte[] key, Func<Aes, T> func)
        {
            using var aes = Aes.Create();

            
            aes.KeySize = Aes256KeySizeBit;
            aes.BlockSize = Aes256BlockSizeBit;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            
            aes.Key = key;

            return func(aes);
        }

        public static byte[] EncryptAes256(byte[] key, byte[] data)
        {
            return WithAes256(key, aes => {
                aes.IV = GenerateByteData(Aes256IvSizeByte);

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using var ms = new MemoryStream();

                ms.Write(aes.IV, 0, aes.IV.Length);

                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                }

                return ms.ToArray();
            });
        }

        public static byte[] DecryptAes256(byte[] key, byte[] encData)
        {
            var iv = new byte[Aes256IvSizeByte];
            Array.Copy(encData, 0, iv, 0, iv.Length);

            return WithAes256(key, aes => {
                aes.IV = iv;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using var ms = new MemoryStream(encData, Aes256IvSizeByte, encData.Length - Aes256IvSizeByte);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var readMs = new MemoryStream();

                cs.CopyTo(readMs);

                return readMs.ToArray();
            });
        }
    }
}
