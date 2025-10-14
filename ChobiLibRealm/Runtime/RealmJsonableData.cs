using MongoDB.Bson;
using Realms;
using UnityEngine;

namespace ChobiLib.Unity.Realm
{
    public partial class RealmJsonableData : IRealmObject
    {
        [PrimaryKey]
        public ObjectId Id { get; set; }

        [Indexed]
        public string SubIdentifier { get; set; }

        public string JsonString { get; set; }


        public T GetObject<T>() where T : Object
        {
            return JsonUtility.FromJson<T>(JsonString);
        }

        public void SetObject<T>(T obj) where T : Object
        {
            JsonString = JsonUtility.ToJson(obj);
        }

    }
}