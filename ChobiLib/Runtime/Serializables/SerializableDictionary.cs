using System;
using System.Collections;
using System.Collections.Generic;
using ChobiLib;
using UnityEngine;

namespace ChobiLib.Unity.Serializables
{

    [Serializable]
    public class SerializableDictionary<K, V> : IDictionary<K, V>, ISerializationCallbackReceiver
    {
        [Serializable]
        private struct SDictEntry<KK, VV>
        {
            public KK key;
            public VV value;

            public KeyValuePair<KK, VV> ToKeyValuePair() => new(key, value);
        }

        [SerializeField]
        private List<SDictEntry<K, V>> values = new();

        private V InnerGetValue(int index) => values[index].value;


        [NonSerialized]
        private readonly Dictionary<K, int> _indexMap = new();

        public ICollection<K> Keys => values.Map(v => v.key);

        public ICollection<V> Values => values.Map(v => v.value);

        public int Count => values.Count;

        public bool IsReadOnly => false;

        public V this[K key]
        {
            get
            {
                if (TryGetValue(key, out var v))
                {
                    return v;
                }
                throw new KeyNotFoundException();
            }
            set
            {
                var v = new SDictEntry<K, V>()
                {
                    key = key,
                    value = value,
                };
                if (_indexMap.TryGetValue(key, out var i))
                {
                    values[i] = v;
                }
                else
                {
                    _indexMap[key] = values.Count;
                    values.Add(v);
                }
            }
        }

        public void Add(K key, V value) => this[key] = value;

        public bool ContainsKey(K key) => _indexMap.ContainsKey(key);

        public bool Remove(K key)
        {
            if (_indexMap.TryGetValue(key, out var i))
            {
                values.RemoveAt(i);
                _indexMap.Clear();
                for (var n = 0; n < values.Count; n++)
                {
                    _indexMap[values[n].key] = n;
                }
                return true;
            }

            return false;
        }

        public bool TryGetValue(K key, out V value)
        {
            if (_indexMap.TryGetValue(key, out var i))
            {
                value = InnerGetValue(i);
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public void Add(KeyValuePair<K, V> item) => this[item.Key] = item.Value;

        public void Clear()
        {
            values.Clear();
            _indexMap.Clear();
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            if (_indexMap.TryGetValue(item.Key, out var i))
            {
                return InnerGetValue(i).Equals(item.Value);
            }
            return false;
        }


        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            for (var i = arrayIndex; i < array.Length; i++)
            {
                Add(array[i]);
            }
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            if (Contains(item))
            {
                return Remove(item.Key);
            }
            return false;
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => values.Map(v => v.ToKeyValuePair()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            _indexMap.Clear();
            for (var i = 0; i < values.Count; i++)
            {
                _indexMap[values[i].key] = i;
            }
        }
    }
}