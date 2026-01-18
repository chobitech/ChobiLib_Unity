
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ChobiLib.Unity
{
    public interface IJsonable
    {
        Dictionary<string, dynamic> ToMap();

        string ToJson() => JsonConvert.SerializeObject(ToMap());
    }
}
