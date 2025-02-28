using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LAV.RedBlackTree
{
    public sealed partial class ConcurrentRedBlackTree<T> : IAsyncEnumerable<T>
    {
        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            IEnumerable<T> snapshot;
            await WaitForReadLockAsync(cancellationToken);
            try
            {
                snapshot = GetSnapshotInternal();
            }
            finally
            {
                _lock.ExitReadLock();
            }

            foreach (var item in snapshot)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return item;
            }
        }

        public async IAsyncEnumerable<T> GetSnapshotAsync([EnumeratorCancellation] CancellationToken token = default)
        {
            await foreach (var item in this.WithCancellation(token))
            {
                yield return item;
            }
        }
    }
}
