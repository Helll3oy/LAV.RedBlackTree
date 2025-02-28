using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LAV.RedBlackTree
{
    public sealed partial class ConcurrentRedBlackTree<T>
    {
        public async Task<bool> InsertAsync(T key, CancellationToken cancellationToken = default)
        {
            await WaitForWriteLockAsync(cancellationToken);
            try
            {
                return InsertInternal(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public async Task<bool> SearchAsync(T key, CancellationToken cancellationToken = default)
        {
            await WaitForReadLockAsync(cancellationToken);
            try
            {
                return SearchInternal(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public async Task<bool> DeleteAsync(T key, CancellationToken cancellationToken = default)
        {
            await WaitForWriteLockAsync(cancellationToken);
            try
            {
                return DeleteInternal(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

#if NET452
        public async Task<IEnumerable<T>> GetSnapshotAsync(CancellationToken cancellationToken = default)
        {
            await WaitForReadLockAsync(cancellationToken);
            try
            {
                return GetSnapshotInternal();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
#endif

        public async Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
        {
            await WaitForReadLockAsync(cancellationToken);
            try
            {
                return ToListInternal();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private async Task WaitForReadLockAsync(CancellationToken ct = default)
        {
            while (true)
            {
                if (_lock.TryEnterReadLock(0)) return;
                await Task.Delay(25, ct).ConfigureAwait(false);
            }
        }

        private async Task WaitForWriteLockAsync(CancellationToken ct = default)
        {
            while (true)
            {
                if (_lock.TryEnterWriteLock(0)) return;
                await Task.Delay(25, ct).ConfigureAwait(false);
            }
        }
    }
}
