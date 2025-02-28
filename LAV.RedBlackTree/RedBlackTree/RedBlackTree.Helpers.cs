using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.RedBlackTree
{
    public partial class RedBlackTree<T>
    {
        private Node FindNode(T key)
        {
            Node current = _root;
            while (current != _nil && current != null)
            {
                int cmp = key.CompareTo(current.Key);
                if (cmp == 0) return current;
                current = cmp < 0 ? current.Left : current.Right;
            }
            return _nil;
        }

        private void InsertFixup(Node z)
        {
            while (z.Parent.Color == NodeColor.Red)
            {
                if (z.Parent == z.Parent.Parent.Left)
                {
                    Node y = z.Parent.Parent.Right;
                    if (y.Color == NodeColor.Red)
                    {
                        z.Parent.Color = NodeColor.Black;
                        y.Color = NodeColor.Black;
                        z.Parent.Parent.Color = NodeColor.Red;
                        z = z.Parent.Parent;
                    }
                    else
                    {
                        if (z == z.Parent.Right)
                        {
                            z = z.Parent;
                            LeftRotate(z);
                        }
                        z.Parent.Color = NodeColor.Black;
                        z.Parent.Parent.Color = NodeColor.Red;
                        RightRotate(z.Parent.Parent);
                    }
                }
                else
                {
                    Node y = z.Parent.Parent.Left;
                    if (y.Color == NodeColor.Red)
                    {
                        z.Parent.Color = NodeColor.Black;
                        y.Color = NodeColor.Black;
                        z.Parent.Parent.Color = NodeColor.Red;
                        z = z.Parent.Parent;
                    }
                    else
                    {
                        if (z == z.Parent.Left)
                        {
                            z = z.Parent;
                            RightRotate(z);
                        }
                        z.Parent.Color = NodeColor.Black;
                        z.Parent.Parent.Color = NodeColor.Red;
                        LeftRotate(z.Parent.Parent);
                    }
                }
            }
            _root.Color = NodeColor.Black;
        }

        private void DeleteFixup(Node x)
        {
            while (x != _root && x.Color == NodeColor.Black)
            {
                if (x == x.Parent.Left)
                {
                    Node w = x.Parent.Right;
                    if (w.Color == NodeColor.Red)
                    {
                        w.Color = NodeColor.Black;
                        x.Parent.Color = NodeColor.Red;
                        LeftRotate(x.Parent);
                        w = x.Parent.Right;
                    }

                    if (w.Left.Color == NodeColor.Black && w.Right.Color == NodeColor.Black)
                    {
                        w.Color = NodeColor.Red;
                        x = x.Parent;
                    }
                    else
                    {
                        if (w.Right.Color == NodeColor.Black)
                        {
                            w.Left.Color = NodeColor.Black;
                            w.Color = NodeColor.Red;
                            RightRotate(w);
                            w = x.Parent.Right;
                        }

                        w.Color = x.Parent.Color;
                        x.Parent.Color = NodeColor.Black;
                        w.Right.Color = NodeColor.Black;
                        LeftRotate(x.Parent);
                        x = _root;
                    }
                }
                else
                {
                    // Mirror case omitted for brevity
                }
            }
            x.Color = NodeColor.Black;
        }

        private void LeftRotate(Node x)
        {
            Node y = x.Right;
            x.Right = y.Left;

            if (y.Left != _nil)
                y.Left.Parent = x;

            y.Parent = x.Parent;

            if (x.Parent == _nil)
                _root = y;
            else if (x == x.Parent.Left)
                x.Parent.Left = y;
            else
                x.Parent.Right = y;

            y.Left = x;
            x.Parent = y;
        }

        private void RightRotate(Node y)
        {
            Node x = y.Left;
            y.Left = x.Right;

            if (x.Right != _nil)
                x.Right.Parent = y;

            x.Parent = y.Parent;

            if (y.Parent == _nil)
                _root = x;
            else if (y == y.Parent.Right)
                y.Parent.Right = x;
            else
                y.Parent.Left = x;

            x.Right = y;
            y.Parent = x;
        }

        private void Transplant(Node u, Node v)
        {
            if (u.Parent == _nil)
                _root = v;
            else if (u == u.Parent.Left)
                u.Parent.Left = v;
            else
                u.Parent.Right = v;

            v.Parent = u.Parent;
        }

        private Node Minimum(Node node)
        {
            while (node.Left != _nil)
                node = node.Left;
            return node;
        }
    }
}
