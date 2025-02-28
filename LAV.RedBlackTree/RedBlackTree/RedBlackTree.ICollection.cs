using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LAV.RedBlackTree
{
    public partial class RedBlackTree<T> : ICollection<T>
    {
        

        private int _count = 0;
        public int Count
        { 
            get
            {
                _readLocks.Wait();
                try
                {
                    return _count;
                }
                finally
                {
                    _readLocks.Release();
                }
            }
        }

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
