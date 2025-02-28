using System;
using System.Runtime.CompilerServices;

namespace LAV.RedBlackTree
{

    public unsafe struct RbNode<T> where T : unmanaged, IComparable<T>
    {
        public T Key;
        public RbNode<T>* Left;
        public RbNode<T>* Right;
        public RbNode<T>* Parent;
        public NodeColor Color;
    }

    public unsafe class RedBlackTree<T> where T : unmanaged, IComparable<T>
    {
        private RbNode<T>* _root;
        private readonly StackAllocator<RbNode<T>> _allocator;

        public RedBlackTree(int capacity)
        {
            _allocator = new StackAllocator<RbNode<T>>(capacity);
        }

        public bool Insert(T key)
        {
            RbNode<T>* newNode = _allocator.Allocate();
            newNode->Key = key;
            newNode->Color = NodeColor.Red;
            newNode->Left = newNode->Right = newNode->Parent = null;

            RbNode<T>* parent = null;
            RbNode<T>* current = _root;
            while (current != null)
            {
                parent = current;
                int cmp = key.CompareTo(current->Key);
                current = cmp < 0 ? current->Left : current->Right;
            }

            newNode->Parent = parent;
            if (parent == null)
            {
                _root = newNode;
            }
            else if (key.CompareTo(parent->Key) < 0)
            {
                parent->Left = newNode;
            }
            else
            {
                parent->Right = newNode;
            }

            InsertFixup(newNode);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InsertFixup(RbNode<T>* node)
        {
            while (node != _root && node->Parent->Color == NodeColor.Red)
            {
                if (node->Parent == node->Parent->Parent->Left)
                {
                    RbNode<T>* uncle = node->Parent->Parent->Right;
                    if (uncle != null && uncle->Color == NodeColor.Red)
                    {
                        node->Parent->Color = NodeColor.Black;
                        uncle->Color = NodeColor.Black;
                        node->Parent->Parent->Color = NodeColor.Red;
                        node = node->Parent->Parent;
                    }
                    else
                    {
                        if (node == node->Parent->Right)
                        {
                            node = node->Parent;
                            LeftRotate(node);
                        }
                        node->Parent->Color = NodeColor.Black;
                        node->Parent->Parent->Color = NodeColor.Red;
                        RightRotate(node->Parent->Parent);
                    }
                }
                else
                {
                    // Mirror case omitted for brevity
                }
            }
            _root->Color = NodeColor.Black;
        }

        private void LeftRotate(RbNode<T>* node)
        {
            RbNode<T>* rightChild = node->Right;
            node->Right = rightChild->Left;

            if (rightChild->Left != null)
                rightChild->Left->Parent = node;

            rightChild->Parent = node->Parent;

            if (node->Parent == null)
                _root = rightChild;
            else if (node == node->Parent->Left)
                node->Parent->Left = rightChild;
            else
                node->Parent->Right = rightChild;

            rightChild->Left = node;
            node->Parent = rightChild;
        }

        // RightRotate and Delete methods omitted for brevity
    }

    // Fast stack-based allocator for node memory
    public unsafe struct StackAllocator<T> where T : unmanaged
    {
        private readonly T* _buffer;
        private int _index;

        public StackAllocator(int capacity)
        {
            unsafe
            {
                _buffer = stackalloc T[capacity];
            }

            _index = 0;
        }

        public T* Allocate()
        {
            return &_buffer[_index++];
        }
    }
}