using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ChobiLib.Unity
{
    public class ChobiBackgroundWorker : IDisposable
    {
        public const string WorkerThreadNamePrefix = "ChobiBackgroundWorker";

        private readonly BlockingCollection<UnityAction> _queue = new();
        private readonly Thread _workerThread;
        private readonly CancellationTokenSource _cts = new();

        public readonly object SyncLock = new();

        public string ThreadName => _workerThread?.Name;

        public bool IsThreadRunning => _workerThread?.IsAlive == true;
        public bool IsProcessing { get; private set; }
        public int CurrentTasks => _queue.Count;
        public bool HasPendingTasks => CurrentTasks > 0;

        public ChobiBackgroundWorker(string threadName = null)
        {
            var thName = threadName ?? $"{WorkerThreadNamePrefix}-{RandomString.GetCharDigitsRandomString().GetRandomString(8)}";

            _workerThread = new Thread(ProcessQueue)
            {
                Name = thName,
                IsBackground = true,
            };
            _workerThread.Start();
        }

        private void ProcessQueue()
        {
            foreach (var action in _queue.GetConsumingEnumerable(_cts.Token))
            {
                IsProcessing = true;
                try
                {
                    lock (SyncLock)
                    {
                        action();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                finally
                {
                    IsProcessing = false;
                }
            }
        }


        public Task RunInBackground(UnityAction action, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (_queue.IsAddingCompleted)
            {
                return Task.FromException(new InvalidOperationException("Worker is already disposed."));
            }

            token.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<bool>();
            _queue.Add(() =>
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    action();
                    tcs.SetResult(true);
                }
                catch (OperationCanceledException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            token.ThrowIfCancellationRequested();

            return tcs.Task;
        }

        public void Dispose(int waitTimeMs)
        {
            _queue.CompleteAdding();

            if (_workerThread.IsAlive && waitTimeMs > 0)
            {
                _workerThread.Join(waitTimeMs);
            }

            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }

            _cts.Dispose();
            _queue.Dispose();
        }

        public void Dispose() => Dispose(500);
    }
}