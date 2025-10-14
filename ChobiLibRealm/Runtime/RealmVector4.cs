namespace ChobiLib.Unity.Realm
{
    using System.Numerics;
    using MongoDB.Bson;
    using Realms;

    public partial class RealmVector4 : IRealmObject
    {
        public static RealmVector4 CreateNew(Vector4? vector4 = null) => new()
        {
            Id = ObjectId.GenerateNewId(),
            Vector4 = vector4 ?? Vector4.Zero
        };

        [PrimaryKey]
        public ObjectId Id { get; set; }


        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }


        [Ignored]
        public Vector4 Vector4
        {
            get => new(X, Y, Z, W);
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
                W = value.W;
            }
        }

        public static implicit operator Vector4(RealmVector4 rv) => rv.Vector4;
    }
}