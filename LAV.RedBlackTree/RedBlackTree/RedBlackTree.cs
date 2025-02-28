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
        private sealed class Node(T key)
        {
            public T Key = key;
            public Node Left;
            public Node Right;
            public Node Parent;
            public NodeColor Color = NodeColor.Red;
        }

        private Node _root;
        private static readonly Node _nil = new Node(default) { Color = NodeColor.Black };

        public RedBlackTree()
        {
            _nil.Left = _nil.Right = _nil.Parent = _nil;
        }

        public bool Insert(T key)
        {
            Node newNode = new Node(key) { Left = _nil, Right = _nil };
            Node parent = _nil;
            Node current = _root;

            while (current != _nil && current != null)
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
            else
                parent.Right = newNode;

            InsertFixup(newNode);
            return true;
        }

        public bool Search(T key)
        {
            return !ReferenceEquals(_nil, FindNode(key));
        }

        public bool Delete(T key)
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
    }
}
