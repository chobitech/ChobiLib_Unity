using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SqlCipher4Unity3D;
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

        private const string ExceptPropertyName = nameof(AbsSecureDbContentData.SecureDbContentDataId);

        public static InnerTypeAndPropertyNameData GetProperties(Type t)
        {
            if (!typeDataMap.TryGetValue(t, out var pData))
            {
                var pList = t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p =>
                        p.Name != ExceptPropertyName &&
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

        public static Dictionary<string, dynamic> GetValuesMap(Type t, object obj)
        {
            var props = GetProperties(t);
            var res = new Dictionary<string, dynamic>();
            foreach (var p in props.propertyList)
            {
                res[p.Name] = p.GetValue(obj);
            }
            return res;
        }
    }

    public abstract class AbsSecureDbContentData
    {
        private class JsonKeySorter : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                return base.CreateProperties(type, memberSerialization)
                    .OrderBy(p => p.PropertyName).ToList();
            }
        }

        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new JsonKeySorter(),
        };

        public const string ContentIdColumnName = "SecureDbContentDataId";

        public abstract string SecureDbContentDataId { get; set; }

        public virtual Dictionary<string, dynamic> ToMap() => InnerSecureDbContentDataManager.GetValuesMap(GetType(), this);

        public string ToJson() => JsonConvert.SerializeObject(ToMap(), serializerSettings);
    }

    public static class AbsSecureDbContentDataExtensions
    {
        public static SecureDbContentData InsertAbsSecureDbContentData(this SQLiteConnection con, AbsSecureDbContentData data)
        {
            var cid = data.SecureDbContentDataId ??= SecureDbContentData.GenerateGuidAsId();
            con.Insert(data);

            var json = data.ToJson();
            var scd = SecureDbContentData.CreateContentDataFromJson(json, cid, true);

            con.Insert(scd);

            return scd;
        }

        public static SecureDbContentData UpdateAbsSecureDbContentData(this SQLiteConnection con, AbsSecureDbContentData data)
        {
            var cid = data.SecureDbContentDataId ??= SecureDbContentData.GenerateGuidAsId();
            con.Update(data);

            var json = data.ToJson();
            var scd = SecureDbContentData.CreateContentDataFromJson(json, cid, true);

            con.Update(scd);

            return scd;
        }

        public static int DeleteAbsSecureDbContentData<T>(this SQLiteConnection con, T absData) where T : AbsSecureDbContentData, new()
        {
            var cid = absData.SecureDbContentDataId;
            con.Delete<SecureDbContentData>(cid);

            var data = con.Table<T>().Where(d => d.SecureDbContentDataId == cid);
            var ct = 0;
            foreach (var d in data)
            {
                con.Delete(d);
                ct++;
            }

            return ct;
        }



        public static T LoadAbsSecureDbContentData<T>(this SQLiteConnection con, string contentId) where T : AbsSecureDbContentData, new()
        {
            var scd = con.Find<SecureDbContentData>(contentId);
            var data = con.Table<T>().Where(d => d.SecureDbContentDataId == contentId).FirstOrDefault();

            if (scd == null || data == null)
            {
                return null;
            }

            var json = data.ToJson();
            if (!scd.CheckIsValidDataWithContent(data.ToJson()))
            {
                return null;
            }

            return data;
        }

        public static bool TryLoadAbsSecureContentData<T>(this SQLiteConnection con, string contentId, out T outData) where T : AbsSecureDbContentData, new()
        {
            outData = con.LoadAbsSecureDbContentData<T>(contentId);
            return outData != null;
        }
    }

}