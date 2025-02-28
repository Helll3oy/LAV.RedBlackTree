using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LAV.RedBlackTree
{
    public partial class RedBlackTree<T> where T : IComparable<T>
    {
        private long _readers;
        private readonly SemaphoreSlim _readLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);

        //private async Task WaitForReadLockAsync(CancellationToken ct = default)
        //{
        //    while (true)
        //    {
        //        if (_lock.TryEnterReadLock(0)) return;
        //        await Task.Delay(25, ct).ConfigureAwait(false);
        //    }
        //}

        //private async Task WaitForWriteLockAsync(CancellationToken ct = default)
        //{
        //    while (true)
        //    {
        //        if (_lock.TryEnterWriteLock(0)) return;
        //        await Task.Delay(25, ct).ConfigureAwait(false);
        //    }
        //}

        // Lock management
        private void EnterReadLock()
        {
            if (Interlocked.Increment(ref _readers) > 0)
                _writeLock.Wait();

            //_readLock.Wait();
            //try
            //{
            //    if (++_readers == 1)
            //        _writeLock.Wait();
            //}
            //finally
            //{
            //    _readLock.Release();
            //}
        }

        private async Task EnterReadLockAsync(CancellationToken ct)
        {
            if (Interlocked.Increment(ref _readers) > 0)
                await _writeLock.WaitAsync(ct);

            //await _readLock.WaitAsync(ct);
            //try
            //{
            //    if (++_readers == 1)
            //        await _writeLock.WaitAsync(ct);
            //}
            //finally
            //{
            //    _readLock.Release();
            //}
        }

        private void ExitReadLock()
        {
            if (Interlocked.Decrement(ref _readers) == 0)
                _writeLock.Release();

            _readLock.Wait();
            try
            {
                if (--_readers == 0)
                    _writeLock.Release();
            }
            finally
            {
                _readLock.Release();
            }
        }

        private void EnterWriteLock()
        {
            _writeLock.Wait();
        }

        private async Task EnterWriteLockAsync(CancellationToken ct)
        {
            await _writeLock.WaitAsync(ct);
        }

        private void ExitWriteLock()
        {
            _writeLock.Release();
        }
    }
}
