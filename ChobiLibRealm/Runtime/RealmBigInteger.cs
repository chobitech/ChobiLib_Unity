using System;
using System.Numerics;
using System.Security.Cryptography;
using MongoDB.Bson;
using Realms;

namespace ChobiLib.Unity.Realm
{
    public partial class RealmBigInteger : IRealmObject
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

        public static implicit operator BigInteger(RealmBigInteger rbi) => rbi.BigInteger;

        public static RealmBigInteger operator -(RealmBigInteger rbi)
        {
            rbi.BigInteger = -rbi.BigInteger;
            return rbi;
        }

        public static RealmBigInteger operator +(RealmBigInteger rbi, BigInteger bi)
        {
            rbi.BigInteger += bi;
            return rbi;
        }

        public static RealmBigInteger operator -(RealmBigInteger rbi, BigInteger bi)
        {
            rbi.BigInteger -= bi;
            return rbi;
        }


        public static RealmBigInteger operator *(RealmBigInteger rbi, BigInteger bi)
        {
            rbi.BigInteger *= bi;
            return rbi;
        }

        public static RealmBigInteger operator /(RealmBigInteger rbi, BigInteger bi)
        {
            rbi.BigInteger /= bi;
            return rbi;
        }

        public static RealmBigInteger operator ++(RealmBigInteger rbi)
        {
            rbi.BigInteger++;
            return rbi;
        }

        public static RealmBigInteger operator --(RealmBigInteger rbi)
        {
            rbi.BigInteger--;
            return rbi;
        }

    }
}