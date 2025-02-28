using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LAV.RedBlackTree
{
    public sealed partial class ConcurrentRedBlackTree<T>where T : IComparable<T>
    {
        public void ParallelInsert(IEnumerable<T> items)
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

            Parallel.ForEach(items, options, item =>
            {
                _lock.EnterWriteLock();
                try
                {
                    Insert(item);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            });
        }

        public bool[] ParallelSearch(IEnumerable<T> keys)
        {
            var keyArray = keys.ToArray();
            var results = new bool[keyArray.Length];

            Parallel.For(0, keyArray.Length, i =>
            {
                results[i] = Search(keyArray[i]);
            });

            return results;
        }

        public void ParallelDelete(Func<T, bool> predicate)
        {
            var toDelete = new List<T>();

            // Phase 1: Identify candidates under read lock
            _lock.EnterReadLock();
            try
            {
                ParallelTraverse(node =>
                {
                    if (predicate(node))
                        toDelete.Add(node);
                });
            }
            finally
            {
                _lock.ExitReadLock();
            }

            // Phase 2: Delete under write lock
            _lock.EnterWriteLock();
            try
            {
                Parallel.ForEach(toDelete, key => Delete(key));
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void ParallelTraverse(Action<T> action)
        {
            _lock.EnterReadLock();
            try
            {
                var snapshot = GetAllNodes();
                Parallel.ForEach(snapshot, node => action(node.Key));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private List<Node> GetAllNodes()
        {
            var nodes = new List<Node>();
            InOrderTraversal(_root, nodes);
            return nodes;
        }

        private void InOrderTraversal(Node node, List<Node> nodes)
        {
            if (node == _nil) return;
            InOrderTraversal(node.Left, nodes);
            nodes.Add(node);
            InOrderTraversal(node.Right, nodes);
        }
    }
}
