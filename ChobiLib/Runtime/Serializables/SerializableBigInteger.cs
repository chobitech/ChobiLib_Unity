using System;
using System.Linq;
using System.Numerics;
using UnityEngine;

namespace ChobiLib.Unity.Serializables
{
    [Serializable]
    public struct SerializableBigInteger
    {
        [SerializeField]
        private byte[] biBytes;

        [NonSerialized]
        private BigInteger? _innerBi;

        public BigInteger BigInteger
        {
            get
            {
                if (_innerBi == null)
                {
                    if (biBytes == null || !biBytes.Any())
                    {
                        _innerBi = BigInteger.Zero;
                    }
                    else
                    {
                        _innerBi = new(biBytes);
                    }
                }
                return _innerBi.Value;
            }
            set
            {
                if (_innerBi != value)
                {
                    biBytes = value.ToByteArray();
                    _innerBi = value;
                }
            }
        }

        public static implicit operator BigInteger(SerializableBigInteger sbi) => sbi.BigInteger;
        public static implicit operator SerializableBigInteger(BigInteger bi) => new()
        {
            biBytes = bi.ToByteArray(),
            _innerBi = bi,
        };
    }
}