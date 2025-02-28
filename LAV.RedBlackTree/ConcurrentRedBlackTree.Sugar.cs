using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LAV.RedBlackTree
{
    public sealed partial class ConcurrentRedBlackTree<T> : IEnumerable<T>
    {
        public IEnumerable<T> GetRange(T min, T max)
        {
            return GetSnapshot().Where(x =>
                x.CompareTo(min) >= 0 &&
                x.CompareTo(max) <= 0
            );
        }
    }
}
