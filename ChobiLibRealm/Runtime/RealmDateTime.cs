namespace Chobitech.Realm
{
    using System;
    using MongoDB.Bson;
    using Realms;

    public class RealmDateTime : RealmObject
    {
        public static RealmDateTime CreateNew(DateTime? dateTime = null) => new()
        {
            Id = ObjectId.GenerateNewId(),
            DateTime = dateTime ?? DateTime.Now
        };
        

        [PrimaryKey]
        public ObjectId Id { get; set; }

        [Indexed]
        public long DateTimeTicksUtc { get; set; }
        
        
        [Ignored]
        public DateTime DateTime
        {
            get => new DateTime(DateTimeTicksUtc).ToLocalTime();
            set => DateTimeTicksUtc = value.ToUniversalTime().Ticks;
        }


        public static implicit operator DateTime(RealmDateTime rdt) => rdt.DateTime;
    }
}