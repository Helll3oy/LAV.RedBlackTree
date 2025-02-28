using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LAV.RedBlackTree
{
    public sealed class AsyncReaderWriterLock
    {
        private const int WAIT_TRIES_COUNT = 5;

        private readonly TimeSpan _maxReadLockTimeout;
        private TimeSpan _maxWriteLockTimeout;

        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _readLock = new SemaphoreSlim(int.MaxValue, int.MaxValue);
        private long _readers = 0;

        public AsyncReaderWriterLock() : this(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30))
        {
            
        }
        public AsyncReaderWriterLock(TimeSpan maxReadLockTimeout, TimeSpan maxWriteLockTimeout)
        {
            _maxReadLockTimeout = maxReadLockTimeout;
            _maxWriteLockTimeout = maxWriteLockTimeout;
        }

        // Waits for the read lock to be available (non-blocking)
        private async Task<bool> WaitForReadLockAsync(CancellationToken cancellationToken = default, 
            TimeSpan? maxWaitTimeout = null, int triesCount = WAIT_TRIES_COUNT)
        {
            if (triesCount == 0 || cancellationToken.IsCancellationRequested) return false;

            //// Wait for the write lock to be available
            //if (Interlocked.Read(ref _readers) == 0)
            //    await _writeLock.WaitAsync(maxWaitTimeout ?? _maxWriteLockTimeout, cancellationToken).ConfigureAwait(false);

            //return Interlocked.Increment(ref _readers) > 0;

            if(!await _readLock.WaitAsync(maxWaitTimeout ?? _maxReadLockTimeout, cancellationToken).ConfigureAwait(false))
            {
                triesCount--;
                return await WaitForReadLockAsync(cancellationToken, maxWaitTimeout, triesCount);
            }

            try
            {
                if (_readers == 0)
                {
                    // Wait for the write lock to be available
                    await _writeLock.WaitAsync(_maxWriteLockTimeout, cancellationToken).ConfigureAwait(false);
                }
                _readers++;

                return true;
            }
            finally
            {
                _readLock.Release();
            }
        }


        private bool WaitForReadLock(CancellationToken cancellationToken = default,
            TimeSpan? maxWaitTimeout = null, int triesCount = WAIT_TRIES_COUNT)
        {
            if (triesCount == 0 || cancellationToken.IsCancellationRequested) return false;

            //// Wait for the write lock to be available
            //if (Interlocked.Read(ref _readers) == 0)
            //    await _writeLock.WaitAsync(maxWaitTimeout ?? _maxWriteLockTimeout, cancellationToken).ConfigureAwait(false);

            //return Interlocked.Increment(ref _readers) > 0;

            if (!_readLock.Wait(maxWaitTimeout ?? _maxReadLockTimeout, cancellationToken))
            {
                triesCount--;
                return WaitForReadLock(cancellationToken, maxWaitTimeout, triesCount);
            }

            try
            {
                if (_readers == 0)
                {
                    // Wait for the write lock to be available
                    _writeLock.Wait(_maxWriteLockTimeout, cancellationToken);
                }
                _readers++;

                return true;
            }
            finally
            {
                _readLock.Release();
            }
        }

        // Enters the read lock (blocks until the lock is acquired)
        public async Task<bool> TryEnterReadLockAsync(CancellationToken cancellationToken = default, TimeSpan? timeout = null)
        {
            return
                await WaitForReadLockAsync(cancellationToken, timeout ?? _maxReadLockTimeout).ConfigureAwait(false) &&
                await _readLock.WaitAsync(timeout ?? _maxReadLockTimeout, cancellationToken).ConfigureAwait(false);
        }

        public bool TryEnterReadLock(CancellationToken cancellationToken = default, TimeSpan? timeout = null)
        {
            return
                WaitForReadLock(cancellationToken, timeout ?? _maxReadLockTimeout) &&
                _readLock.Wait(timeout ?? _maxReadLockTimeout, cancellationToken);
        }

        // Exits the read lock
        public async Task ExitReadLockAsync(CancellationToken cancellationToken = default)
        {
            //if(Interlocked.Decrement(ref _readers) == 0)
            //    _writeLock.Release();

            //_readLock.Release(1);

            await _readLock.WaitAsync(_maxReadLockTimeout, cancellationToken).ConfigureAwait(false);
            try
            {
                _readers--;
                if (_readers == 0)
                {
                    _writeLock.Release();
                }
            }
            finally
            {
                _readLock.Release();
            }
        }

        public void ExitReadLock(CancellationToken cancellationToken = default)
        {
            _readLock.Wait(_maxReadLockTimeout, cancellationToken);
            try
            {
                _readers--;
                if (_readers == 0)
                {
                    _writeLock.Release();
                }
            }
            finally
            {
                _readLock.Release();
            }
        }

        // Waits for the write lock to be available (non-blocking)
        private async Task<bool> WaitForWriteLockAsync(CancellationToken cancellationToken = default, TimeSpan? timeout = null, int triesCount = WAIT_TRIES_COUNT)
        {
            if (triesCount == 0 || cancellationToken.IsCancellationRequested) return false;

            if (!await _writeLock.WaitAsync(timeout ?? _maxWriteLockTimeout, cancellationToken).ConfigureAwait(false))
            {
                triesCount--;
                return await WaitForWriteLockAsync(cancellationToken, timeout, triesCount);
            }

            return true;
        }

        private bool WaitForWriteLock(CancellationToken cancellationToken = default, TimeSpan? timeout = null, int triesCount = WAIT_TRIES_COUNT)
        {
            if (triesCount == 0 || cancellationToken.IsCancellationRequested) return false;

            if (!_writeLock.Wait(timeout ?? _maxWriteLockTimeout, cancellationToken))
            {
                triesCount--;
                return WaitForWriteLock(cancellationToken, timeout, triesCount);
            }

            return true;
        }

        // Enters the write lock (blocks until the lock is acquired)
        public async Task<bool> TryEnterWriteLockAsync(CancellationToken cancellationToken = default, TimeSpan? timeout = null)
        {
            return await WaitForWriteLockAsync(cancellationToken, timeout ?? _maxWriteLockTimeout).ConfigureAwait(false);
        }

        public bool TryEnterWriteLock(CancellationToken cancellationToken = default, TimeSpan? timeout = null)
        {
            return WaitForWriteLock(cancellationToken, timeout ?? _maxWriteLockTimeout);
        }

        // Exits the write lock
        public void ExitWriteLock()
        {
            _writeLock.Release();
        }
    }
}
