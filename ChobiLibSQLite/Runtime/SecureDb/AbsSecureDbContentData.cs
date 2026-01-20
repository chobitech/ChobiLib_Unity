using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
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
    }

    public abstract class AbsSecureDbContentData : IUnityJsonable
    {
        public abstract string SecureDbContentDataId { get; set; }

        public virtual Dictionary<string, dynamic> ToMap()
        {
            var pRes = InnerSecureDbContentDataManager.GetProperties(GetType());
            var map = new Dictionary<string, dynamic>();
            foreach (var p in pRes.propertyList)
            {
                map[p.Name] = p.GetValue(this);
            }
            return map;
        }

        public string ToJson()
        {
            var map = ToMap();
            return JsonConvert.SerializeObject(map);
        }
    }

    public static class AbsSecureDbContentDataExtensions
    {
        public static SecureDbContentData InsertAbsSecureDbContentData(this SQLiteConnection con, AbsSecureDbContentData data)
        {
            var cid = data.SecureDbContentDataId ??= SecureDbContentData.GenerateGuidAsId();
            con.Insert(data);

            var json = data.ToJson();
            var scd = SecureDbContentData.CreateContentDataFromJson(json, cid);

            con.Insert(scd);

            return scd;
        }

        public static SecureDbContentData UpdateAbsSecureDbContentData(this SQLiteConnection con, AbsSecureDbContentData data)
        {
            var cid = data.SecureDbContentDataId ??= SecureDbContentData.GenerateGuidAsId();
            con.Update(data);

            var json = data.ToJson();
            var scd = SecureDbContentData.CreateContentDataFromJson(json, cid);

            con.Update(scd);

            return scd;
        }

        public static int DeleteAbsSecureDbContentData<T>(this SQLiteConnection con, string contentId) where T : AbsSecureDbContentData, new()
        {
            con.Delete<SecureDbContentData>(contentId);
            var data = con.Table<T>().Where(d => d.SecureDbContentDataId == contentId);
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
                return default;
            }

            var json = data.ToJson();
            if (!scd.CheckIsValidDataWithContent(data.ToJson()))
            {
                return default;
            }

            return data;
        }
    }

}