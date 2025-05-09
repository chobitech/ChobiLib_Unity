using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class AsyncInitializeMonoBehaviour : ChobiMonoBehaviour
{
    private static readonly List<AsyncInitializeMonoBehaviour> globalCheckTargetList = new();

    public static AsyncInitializeMonoBehaviour[] GlobalCheckTargets => globalCheckTargetList.ToArray();

    public static void AddToGlobalCheckTarget(AsyncInitializeMonoBehaviour mb) => globalCheckTargetList.Add(mb);
    public static void RemoveFromGlobalCheckTarget(AsyncInitializeMonoBehaviour mb) => globalCheckTargetList.Remove(mb);

    public static bool IsAllGlobalCheckTargetInitialized
    {
        get
        {
            foreach (var mb in globalCheckTargetList)
            {
                if (!mb.IsInitialized)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public static IEnumerator OnGlobalCheckTargetsInitializedRoutine(UnityAction onAllInitialized)
    {
        while (!IsAllGlobalCheckTargetInitialized)
        {
            yield return null;
        }

        onAllInitialized?.Invoke();
    }

    public static Coroutine ExecuteOnGlobalCheckTargetInitialized(MonoBehaviour mb, UnityAction onAllInitialized)
    {
        return mb.StartCoroutine(
            OnGlobalCheckTargetsInitializedRoutine(onAllInitialized)
        );
    }


    //---


    public bool IsInitialized { get; private set; }

    protected abstract IEnumerator InitializeRoutine();
    protected virtual bool IsGlobalCheckTarget { get; }

    private UnityAction onFinishedActions;

    public void ExecuteOnFinished(UnityAction action)
    {
        if (action == null)
        {
            return;
        }

        if (!IsInitialized)
        {
            onFinishedActions += action;
        }
        else
        {
            action();
        }
    }


    private IEnumerator InnerInitializeRoutine()
    {
        yield return InitializeRoutine();

        IsInitialized = true;
        initializeCoroutine = null;

        onFinishedActions?.Invoke();
        onFinishedActions = null;
    }

    private Coroutine initializeCoroutine;

    public void ResetInitializedState() => IsInitialized = false;

    protected void StartInitialize()
    {
        if (IsInitialized || initializeCoroutine != null)
        {
            return;
        }

        initializeCoroutine = StartCoroutine(InnerInitializeRoutine());
    }

    protected override void Awake()
    {
        base.Awake();

        if (IsGlobalCheckTarget)
        {
            AddToGlobalCheckTarget(this);
        }

        StartInitialize();
    }

    protected virtual void OnDestroy()
    {
        RemoveFromGlobalCheckTarget(this);
    }
}
