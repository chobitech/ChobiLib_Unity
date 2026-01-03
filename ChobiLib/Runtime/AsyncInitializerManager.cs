using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChobiLib;
using UnityEngine;

public class AsyncInitializerManager : MonoBehaviour
{
    [SerializeField]
    private List<AsyncInitializer> initializers;

    public bool IsAllInitialized { get; private set; }

    public async Task WaitForAllInitializedAsync()
    {
        while (!IsAllInitialized)
        {
            await Task.Yield();
        }
    }

    public IEnumerator WaitForAllInitializedRoutine()
    {
        while (!IsAllInitialized)
        {
            yield return null;
        }
    }

    public async Task RunInitializersAsync()
    {
        if (IsAllInitialized)
        {
            return;
        }

        if (initializers != null)
        {
            foreach (var i in initializers)
            {
                await i.InitializeAsync();
            }
        }

        IsAllInitialized = true;
    }
}
