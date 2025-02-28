using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LAV.RedBlackTree
{
    public sealed partial class ConcurrentRedBlackTree<T> where T : IComparable<T>
    {
        private sealed class Node
        {
            public T Key;
            public Node Left;
            public Node Right;
            public Node Parent;
            public NodeColor Color;

            public Node(T key)
            {
                Key = key;
                Color = NodeColor.Red;
            }
        }

        private Node _root;
        private readonly Node _nil;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public ConcurrentRedBlackTree()
        {
            _nil = new Node(default(T)) { Color = NodeColor.Black };
            _nil.Left = _nil.Right = _nil.Parent = _nil;
            _root = _nil;
        }

        private bool InsertInternal(T key)
        {
            Node newNode = new Node(key) { Left = _nil, Right = _nil };
            Node parent = _nil;
            Node current = _root;

            while (current != _nil)
            {
                parent = current;
                int cmp = key.CompareTo(current.Key);
                current = cmp < 0 ? current.Left : current.Right;
            }

            newNode.Parent = parent;
            if (parent == _nil)
                _root = newNode;
            else if (key.CompareTo(parent.Key) < 0)
                parent.Left = newNode;
            else if (key.CompareTo(parent.Key) > 0)
                parent.Right = newNode;
            else
                return false; // Duplicate

            InsertFixup(newNode);
            return true;
        }
        public bool Insert(T key)
        {
            _lock.EnterWriteLock();
            try
            {
                return InsertInternal(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private bool SearchInternal(T key)
        {
            Node current = _root;
            while (current != _nil)
            {
                int cmp = key.CompareTo(current.Key);
                if (cmp == 0) return true;
                current = cmp < 0 ? current.Left : current.Right;
            }
            return false;
        }
        public bool Search(T key)
        {
            _lock.EnterReadLock();
            try
            {
                return SearchInternal(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private bool DeleteInternal(T key)
        {
            Node z = FindNode(key);
            if (z == _nil) return false;

            Node y = z;
            NodeColor originalColor = y.Color;
            Node x;

            if (z.Left == _nil)
            {
                x = z.Right;
                Transplant(z, z.Right);
            }
            else if (z.Right == _nil)
            {
                x = z.Left;
                Transplant(z, z.Left);
            }
            else
            {
                y = Minimum(z.Right);
                originalColor = y.Color;
                x = y.Right;

                if (y.Parent == z)
                    x.Parent = y;
                else
                {
                    Transplant(y, y.Right);
                    y.Right = z.Right;
                    y.Right.Parent = y;
                }

                Transplant(z, y);
                y.Left = z.Left;
                y.Left.Parent = y;
                y.Color = z.Color;
            }

            if (originalColor == NodeColor.Black)
                DeleteFixup(x);

            return true;
        }
        public bool Delete(T key)
        {
            _lock.EnterWriteLock();
            try
            {
                return DeleteInternal(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void BatchOperation(Action<ConcurrentRedBlackTree<T>> action)
        {
            _lock.EnterWriteLock();
            try
            {
                action(this);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public T ReadOperation(Func<ConcurrentRedBlackTree<T>, T> func)
        {
            _lock.EnterReadLock();
            try
            {
                return func(this);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

           
    }
}
