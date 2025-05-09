using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ChobiLib.Unity
{
    public abstract class BaseAsyncInitializeMonoBehaviour : SelfObjectHoldingMonoBehaviour
    {
        public bool IsInitialized { get; private set; }

        protected abstract IEnumerator InitializeRoutine();

        protected virtual void InnerOnFinishedAction() { }

        protected virtual bool IsGlobalCheckTarget { get; }

        private Coroutine initializeCoroutine;

        private IEnumerator InnerInitializeRoutine()
        {
            yield return InitializeRoutine();

            IsInitialized = true;
            initializeCoroutine = null;

            InnerOnFinishedAction();
        }

        public void ResetInitializedState() => IsInitialized = false;

        protected void StartInitialize()
        {
            if (IsInitialized || initializeCoroutine != null)
            {
                return;
            }

            initializeCoroutine = StartCoroutine(InnerInitializeRoutine());
        }
    }


    public abstract class AsyncInitializeMonoBehaviour<T> : BaseAsyncInitializeMonoBehaviour where T : AsyncInitializeMonoBehaviour<T>
    {
        private static readonly List<BaseAsyncInitializeMonoBehaviour> globalCheckTargetList = new();

        public static BaseAsyncInitializeMonoBehaviour[] GlobalCheckTargets => globalCheckTargetList.ToArray();

        public static void AddToGlobalCheckTarget(BaseAsyncInitializeMonoBehaviour mb) => globalCheckTargetList.Add(mb);
        public static void RemoveFromGlobalCheckTarget(BaseAsyncInitializeMonoBehaviour mb) => globalCheckTargetList.Remove(mb);

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


        private UnityAction<T> onFinishedActions;
        public void ExecuteOnFinished(UnityAction<T> action)
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
                action((T)(object)this);
            }
        }


        protected override void InnerOnFinishedAction()
        {
            onFinishedActions?.Invoke((T)(object)this);
            onFinishedActions = null;
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

}