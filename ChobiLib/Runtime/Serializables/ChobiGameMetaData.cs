using System;
using UnityEngine;

namespace ChobiLib.Unity.Serializables
{
    [Serializable]
    public class ChobiGameMetaData
    {
        public static ChobiGameMetaData CreateNew()
        {
            var dt = DateTime.Now;
            return new()
            {
                firstStartUpTime = SerializableDateTime.From(dt),
                lastStartUpTime = SerializableDateTime.From(dt),
            };
        }

        [SerializeField]
        private SerializableDateTime firstStartUpTime;
        public DateTime FirstStartUpTime => firstStartUpTime.DateTime;

        [SerializeField]
        private SerializableDateTime lastStartUpTime;
        public DateTime LastStartUpTime
        {
            get => lastStartUpTime.DateTime;
            set => lastStartUpTime.DateTime = value;
        }

        public long totalStartUpCount;

        public long totalPlayTimeSecond;
    }
}
