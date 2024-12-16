

namespace Chobitech.Realm
{
    using System.Numerics;
    using MongoDB.Bson;
    using Realms;

    public class RealmVector2 : RealmObject
    {
        public static RealmVector2 CreateNew(Vector2? vector2 = null) => new()
        {
            Id = ObjectId.GenerateNewId(),
            Vector2 = vector2 ?? Vector2.Zero
        };

        [PrimaryKey]
        public ObjectId Id { get; set; }


        public float X { get; set; }
        public float Y { get; set; }


        [Ignored]
        public Vector2 Vector2
        {
            get => new(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public static implicit operator Vector2(RealmVector2 rv) => rv.Vector2;
    }
}