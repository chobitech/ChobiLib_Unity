namespace ChobiLib.Unity.Realm
{
    using MongoDB.Bson;
    using UnityEngine;
    using Realms;

    public class RealmColor : RealmObject
    {
        public static RealmColor CreateNew(Color? color = null) => new()
        {
            Id = ObjectId.GenerateNewId(),
            Color = color ?? Color.black
        };

        [PrimaryKey]
        public ObjectId Id { get; set; }


        public float Red { get; set; }
        public float Green { get; set; }
        public float Blue { get; set; }
        public float Alpha { get; set; }


        [Ignored]
        public Color Color
        {
            get => new(Red, Green, Blue, Alpha);
            set
            {
                Red = value.r;
                Green = value.g;
                Blue = value.b;
                Alpha = value.a;
            }
        }

        public static implicit operator Color(RealmColor rc) => rc.Color;
    }

}