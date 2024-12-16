

using System;

namespace Chobitech.Realm
{
    public class CascadeDeleteAttribute : Attribute
    {
        public static readonly Type SelfType = typeof(CascadeDeleteAttribute);
    }
}