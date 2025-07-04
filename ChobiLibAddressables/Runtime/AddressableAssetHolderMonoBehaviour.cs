using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ChobiLib.Unity.Addressables
{
    public class AddressableAssetHolderMonoBehaviour : UnityMainThreadHoldingMonoBehaviour
    {
        class InnerAssetData
        {
            public AsyncOperationHandle handle;
            public System.Type type;
            public object obj;
        }


        private readonly Dictionary<string, InnerAssetData> assetDataMap = new();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="loader">execute on unity main thread</param>
        /// <param name="onLoaded">execute on unity main thread</param>
        /// <param name="onLoadFailed">execute on unity main thread</param>
        private async void InnerLoadAssetAsync<T>(string address, System.Func<object, T> loader, UnityAction<T> onLoaded, UnityAction onLoadFailed = null) where T : Object
        {
            if (!assetDataMap.TryGetValue(address, out InnerAssetData data))
            {
                var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(address);
                await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    handle.Release();
                    RunOnUnityThread(onLoadFailed);
                    return;
                }

                assetDataMap[address] = new()
                {
                    handle = handle,
                    type = typeof(T),
                    obj = handle.Result
                };
            }

            if (loader != null && onLoaded != null)
            {
                RunOnUnityThread(() => onLoaded(loader(data.obj)));
            }
        }

        public void LoadPrefab<T>(string address, UnityAction<T> onLoaded, UnityAction onLoadFailed = null, Transform parent = null) where T : Component
        {
            if (parent == null)
            {
                parent = SelfTransform;
            }

            InnerLoadAssetAsync(
                address,
                o => Instantiate(o as GameObject, parent).GetComponent<T>(),
                onLoaded,
                onLoadFailed);
        }

        public void LoadAsset<T>(string address, UnityAction<T> onLoaded, UnityAction onLoadFailed = null) where T : Object
        {
            InnerLoadAssetAsync(
                address,
                o => o as T,
                onLoaded,
                onLoadFailed
            );
        }


        /*
        private IEnumerator InnerLoadRoutine<T>(string address, System.Func<object, T> loader, UnityAction<T> onLoaded, UnityAction onLoadFailed = null)
        {
            if (!assetDataMap.TryGetValue(address, out InnerAssetData data))
            {
                //var handle = UnityEngine.
            }
        }
        */

        protected virtual void OnDestroy()
        {
            foreach (var data in assetDataMap.Values)
            {
                data.handle.Release();
            }
        }
    }
}

/*
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ChobiLib.Unity.Addressables
{
    public class AddressableAssetHolderMonoBehaviour : UnityMainThreadHoldingMonoBehaviour
    {
        private class AddressablesHolder
        {
            AsyncOperationHandle handle;
            Object obj;
        }

        [SerializeField]
        private AssetReference[] assetReferences;

        private readonly Dictionary<System.Type, AddressablesHolder> holderMap = new();

        


        

        private readonly Dictionary<System.Type, AsyncOperationHandle> handleMap = new();

        private readonly Dictionary<System.Type, Object> loadedObjectMap = new();

        private IEnumerator InnerLoadRoutine<T>(System.Func<object, T> loader, UnityAction<T> onLoaded, UnityAction onLoadFailed = null) where T : Object

        {
            var tt = typeof(T);
            if (!handleMap.TryGetValue(tt, out AsyncOperationHandle handle))
            {
                handle = assetReference.LoadAssetAsync<GameObject>();
                yield return handle;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    handle.Release();
                    onLoadFailed?.Invoke();
                    yield break;
                }

                handleMap[tt] = handle;
            }

            if (loader != null && onLoaded != null)
            {
                onLoaded(loader(handle.Result));
            }
        }



        /// <summary>
        /// all function type arguments run in Unity main thread
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="loader"></param>
        /// <param name="onLoaded"></param>
        /// <param name="onLoadFailed"></param>
        private async void InnerAsyncLoad<T>(System.Func<object, T> loader, UnityAction<T> onLoaded, UnityAction onLoadFailed = null) where T : Object
        {
            var tt = typeof(T);
            if (!handleMap.TryGetValue(tt, out AsyncOperationHandle handle))
            {
                handle = assetReference.LoadAssetAsync<GameObject>();
                await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    handle.Release();
                    RunOnUnityThread(onLoadFailed);
                    return;
                }

                handleMap[tt] = handle;
            }

            if (loader != null && onLoaded != null)
            {
                RunOnUnityThread(() =>
                {
                    onLoaded(loader(handle.Result));
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="onLoaded">run on unity main thread</param>
        /// <param name="onLoadFailed">run on unity main thread</param>
        public async void LoadPrefab<T>(UnityAction<T> onLoaded, UnityAction onLoadFailed = null) where T : Component
        {
            InnerAsyncLoad(r => (r as GameObject).GetComponent<T>(), onLoaded, onLoadFailed);
        }


        protected virtual void OnDestroy()
        {
            foreach (var h in handleMap.Values)
            {
                h.Release();
            }
        }
        
    }
}
*/