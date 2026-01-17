using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using NUnit.Framework.Constraints;
using PlasticPipe.PlasticProtocol.Messages;
using SQLite.Attributes;

namespace ChobiLib.Unity.SQLite.SecureDb
{
    internal class InnerTypeAndPropertyNameData
    {
        public Type type;
        public List<PropertyInfo> propertyList = new();
    }

    internal static class InnerSecureDbContentDataManager
    {
        private static readonly Type IgnoreAttributeType = typeof(IgnoreAttribute);

        private static readonly  Dictionary<Type, InnerTypeAndPropertyNameData> typeDataMap = new();

        public static InnerTypeAndPropertyNameData GetProperties(Type t)
        {
            if (!typeDataMap.TryGetValue(t, out var pData))
            {
                var exceptPName = nameof(ISecureDbContentData.SecureDbContentDataId);
                var pList = t.GetProperties(BindingFlags.Instance |  BindingFlags.Public).Where(p =>
                    p.Name != exceptPName &&
                    p.CanRead &&
                    p.CanWrite &&
                    p.GetMethod != null &&
                    p.SetMethod != null &&
                    p.GetMethod.IsPublic &&
                    p.SetMethod.IsPublic &&
                    !p.IsDefined(IgnoreAttributeType, inherit: true)
                );

                pData = new()
                {
                    type = t,
                    propertyList = pList.ToList(),
                };


                typeDataMap[t] = pData;
            }
            return pData;
        }

        public static InnerTypeAndPropertyNameData GetProperties<T>() => GetProperties(typeof(T));

        public static Dictionary<string, dynamic> InnerToMap<T>(this T t) where T : ISecureDbContentData
        {
            var pData = GetProperties<T>();
            var map = new Dictionary<string, dynamic>();
            foreach (var p in pData.propertyList)
            {
                map[p.Name] = p.GetValue(t);
            }
            return map;
        }
    }

    public interface ISecureDbContentData : IJsonable
    {
        string SecureDbContentDataId { get; set; }

        Dictionary<string, dynamic> IJsonable.ToMap() => this.InnerToMap();
    }

}