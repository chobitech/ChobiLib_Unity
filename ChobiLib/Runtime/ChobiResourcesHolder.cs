
/*
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChobiLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace ChobiLib.Unity
{
    public class ChobiResourcesHolder : MonoBehaviour
    {
        private enum LoadObjectType
        {
            Asset, Prefab
        }

        public delegate void OnLoadFinishedEventHandler(ChobiResourcesHolder holder);

        public event OnLoadFinishedEventHandler OnLoadFinished;

        public bool IsLoadFinished { get; private set; } = false;

        private Coroutine loadCoroutine;

        private readonly Dictionary<string, GameObject> prefabMap = new();
        private readonly Dictionary<string, UnityEngine.Object> assetMap = new();

        public IEnumerable<string> PrefabAddresses { get; private set; }

        public IEnumerable<string> AssetAddresses { get; private set; }


        private IEnumerator InnerLoadProcess<T>(IEnumerable<string> addresses, Func<T> loader, UnityAction<bool> onFinished)
        {
            if (addresses == null || !addresses.IsNotEmpty())
            {
                onFinished?.Invoke(false);
                yield break;
            }
        }
    }
}
*/
