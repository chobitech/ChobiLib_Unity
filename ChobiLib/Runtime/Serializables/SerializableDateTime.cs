using System;
using UnityEngine;

namespace ChobiLib.Unity.Serializables
{
    [Serializable]
    public struct SerializableDateTime
    {
        public static SerializableDateTime Current => new()
        {
            ticksUtc = DateTime.UtcNow.Ticks,
        };

        public static SerializableDateTime From(DateTime dt) => new()
        {
            ticksUtc = dt.ToUniversalTime().Ticks,
        };

        [SerializeField]
        private long ticksUtc;

        [NonSerialized]
        private DateTime? _innerDt;

        public DateTime DateTime
        {
            get => _innerDt ??= new DateTime(ticksUtc, DateTimeKind.Utc).ToLocalTime();
            set
            {
                var utc = value.ToUniversalTime();
                if (ticksUtc != utc.Ticks)
                {
                    ticksUtc = utc.Ticks;
                    _innerDt = null;
                }
            }
        }

        public static implicit operator DateTime(SerializableDateTime sdt) => sdt.DateTime;
        public static implicit operator SerializableDateTime(DateTime dt) => From(dt);
    }
}