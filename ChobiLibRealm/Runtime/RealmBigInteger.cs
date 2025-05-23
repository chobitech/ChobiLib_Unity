using System;
using System.Numerics;
using MongoDB.Bson;
using Realms;

namespace ChobiLib.Unity.Realm
{
    public class RealmBigInteger : RealmObject
    {
        public static RealmBigInteger CreateNew(BigInteger? bi = null) => new()
        {
            Id = ObjectId.GenerateNewId(),
        };


        [PrimaryKey]
        public ObjectId Id { get; set; }


        public byte[] BigIntRawBytes { get; set; } = Array.Empty<byte>();

        private BigInteger? innerBi;

        [Ignored]
        public BigInteger BigInteger
        {
            get
            {
                innerBi ??= new(BigIntRawBytes);
                return innerBi.Value;
            }
            set
            {
                if (innerBi != value)
                {
                    BigIntRawBytes = value.ToByteArray();
                    innerBi = null;
                }
            }
        }
    }
}