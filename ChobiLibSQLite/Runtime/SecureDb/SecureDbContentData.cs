using System;
using System.Security.Cryptography;
using ChobiLib.Security;
using SQLite.Attributes;
using UnityEngine;

namespace ChobiLib.Unity.SQLite.SecureDb
{
    public class SecureDbContentData
    {
        internal static string GenerateGuidAsId() => Guid.NewGuid().ToString();

        public static SecureDbContentData CreateContentDataFromJson(string json, string contentId = null)
        {
            var hk = ChobiLib.GenerateRandomBytes(32);
            var tOfs = DateTimeOffset.UtcNow;
            var cid = contentId ?? GenerateGuidAsId();
            var hash = CalcHash(hk, cid, json, tOfs);

            return new()
            {
                ContentId = cid,
                Content = json,
                HKey = hk,
                HData = hash,
                CreateTimeOffsetUtc = tOfs,
            };
        }

        public static SecureDbContentData CreateContentDataFromJsonable(IJsonable jsonable, string contentId = null)
        {
            return CreateContentDataFromJson(jsonable.ToJson(), contentId);
        }

        public static SecureDbContentData CreateContentDataFromSerializable(object obj, string contentId = null)
        {
            return CreateContentDataFromJson(JsonUtility.ToJson(obj), contentId);
        }

        public static string GetHashContent(
            byte[] key,
            string cid,
            string content,
            DateTimeOffset dtOfs
        )
        {
            return $"{cid}\n{dtOfs.Ticks}\n{Convert.ToBase64String(key)}\n{content}";
        }

        public static byte[] CalcHash(
            byte[] key,
            string cid,
            string content,
            DateTimeOffset dtOfs
        )
        {
            var json = GetHashContent(key, cid, content, dtOfs);
            return new ChobiHash(key).CalcHash(json.ConvertToByteArray());
        }


        //--- columns

        [PrimaryKey]
        public string ContentId { get; set; }

        public string Content { get; set; }

        public byte[] HKey { get; set; }

        public byte[] HData { get; set; }

        [Indexed]
        public DateTimeOffset CreateTimeOffsetUtc { get; set; }

        public string GetJson()
        {
            if (CheckIsValidData())
            {
                return Content;
            }
            else
            {
                throw new CryptographicException("[SecureDbContentData] Verification failed: computed hash differs from the stored hash.");
            }
        }

        public T ConvertTo<T>()
        {
            return JsonUtility.FromJson<T>(GetJson());
        }


        public bool CheckIsValidData(byte[] data = null) => new ChobiHash(HKey).CompareHash(
            data ?? GetHashContent(
                HKey,
                ContentId,
                Content,
                CreateTimeOffsetUtc
            ).ConvertToByteArray(),
            HData
        );

        public bool CheckIsValidDataWithContent(string content) => new ChobiHash(HKey).CompareHash(
            GetHashContent(
                HKey,
                ContentId,
                content,
                CreateTimeOffsetUtc
            ).ConvertToByteArray(),
            HData
        );

        public SecureDbContentData Copy()
        {
            var scd = (SecureDbContentData)MemberwiseClone();
            scd.HKey = (byte[])HKey.Clone();
            scd.HData = (byte[])HData.Clone();
            return scd;
        }

        public SecureDbContentData CopyWithContent(string content)
        {
            var scd = Copy();
            scd.Content = content;
            return scd;
        }
    }
}