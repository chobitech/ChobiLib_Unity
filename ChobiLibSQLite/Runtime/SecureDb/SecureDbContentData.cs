using System;
using System.Security.Cryptography;
using ChobiLib.Security;
using SQLite.Attributes;
using UnityEngine;

namespace ChobiLib.Unity.SQLite.SecureDb
{
    public class SecureDbContentData
    {
        public static SecureDbContentData CreateContentDataFromJson(string json, byte[] key = null, string contentId = null, string tagString = null, int? tagInt = null)
        {
            var hk = key ?? ChobiLib.GenerateRandomBytes(32);
            var tOfs = DateTimeOffset.UtcNow;
            var cid = contentId ?? Guid.NewGuid().ToString();
            var hash = CalcHash(hk, cid, tagString, tagInt, json, tOfs);

            return new()
            {
                ContentId = cid,
                Content = json,
                TagString = tagString,
                TagInt = tagInt,
                HKey = hk,
                HData = hash,
                CreateTimeOffsetUtc = tOfs,
            };
        }

        public static SecureDbContentData CreateContentDataFromJsonable(IJsonable jsonable, byte[] key = null, string contentId = null, string tagString = null, int? tagInt = null)
        {
            return CreateContentDataFromJson(jsonable.ToJson(), key, contentId, tagString, tagInt);
        }

        public static SecureDbContentData CreateContentDataFromSerializable(object obj, byte[] key = null, string contentId = null, string tagString = null, int? tagInt = null)
        {
            return CreateContentDataFromJson(JsonUtility.ToJson(obj), key, contentId, tagString, tagInt);
        }

        public static string GetHashContent(
            byte[] key,
            string cid,
            string tagString,
            int? tagInt,
            string content,
            DateTimeOffset dtOfs
        )
        {
            return $"{cid}\n{dtOfs.Ticks}\n{Convert.ToBase64String(key)}\n{tagString ?? ""}\n{tagInt ?? 0}\n{content}";
        }

        public static byte[] CalcHash(
            byte[] key,
            string cid,
            string tagString,
            int? tagInt,
            string content,
            DateTimeOffset dtOfs
        )
        {
            var json = GetHashContent(key, cid, tagString, tagInt, content, dtOfs);
            return new ChobiHash(key).CalcHash(json.ConvertToByteArray());
        }


        //--- columns

        [PrimaryKey]
        public string ContentId { get; set; }

        [Indexed]
        public string TagString { get; set; }

        [Indexed]
        public int? TagInt { get; set; }

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
                TagString,
                TagInt,
                Content,
                CreateTimeOffsetUtc
            ).ConvertToByteArray(),
            HData
        );

        public bool CheckIsValidDataWithContent(string content) => new ChobiHash(HKey).CompareHash(
            GetHashContent(
                HKey,
                ContentId,
                TagString,
                TagInt,
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