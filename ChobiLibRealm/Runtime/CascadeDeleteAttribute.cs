

using System;

namespace ChobiLib.Unity.Realm
{
    public class CascadeDeleteAttribute : Attribute
    {
        public static readonly Type SelfType = typeof(CascadeDeleteAttribute);
    }
}