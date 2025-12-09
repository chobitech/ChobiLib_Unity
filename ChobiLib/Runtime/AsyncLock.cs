using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChobiLib.Unity
{
    public sealed class AsyncLock
    {
        private readonly SemaphoreSlim _mutex = new(1, 1);

        public async Task<IDisposable> LockAsync()
        {
            await _mutex.WaitAsync();
            return new Releaser(_mutex);
        }

        public IDisposable Lock()
        {
            _mutex.Wait();
            return new Releaser(_mutex);
        }


        private sealed class Releaser : IDisposable
        {
            private readonly SemaphoreSlim _ss;

            public Releaser(SemaphoreSlim ss) => _ss = ss;

            public void Dispose() => _ss.Release();
        }
    }
}