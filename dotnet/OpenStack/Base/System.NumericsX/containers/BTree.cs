#define BTREE_CHECK
using System.Diagnostics;

namespace System.NumericsX
{
    public class BTree<TValue, TKey> where TKey : IComparable<TKey>
    {
        public class Node : BlockAllocElement<Node>
        {
            public TKey key;            // key used for sorting
            public TValue value;           // if != null pointer to object stored in leaf node
            public Node parent;            // parent node
            public Node next;          // next sibling
            public Node prev;          // prev sibling
            public int numChildren;    // number of children
            public Node firstChild;        // first child
            public Node lastChild;     // last child
        }

        int maxChildrenPerNode;

        Node root;
        BlockAlloc<Node> nodeAllocator = new(128);

        public BTree(int maxChildrenPerNode)
        {
            this.maxChildrenPerNode = maxChildrenPerNode;
            Debug.Assert(maxChildrenPerNode >= 4);
            root = null;
        }

        public void Dispose()
            => Shutdown();

        public void Init()
            => root = AllocNode();

        public void Shutdown()
        {
            nodeAllocator.Shutdown();
            root = null;
        }

        public Node Add(TValue value, TKey key)                        // add an object to the tree
        {
            Node node, child, newNode;

            if (root.numChildren >= maxChildrenPerNode)
            {
                newNode = AllocNode();
                newNode.key = root.key;
                newNode.firstChild = root;
                newNode.lastChild = root;
                newNode.numChildren = 1;
                root.parent = newNode;
                SplitNode(root);
                root = newNode;
            }

            newNode = AllocNode();
            newNode.key = key;
            newNode.value = value;

            for (node = root; node.firstChild != null; node = child)
            {
                if (key.CompareTo(node.key) > 0)
                    node.key = key;

                // find the first child with a key larger equal to the key of the new node
                for (child = node.firstChild; child.next != null; child = child.next) if (key.CompareTo(child.key) <= 0) break;

                if (child.value != null)
                {
                    if (key.CompareTo(child.key) <= 0)
                    {
                        // insert new node before child
                        if (child.prev != null) child.prev.next = newNode;
                        else node.firstChild = newNode;
                        newNode.prev = child.prev;
                        newNode.next = child;
                        child.prev = newNode;
                    }
                    else
                    {
                        // insert new node after child
                        if (child.next != null) child.next.prev = newNode;
                        else node.lastChild = newNode;
                        newNode.prev = child;
                        newNode.next = child.next;
                        child.next = newNode;
                    }

                    newNode.parent = node;
                    node.numChildren++;

#if BTREE_CHECK
                    CheckTree();
#endif

                    return newNode;
                }

                // make sure the child has room to store another node
                if (child.numChildren >= maxChildrenPerNode)
                {
                    SplitNode(child);
                    if (key.CompareTo(child.prev.key) <= 0) child = child.prev;
                }
            }

            // we only end up here if the root node is empty
            newNode.parent = root;
            root.key = key;
            root.firstChild = newNode;
            root.lastChild = newNode;
            root.numChildren++;

#if BTREE_CHECK
            CheckTree();
#endif

            return newNode;
        }

        public void Remove(Node node)               // remove an object node from the tree
        {
            Node parent;

            Debug.Assert(node.value != null);

            // unlink the node from it's parent
            if (node.prev != null) node.prev.next = node.next;
            else node.parent.firstChild = node.next;
            if (node.next != null) node.next.prev = node.prev;
            else node.parent.lastChild = node.prev;
            node.parent.numChildren--;

            // make sure there are no parent nodes with a single child
            for (parent = node.parent; parent != root && parent.numChildren <= 1; parent = parent.parent)
            {
                if (parent.next != null) parent = MergeNodes(parent, parent.next);
                else if (parent.prev != null) parent = MergeNodes(parent.prev, parent);

                // a parent may not use a key higher than the key of it's last child
                if (parent.key.CompareTo(parent.lastChild.key) > 0) parent.key = parent.lastChild.key;

                if (parent.numChildren > maxChildrenPerNode) { SplitNode(parent); break; }
            }
            // a parent may not use a key higher than the key of it's last child
            for (; parent != null && parent.lastChild != null; parent = parent.parent) if (parent.key.CompareTo(parent.lastChild.key) > 0) parent.key = parent.lastChild.key;

            // free the node
            FreeNode(node);

            // remove the root node if it has a single internal node as child
            if (root.numChildren == 1 && root.firstChild.value == null)
            {
                var oldRoot = root;
                root.firstChild.parent = null;
                root = root.firstChild;
                FreeNode(oldRoot);
            }

#if BTREE_CHECK
            CheckTree();
#endif
        }

        public TValue Find(TKey key)                                   // find an object using the given key
        {
            for (var node = root.firstChild; node != null; node = node.firstChild)
            {
                while (node.next != null) { if (node.key.CompareTo(key) >= 0) break; node = node.next; }
                if (node.value != null)
                {
                    if (node.key.CompareTo(key) == 0) return node.value;
                    else return default;
                }
            }
            return default;
        }

        public TValue FindSmallestLargerEqual(TKey key)                // find an object with the smallest key larger equal the given key
        {
            for (var node = root.firstChild; node != null; node = node.firstChild)
            {
                while (node.next != null) { if (node.key.CompareTo(key) >= 0) break; node = node.next; }
                if (node.value != null)
                {
                    if (node.key.CompareTo(key) >= 0) return node.value;
                    else return default;
                }
            }
            return default;
        }

        public TValue FindLargestSmallerEqual(TKey key)                // find an object with the largest key smaller equal the given key
        {
            for (var node = root.lastChild; node != null; node = node.lastChild)
            {
                while (node.prev != null) { if (node.key.CompareTo(key) <= 0) break; node = node.prev; }
                if (node.value != null)
                {
                    if (node.key.CompareTo(key) <= 0) return node.value;
                    else return default;
                }
            }
            return default;
        }

        public Node Root => root;                                        // returns the root node of the tree

        public int NodeCount => nodeAllocator.AllocCount;                                  // returns the total number of nodes in the tree

        public Node GetNext(Node node)      // goes through all nodes of the tree
        {
            if (node.firstChild != null) return node.firstChild;
            else
            {
                while (node != null && node.next == null) node = node.parent;
                return node;
            }
        }

        public Node GetNextLeaf(Node node)  // goes through all leaf nodes of the tree
        {
            if (node.firstChild != null)
            {
                while (node.firstChild != null) node = node.firstChild;
                return node;
            }
            else
            {
                while (node != null && node.next == null) node = node.parent;
                if (node != null)
                {
                    node = node.next;
                    while (node.firstChild != null) node = node.firstChild;
                    return node;
                }
                else return null;
            }
        }

        Node AllocNode()
        {
            var node = nodeAllocator.Alloc();
            node.key = default;
            node.parent = null;
            node.next = null;
            node.prev = null;
            node.numChildren = 0;
            node.firstChild = null;
            node.lastChild = null;
            node.value = default;
            return node;
        }

        void FreeNode(Node node)
            => nodeAllocator.Free(node);

        void SplitNode(Node node)
        {
            int i; Node child, newNode;

            // allocate a new node
            newNode = AllocNode();
            newNode.parent = node.parent;

            // divide the children over the two nodes
            child = node.firstChild;
            child.parent = newNode;
            for (i = 3; i < node.numChildren; i += 2) { child = child.next; child.parent = newNode; }

            newNode.key = child.key;
            newNode.numChildren = node.numChildren / 2;
            newNode.firstChild = node.firstChild;
            newNode.lastChild = child;

            node.numChildren -= newNode.numChildren;
            node.firstChild = child.next;

            child.next.prev = null;
            child.next = null;

            // add the new child to the parent before the split node
            Debug.Assert(node.parent.numChildren < maxChildrenPerNode);

            if (node.prev != null) node.prev.next = newNode;
            else node.parent.firstChild = newNode;
            newNode.prev = node.prev;
            newNode.next = node;
            node.prev = newNode;

            node.parent.numChildren++;
        }

        Node MergeNodes(Node node1, Node node2)
        {
            Node child;

            Debug.Assert(node1.parent == node2.parent);
            Debug.Assert(node1.next == node2 && node2.prev == node1);
            Debug.Assert(node1.value == null && node2.value == null);
            Debug.Assert(node1.numChildren >= 1 && node2.numChildren >= 1);

            for (child = node1.firstChild; child.next != null; child = child.next) child.parent = node2;
            child.parent = node2;
            child.next = node2.firstChild;
            node2.firstChild.prev = child;
            node2.firstChild = node1.firstChild;
            node2.numChildren += node1.numChildren;

            // unlink the first node from the parent
            if (node1.prev != null) node1.prev.next = node2;
            else node1.parent.firstChild = node2;
            node2.prev = node1.prev;
            node2.parent.numChildren--;

            FreeNode(node1);

            return node2;
        }

        void CheckTree_r(Node node, ref int numNodes)
        {
            int numChildren; Node child;

            numNodes++;

            // the root node may have zero children and leaf nodes always have zero children, all other nodes should have at least 2 and at most maxChildrenPerNode children
            Debug.Assert(node == root || (node.value != null && node.numChildren == 0) || (node.numChildren >= 2 && node.numChildren <= maxChildrenPerNode));
            // the key of a node may never be larger than the key of it's last child
            Debug.Assert(node.lastChild == null || node.key.CompareTo(node.lastChild.key) <= 0);

            numChildren = 0;
            for (child = node.firstChild; child != null; child = child.next)
            {
                numChildren++;
                // make sure the children are properly linked
                if (child.prev == null) Debug.Assert(node.firstChild == child);
                else Debug.Assert(child.prev.next == child);
                if (child.next == null) Debug.Assert(node.lastChild == child);
                else Debug.Assert(child.next.prev == child);
                // recurse down the tree
                CheckTree_r(child, ref numNodes);
            }
            // the number of children should equal the number of linked children
            Debug.Assert(numChildren == node.numChildren);
        }

        void CheckTree()
        {
            var numNodes = 0; Node node, lastNode;

            CheckTree_r(root, ref numNodes);

            // the number of nodes in the tree should equal the number of allocated nodes
            Debug.Assert(numNodes == nodeAllocator.AllocCount);

            // all the leaf nodes should be ordered
            lastNode = GetNextLeaf(Root);
            if (lastNode != null) for (node = GetNextLeaf(lastNode); node != null; lastNode = node, node = GetNextLeaf(node)) Debug.Assert(lastNode.key.CompareTo(node.key) <= 0);
        }
    }
}