using System;
using ChobiLib.Security;
using SQLite.Attributes;
using UnityEditor.UI;

namespace ChobiLib.Unity.SQLite.SecureDb
{
    public class SecureDbContentData
    {
        public static string GetJson(
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
            var json = GetJson(key, cid, tagString, tagInt, content, dtOfs);
            return new ChobiHash(key).CalcHash(json.ConvertToByteArray());
        }

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

        public string ToJson()
        {
            throw new NotImplementedException();
        }

        public byte[] CalcHashData() => new ChobiHash(HKey).CalcHash(ToJson().ConvertToByteArray());

        public SecureDbContentData Copy() => (SecureDbContentData)MemberwiseClone();
    }
}