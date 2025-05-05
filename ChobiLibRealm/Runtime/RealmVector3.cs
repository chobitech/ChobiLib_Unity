namespace ChobiLib.Unity.Realm
{
    using System.Numerics;
    using MongoDB.Bson;
    using Realms;

    public class RealmVector3 : RealmObject
    {
        public static RealmVector3 CreateNew(Vector3? vector3 = null) => new()
        {
            Id = ObjectId.GenerateNewId(),
            Vector3 = vector3 ?? Vector3.Zero
        };

        [PrimaryKey]
        public ObjectId Id { get; set; }


        public float X { get; set; }
        public float Y { get; set; }

        public float Z { get; set; }



        [Ignored]
        public Vector3 Vector3
        {
            get => new(X, Y, Z);
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
            }
        }

        public static implicit operator Vector3(RealmVector3 rv) => rv.Vector3;
    }
}