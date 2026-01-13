
using UnityEngine;

namespace ChobiLib.Unity
{
    public interface IJsonable
    {
        string ToJson();

        public virtual T Instantiate<T>() => JsonUtility.FromJson<T>(ToJson());
    }
}