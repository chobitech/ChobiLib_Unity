
/*
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chobitech
{
    public class Randomize<T>
    {
        public T[] Seeds { get; private set; }


        public Randomize(IList<T> seeds)
        {
            if (!seeds.IsNotEmpty())
            {
                throw new System.Exception("'seeds' is empty");
            }

            Seeds = seeds.ToArray();
        }


        public T Next() => Seeds[Random.Range(0, Seeds.Length)];

        public List<T> GetRandomArray(int size, bool duplicate = true)
        {
            var res = new List<T>();

            if (duplicate)
            {
                for (var i = 0; i < size; i++)
                {
                    res.Add(Next());
                }
            }
            else
            {
                var tmpList = new List<T>(Seeds);
                var maxLen = System.Math.Min(size, tmpList.Count);
                for (var i = 0; i < maxLen; i++)
                {
                    var n = Random.Range(0, tmpList.Count);
                    res.Add(tmpList[n]);
                    tmpList.RemoveAt(n);
                }
            }

            return res;
        }

    }
}
*/