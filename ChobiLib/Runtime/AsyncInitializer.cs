using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace ChobiLib.Unity
{
    public class AsyncInitializer : MonoBehaviour
    {
        public Func<Task> initializer;

        public bool IsInitialized { get; private set; }



        public void ResetInitializedState()
        {
            IsInitialized = false;
        }

        public async Task InitializeAsync()
        {
            if (IsInitialized)
            {
                return;
            }

            if (initializer != null)
            {
                await initializer();
            }

            IsInitialized = true;
        }


        public async Task WaitForFinishInitializeAsync()
        {
            while (!IsInitialized)
            {
                await Task.Yield();
            }
        }

        public IEnumerator WaitForFinishInitializeRoutine()
        {
            while (!IsInitialized)
            {
                yield return null;
            }
        }

    }
}