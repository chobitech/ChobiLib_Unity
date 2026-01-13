
using System.Collections.Generic;
using UnityEngine;

namespace ChobiLib.Unity
{
    public interface IJsonable
    {
        Dictionary<string, dynamic> ToMap();

        public virtual string ToJson() => JsonUtility.ToJson(ToMap());
        public virtual T Instantiate<T>() => JsonUtility.FromJson<T>(ToJson());
    }
}
