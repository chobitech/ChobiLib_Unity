
/*
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ChobiLib.Unity.Security
{

    public class ChobiAES
    {
        public const int KeyByteSize = 32;
        public const int IvByteSize = 16;

        public static byte[] GenerateRandomBytes(int size)
        {
            var bArr = new byte[size];
            ChobiLibUnity.SecureRandom.GetBytes(bArr);
            return bArr;
        }

        private static byte[] EndDec(byte[] data, byte[] key, byte[] iv, bool isDecrypt)
        {
            if (key.Length != KeyByteSize)
            {
                throw new ArgumentException($"The bytes oh the key is {KeyByteSize} bytes");
            }
            if (iv.Length != IvByteSize)
            {
                throw new ArgumentException($"The bytes oh the iv is {IvByteSize} bytes");
            }

            using var aes = Aes.Create();

            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            if (isDecrypt)
            {
                using var decryptor = aes.CreateDecryptor(key, iv);
                using var ms = new MemoryStream(data);
                using var decStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var outMs = new MemoryStream();
                decStream.CopyTo(outMs);
                return outMs.ToArray();
            }
            else
            {
                using var encryptor = aes.CreateEncryptor(key, iv);
                using var ms = new MemoryStream();
                var encStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                encStream.Write(data, 0, data.Length);
                encStream.FlushFinalBlock();
                return ms.ToArray();
            }
        }

        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv) => EndDec(data, key, iv, false);
        public static byte[] Encrypt(string str, byte[] key, byte[] iv) => Encrypt(Encoding.UTF8.GetBytes(str), key, iv);

        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv) => EndDec(data, key, iv, true);
        public static byte[] Decrypt(string str, byte[] key, byte[] iv) => Decrypt(Encoding.UTF8.GetBytes(str), key, iv);


        public byte[] Key { get; private set; }
        public byte[] IV { get; private set; }

        public ChobiAES(byte[] key = null, byte[] iv = null)
        {
            Key = key ?? GenerateRandomBytes(KeyByteSize);
            IV = iv ?? GenerateRandomBytes(IvByteSize);
        }

    }

    

}*/
