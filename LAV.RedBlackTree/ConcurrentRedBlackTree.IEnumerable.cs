using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LAV.RedBlackTree
{
    public sealed partial class ConcurrentRedBlackTree<T> : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            return GetSnapshot().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetLazyEnumerator()
        {
            return new Enumerator(this, _root, _nil);
        }

        private IEnumerable<T> GetSnapshotInternal()
        {
            var result = new List<T>();
            InOrderTraversal(_root, result);
            return result.AsReadOnly();
        }
        public IEnumerable<T> GetSnapshot()
        {
            _lock.EnterReadLock();
            try
            {
                return GetSnapshotInternal();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private List<T> ToListInternal()
        {
            var result = new List<T>();
            var stack = new Stack<Node>();
            Node current = _root;

            while (current != _nil || stack.Count > 0)
            {
                while (current != _nil)
                {
                    stack.Push(current);
                    current = current.Left;
                }

                current = stack.Pop();
                result.Add(current.Key);
                current = current.Right;
            }

            return result;
        }
        public List<T> ToList()
        {
            _lock.EnterReadLock();
            try
            {
                return ToListInternal();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private sealed class Enumerator : IEnumerator<T>
        {
            private readonly ConcurrentRedBlackTree<T> _tree;
            private readonly Node _nil;
            private readonly Stack<Node> _stack;
            private Node _current;
            private bool _disposed;
            private bool _lockHeld;

            public Enumerator(ConcurrentRedBlackTree<T> tree, Node root, Node nil)
            {
                _tree = tree;
                _nil = nil;
                _stack = new Stack<Node>();
                _current = _nil;

                // Acquire read lock immediately
                _tree._lock.EnterReadLock();
                _lockHeld = true;

                // Initialize traversal state
                PushLeftSubtree(root);
            }

            public T Current
            {
                get
                {
                    if (_current == _nil || _disposed)
                        throw new InvalidOperationException();

                    return _current.Key;
                }
            }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (_disposed)
                    throw new ObjectDisposedException("Enumerator");

                if (_stack.Count == 0)
                {
                    ReleaseLock();
                    return false;
                }

                _current = _stack.Pop();
                PushLeftSubtree(_current.Right);
                return true;
            }

            public void Reset()
            {
                throw new NotSupportedException(
                    "Reset is not supported for concurrent enumerators");
            }

            public void Dispose()
            {
                if (_disposed) return;

                ReleaseLock();
                _disposed = true;
            }

            private void PushLeftSubtree(Node node)
            {
                while (node != _nil)
                {
                    _stack.Push(node);
                    node = node.Left;
                }
            }

            private void ReleaseLock()
            {
                if (!_lockHeld) return;

                _tree._lock.ExitReadLock();
                _lockHeld = false;
                _current = _nil;
            }

            ~Enumerator()
            {
                if (_lockHeld)
                {
                    ReleaseLock();
                    throw new InvalidOperationException(
                        "Enumerator was not properly disposed");
                }
            }
        }
    }
}
