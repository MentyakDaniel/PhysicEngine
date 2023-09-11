using Microsoft.Xna.Framework;
using PhysicEngine.Collision.Raycast;
using PhysicEngine.Shared;
using PhysicEngine.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PhysicEngine.Collision.Broadphase
{
    public class DynamicTree<T>
    {
        public const int NullNode = -1;
        private int _freeList;
        private int _nodeCapacity;
        private int _nodeCount;
        private TreeNode<T>[] _nodes;
        private Stack<int> _queryStack = new Stack<int>(256);
        private Stack<int> _raycastStack = new Stack<int>(256);
        private int _root;

        public int Height
        {
            get
            {
                if (_root == NullNode)
                    return 0;

                return _nodes[_root].Height;
            }
        }

        public float AreaRatio
        {
            get
            {
                if (_root == NullNode)
                    return 0.0f;

                TreeNode<T> root = _nodes[_root];
                float rootArea = root.AABB.Perimeter;

                float totalArea = 0.0f;
                for (int i = 0; i < _nodeCapacity; ++i)
                {
                    TreeNode<T> node = _nodes[i];
                    if (node.Height < 0)
                    {
                        // Free node in pool
                        continue;
                    }

                    totalArea += node.AABB.Perimeter;
                }

                return totalArea / rootArea;
            }
        }

        public int MaxBalance
        {
            get
            {
                int maxBalance = 0;
                for (int i = 0; i < _nodeCapacity; ++i)
                {
                    TreeNode<T> node = _nodes[i];
                    if (node.Height <= 1)
                        continue;

                    Debug.Assert(!node.IsLeaf());

                    int child1 = node.Child1;
                    int child2 = node.Child2;
                    int balance = Math.Abs(_nodes[child2].Height - _nodes[child1].Height);
                    maxBalance = Math.Max(maxBalance, balance);
                }

                return maxBalance;
            }
        }

        public DynamicTree()
        {
            _root = NullNode;

            _nodeCapacity = 16;
            _nodeCount = 0;
            _nodes = new TreeNode<T>[_nodeCapacity];

            // Build a linked list for the free list.
            for (int i = 0; i < _nodeCapacity - 1; ++i)
            {
                _nodes[i] = new TreeNode<T>
                {
                    ParentOrNext = i + 1,
                    Height = 1
                };
            }

            _nodes[_nodeCapacity - 1] = new TreeNode<T>
            {
                ParentOrNext = NullNode,
                Height = 1
            };
            _freeList = 0;
        }

        public int CreateProxy(ref AABB aabb, T userData)
        {
            int proxyId = AllocateNode();

            // Fatten the AABB.
            Vector2 r = new Vector2(Settings.AABBExtension, Settings.AABBExtension);
            _nodes[proxyId].AABB.LowerBound = aabb.LowerBound - r;
            _nodes[proxyId].AABB.UpperBound = aabb.UpperBound + r;
            _nodes[proxyId].UserData = userData;
            _nodes[proxyId].Height = 0;
            _nodes[proxyId].Moved = true;

            InsertLeaf(proxyId);

            return proxyId;
        }

        public void DestroyProxy(int proxyId)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            Debug.Assert(_nodes[proxyId].IsLeaf());

            RemoveLeaf(proxyId);
            FreeNode(proxyId);
        }

        public bool MoveProxy(int proxyId, ref AABB aabb, Vector2 displacement)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);

            Debug.Assert(_nodes[proxyId].IsLeaf());

            // Extend AABB
            AABB fatAABB = new AABB();
            Vector2 r = new Vector2(Settings.AABBExtension, Settings.AABBExtension);
            fatAABB.LowerBound = aabb.LowerBound - r;
            fatAABB.UpperBound = aabb.UpperBound + r;

            // Predict AABB movement
            Vector2 d = Settings.AABBMultiplier * displacement;

            if (d.X < 0.0f)
                fatAABB.LowerBound.X += d.X;
            else
                fatAABB.UpperBound.X += d.X;

            if (d.Y < 0.0f)
                fatAABB.LowerBound.Y += d.Y;
            else
                fatAABB.UpperBound.Y += d.Y;

            AABB treeAABB = _nodes[proxyId].AABB;
            if (treeAABB.Contains(ref aabb))
            {
                // The tree AABB still contains the object, but it might be too large.
                // Perhaps the object was moving fast but has since gone to sleep.
                // The huge AABB is larger than the new fat AABB.
                AABB hugeAABB = new AABB();
                hugeAABB.LowerBound = fatAABB.LowerBound - 4.0f * r;
                hugeAABB.UpperBound = fatAABB.UpperBound + 4.0f * r;

                if (hugeAABB.Contains(ref treeAABB))
                {
                    // The tree AABB contains the object AABB and the tree AABB is
                    // not too large. No tree update needed.
                    return false;
                }

                // Otherwise the tree AABB is huge and needs to be shrunk
            }

            RemoveLeaf(proxyId);

            _nodes[proxyId].AABB = fatAABB;

            InsertLeaf(proxyId);

            _nodes[proxyId].Moved = true;

            return true;
        }

        public bool WasMoved(int proxyId)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            return _nodes[proxyId].Moved;
        }

        public void ClearMoved(int proxyId)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            _nodes[proxyId].Moved = false;
        }

        public T GetUserData(int proxyId)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            return _nodes[proxyId].UserData;
        }

        public void GetFatAABB(int proxyId, out AABB fatAABB)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            fatAABB = _nodes[proxyId].AABB;
        }

        public void Query(Func<int, bool> callback, ref AABB aabb)
        {
            _queryStack.Clear();
            _queryStack.Push(_root);

            while (_queryStack.Count > 0)
            {
                int nodeId = _queryStack.Pop();
                if (nodeId == NullNode)
                    continue;

                TreeNode<T> node = _nodes[nodeId];

                if (AABB.TestOverlap(ref node.AABB, ref aabb))
                {
                    if (node.IsLeaf())
                    {
                        bool proceed = callback(nodeId);
                        if (!proceed)
                            return;
                    }
                    else
                    {
                        _queryStack.Push(node.Child1);
                        _queryStack.Push(node.Child2);
                    }
                }
            }
        }

        public void RayCast(Func<RaycastInput, int, float> callback, ref RaycastInput input)
        {
            Vector2 p1 = input.Point1;
            Vector2 p2 = input.Point2;
            Vector2 r = p2 - p1;
            Debug.Assert(r.LengthSquared() > 0.0f);
            r.Normalize();

            // v is perpendicular to the segment.
            Vector2 absV = MathUtils.Abs(new Vector2(-r.Y, r.X)); //Velcro: Inlined the 'v' variable

            // Separating axis for segment (Gino, p80).
            // |dot(v, p1 - c)| > dot(|v|, h)

            float maxFraction = input.MaxFraction;

            // Build a bounding box for the segment.
            AABB segmentAABB = new AABB();
            {
                Vector2 t = p1 + maxFraction * (p2 - p1);
                Vector2.Min(ref p1, ref t, out segmentAABB.LowerBound);
                Vector2.Max(ref p1, ref t, out segmentAABB.UpperBound);
            }

            _raycastStack.Clear();
            _raycastStack.Push(_root);

            while (_raycastStack.Count > 0)
            {
                int nodeId = _raycastStack.Pop();
                if (nodeId == NullNode)
                    continue;

                TreeNode<T> node = _nodes[nodeId];

                if (!AABB.TestOverlap(ref node.AABB, ref segmentAABB))
                    continue;

                // Separating axis for segment (Gino, p80).
                // |dot(v, p1 - c)| > dot(|v|, h)
                Vector2 c = node.AABB.Center;
                Vector2 h = node.AABB.Extents;
                float separation = Math.Abs(Vector2.Dot(new Vector2(-r.Y, r.X), p1 - c)) - Vector2.Dot(absV, h);
                if (separation > 0.0f)
                    continue;

                if (node.IsLeaf())
                {
                    RaycastInput subInput;
                    subInput.Point1 = input.Point1;
                    subInput.Point2 = input.Point2;
                    subInput.MaxFraction = maxFraction;

                    float value = callback(subInput, nodeId);

                    if (value == 0.0f)
                    {
                        // the client has terminated the raycast.
                        return;
                    }

                    if (value > 0.0f)
                    {
                        // Update segment bounding box.
                        maxFraction = value;
                        Vector2 t = p1 + maxFraction * (p2 - p1);
                        segmentAABB.LowerBound = Vector2.Min(p1, t);
                        segmentAABB.UpperBound = Vector2.Max(p1, t);
                    }
                }
                else
                {
                    _raycastStack.Push(node.Child1);
                    _raycastStack.Push(node.Child2);
                }
            }
        }

        private int AllocateNode()
        {
            // Expand the node pool as needed.
            if (_freeList == NullNode)
            {
                Debug.Assert(_nodeCount == _nodeCapacity);

                // The free list is empty. Rebuild a bigger pool.
                TreeNode<T>[] oldNodes = _nodes;
                _nodeCapacity *= 2;
                _nodes = new TreeNode<T>[_nodeCapacity];
                Array.Copy(oldNodes, _nodes, _nodeCount);

                // Build a linked list for the free list. The parent
                // pointer becomes the "next" pointer.
                for (int i = _nodeCount; i < _nodeCapacity - 1; ++i)
                {
                    _nodes[i] = new TreeNode<T>();
                    _nodes[i].ParentOrNext = i + 1;
                    _nodes[i].Height = -1;
                }
                _nodes[_nodeCapacity - 1] = new TreeNode<T>();
                _nodes[_nodeCapacity - 1].ParentOrNext = NullNode;
                _nodes[_nodeCapacity - 1].Height = -1;
                _freeList = _nodeCount;
            }

            // Peel a node off the free list.
            int nodeId = _freeList;
            _freeList = _nodes[nodeId].ParentOrNext;
            _nodes[nodeId].ParentOrNext = NullNode;
            _nodes[nodeId].Child1 = NullNode;
            _nodes[nodeId].Child2 = NullNode;
            _nodes[nodeId].Height = 0;
            _nodes[nodeId].UserData = default;
            _nodes[nodeId].Moved = false;
            ++_nodeCount;
            return nodeId;
        }

        private void FreeNode(int nodeId)
        {
            Debug.Assert(0 <= nodeId && nodeId < _nodeCapacity);
            Debug.Assert(0 < _nodeCount);
            _nodes[nodeId].ParentOrNext = _freeList;
            _nodes[nodeId].Height = -1;
            _freeList = nodeId;
            --_nodeCount;
        }

        private void InsertLeaf(int leaf)
        {
            if (_root == NullNode)
            {
                _root = leaf;
                _nodes[_root].ParentOrNext = NullNode;
                return;
            }

            // Find the best sibling for this node
            AABB leafAABB = _nodes[leaf].AABB;
            int index = _root;
            while (!_nodes[index].IsLeaf())
            {
                int child1 = _nodes[index].Child1;
                int child2 = _nodes[index].Child2;

                float area = _nodes[index].AABB.Perimeter;

                AABB combinedAABB = new AABB();
                combinedAABB.Combine(ref _nodes[index].AABB, ref leafAABB);
                float combinedArea = combinedAABB.Perimeter;

                // Cost of creating a new parent for this node and the new leaf
                float cost = 2.0f * combinedArea;

                // Minimum cost of pushing the leaf further down the tree
                float inheritanceCost = 2.0f * (combinedArea - area);

                // Cost of descending into child1
                float cost1;
                if (_nodes[child1].IsLeaf())
                {
                    AABB aabb = new AABB();
                    aabb.Combine(ref leafAABB, ref _nodes[child1].AABB);
                    cost1 = aabb.Perimeter + inheritanceCost;
                }
                else
                {
                    AABB aabb = new AABB();
                    aabb.Combine(ref leafAABB, ref _nodes[child1].AABB);
                    float oldArea = _nodes[child1].AABB.Perimeter;
                    float newArea = aabb.Perimeter;
                    cost1 = newArea - oldArea + inheritanceCost;
                }

                // Cost of descending into child2
                float cost2;
                if (_nodes[child2].IsLeaf())
                {
                    AABB aabb = new AABB();
                    aabb.Combine(ref leafAABB, ref _nodes[child2].AABB);
                    cost2 = aabb.Perimeter + inheritanceCost;
                }
                else
                {
                    AABB aabb = new AABB();
                    aabb.Combine(ref leafAABB, ref _nodes[child2].AABB);
                    float oldArea = _nodes[child2].AABB.Perimeter;
                    float newArea = aabb.Perimeter;
                    cost2 = newArea - oldArea + inheritanceCost;
                }

                // Descend according to the minimum cost.
                if (cost < cost1 && cost1 < cost2)
                    break;

                // Descend
                if (cost1 < cost2)
                    index = child1;
                else
                    index = child2;
            }

            int sibling = index;

            // Create a new parent.
            int oldParent = _nodes[sibling].ParentOrNext;
            int newParent = AllocateNode();
            _nodes[newParent].ParentOrNext = oldParent;
            _nodes[newParent].UserData = default;
            _nodes[newParent].AABB.Combine(ref leafAABB, ref _nodes[sibling].AABB);
            _nodes[newParent].Height = _nodes[sibling].Height + 1;

            if (oldParent != NullNode)
            {
                // The sibling was not the root.
                if (_nodes[oldParent].Child1 == sibling)
                    _nodes[oldParent].Child1 = newParent;
                else
                    _nodes[oldParent].Child2 = newParent;

                _nodes[newParent].Child1 = sibling;
                _nodes[newParent].Child2 = leaf;
                _nodes[sibling].ParentOrNext = newParent;
                _nodes[leaf].ParentOrNext = newParent;
            }
            else
            {
                // The sibling was the root.
                _nodes[newParent].Child1 = sibling;
                _nodes[newParent].Child2 = leaf;
                _nodes[sibling].ParentOrNext = newParent;
                _nodes[leaf].ParentOrNext = newParent;
                _root = newParent;
            }

            // Walk back up the tree fixing heights and AABBs
            index = _nodes[leaf].ParentOrNext;
            while (index != NullNode)
            {
                index = Balance(index);

                int child1 = _nodes[index].Child1;
                int child2 = _nodes[index].Child2;

                Debug.Assert(child1 != NullNode);
                Debug.Assert(child2 != NullNode);

                _nodes[index].Height = 1 + Math.Max(_nodes[child1].Height, _nodes[child2].Height);
                _nodes[index].AABB.Combine(ref _nodes[child1].AABB, ref _nodes[child2].AABB);

                index = _nodes[index].ParentOrNext;
            }

            //Validate();
        }

        private void RemoveLeaf(int leaf)
        {
            if (leaf == _root)
            {
                _root = NullNode;
                return;
            }

            int parent = _nodes[leaf].ParentOrNext;
            int grandParent = _nodes[parent].ParentOrNext;
            int sibling;
            if (_nodes[parent].Child1 == leaf)
                sibling = _nodes[parent].Child2;
            else
                sibling = _nodes[parent].Child1;

            if (grandParent != NullNode)
            {
                // Destroy parent and connect sibling to grandParent.
                if (_nodes[grandParent].Child1 == parent)
                    _nodes[grandParent].Child1 = sibling;
                else
                    _nodes[grandParent].Child2 = sibling;
                _nodes[sibling].ParentOrNext = grandParent;
                FreeNode(parent);

                // Adjust ancestor bounds.
                int index = grandParent;
                while (index != NullNode)
                {
                    index = Balance(index);

                    int child1 = _nodes[index].Child1;
                    int child2 = _nodes[index].Child2;

                    _nodes[index].AABB.Combine(ref _nodes[child1].AABB, ref _nodes[child2].AABB);
                    _nodes[index].Height = 1 + Math.Max(_nodes[child1].Height, _nodes[child2].Height);

                    index = _nodes[index].ParentOrNext;
                }
            }
            else
            {
                _root = sibling;
                _nodes[sibling].ParentOrNext = NullNode;
                FreeNode(parent);
            }

            //Validate();
        }

        private int Balance(int iA)
        {
            Debug.Assert(iA != NullNode);

            TreeNode<T> A = _nodes[iA];
            if (A.IsLeaf() || A.Height < 2)
                return iA;

            int iB = A.Child1;
            int iC = A.Child2;
            Debug.Assert(0 <= iB && iB < _nodeCapacity);
            Debug.Assert(0 <= iC && iC < _nodeCapacity);

            TreeNode<T> B = _nodes[iB];
            TreeNode<T> C = _nodes[iC];

            int balance = C.Height - B.Height;

            // Rotate C up
            if (balance > 1)
            {
                int iF = C.Child1;
                int iG = C.Child2;
                TreeNode<T> F = _nodes[iF];
                TreeNode<T> G = _nodes[iG];
                Debug.Assert(0 <= iF && iF < _nodeCapacity);
                Debug.Assert(0 <= iG && iG < _nodeCapacity);

                // Swap A and C
                C.Child1 = iA;
                C.ParentOrNext = A.ParentOrNext;
                A.ParentOrNext = iC;

                // A's old parent should point to C
                if (C.ParentOrNext != NullNode)
                {
                    if (_nodes[C.ParentOrNext].Child1 == iA)
                        _nodes[C.ParentOrNext].Child1 = iC;
                    else
                    {
                        Debug.Assert(_nodes[C.ParentOrNext].Child2 == iA);
                        _nodes[C.ParentOrNext].Child2 = iC;
                    }
                }
                else
                    _root = iC;

                // Rotate
                if (F.Height > G.Height)
                {
                    C.Child2 = iF;
                    A.Child2 = iG;
                    G.ParentOrNext = iA;
                    A.AABB.Combine(ref B.AABB, ref G.AABB);
                    C.AABB.Combine(ref A.AABB, ref F.AABB);

                    A.Height = 1 + Math.Max(B.Height, G.Height);
                    C.Height = 1 + Math.Max(A.Height, F.Height);
                }
                else
                {
                    C.Child2 = iG;
                    A.Child2 = iF;
                    F.ParentOrNext = iA;
                    A.AABB.Combine(ref B.AABB, ref F.AABB);
                    C.AABB.Combine(ref A.AABB, ref G.AABB);

                    A.Height = 1 + Math.Max(B.Height, F.Height);
                    C.Height = 1 + Math.Max(A.Height, G.Height);
                }

                return iC;
            }

            // Rotate B up
            if (balance < -1)
            {
                int iD = B.Child1;
                int iE = B.Child2;
                TreeNode<T> D = _nodes[iD];
                TreeNode<T> E = _nodes[iE];
                Debug.Assert(0 <= iD && iD < _nodeCapacity);
                Debug.Assert(0 <= iE && iE < _nodeCapacity);

                // Swap A and B
                B.Child1 = iA;
                B.ParentOrNext = A.ParentOrNext;
                A.ParentOrNext = iB;

                // A's old parent should point to B
                if (B.ParentOrNext != NullNode)
                {
                    if (_nodes[B.ParentOrNext].Child1 == iA)
                        _nodes[B.ParentOrNext].Child1 = iB;
                    else
                    {
                        Debug.Assert(_nodes[B.ParentOrNext].Child2 == iA);
                        _nodes[B.ParentOrNext].Child2 = iB;
                    }
                }
                else
                    _root = iB;

                // Rotate
                if (D.Height > E.Height)
                {
                    B.Child2 = iD;
                    A.Child1 = iE;
                    E.ParentOrNext = iA;
                    A.AABB.Combine(ref C.AABB, ref E.AABB);
                    B.AABB.Combine(ref A.AABB, ref D.AABB);

                    A.Height = 1 + Math.Max(C.Height, E.Height);
                    B.Height = 1 + Math.Max(A.Height, D.Height);
                }
                else
                {
                    B.Child2 = iE;
                    A.Child1 = iD;
                    D.ParentOrNext = iA;
                    A.AABB.Combine(ref C.AABB, ref D.AABB);
                    B.AABB.Combine(ref A.AABB, ref E.AABB);

                    A.Height = 1 + Math.Max(C.Height, D.Height);
                    B.Height = 1 + Math.Max(A.Height, E.Height);
                }

                return iB;
            }

            return iA;
        }

        public int ComputeHeight(int nodeId)
        {
            Debug.Assert(0 <= nodeId && nodeId < _nodeCapacity);
            TreeNode<T> node = _nodes[nodeId];

            if (node.IsLeaf())
                return 0;

            int height1 = ComputeHeight(node.Child1);
            int height2 = ComputeHeight(node.Child2);
            return 1 + Math.Max(height1, height2);
        }

        public int ComputeHeight()
        {
            int height = ComputeHeight(_root);
            return height;
        }

        public void ValidateStructure(int index)
        {
            if (index == NullNode)
                return;

            if (index == _root)
                Debug.Assert(_nodes[index].ParentOrNext == NullNode);

            TreeNode<T> node = _nodes[index];

            int child1 = node.Child1;
            int child2 = node.Child2;

            if (node.IsLeaf())
            {
                Debug.Assert(child1 == NullNode);
                Debug.Assert(child2 == NullNode);
                Debug.Assert(node.Height == 0);
                return;
            }

            Debug.Assert(0 <= child1 && child1 < _nodeCapacity);
            Debug.Assert(0 <= child2 && child2 < _nodeCapacity);

            Debug.Assert(_nodes[child1].ParentOrNext == index);
            Debug.Assert(_nodes[child2].ParentOrNext == index);

            ValidateStructure(child1);
            ValidateStructure(child2);
        }

        public void ValidateMetrics(int index)
        {
            if (index == NullNode)
                return;

            TreeNode<T> node = _nodes[index];

            int child1 = node.Child1;
            int child2 = node.Child2;

            if (node.IsLeaf())
            {
                Debug.Assert(child1 == NullNode);
                Debug.Assert(child2 == NullNode);
                Debug.Assert(node.Height == 0);
                return;
            }

            Debug.Assert(0 <= child1 && child1 < _nodeCapacity);
            Debug.Assert(0 <= child2 && child2 < _nodeCapacity);

            int height1 = _nodes[child1].Height;
            int height2 = _nodes[child2].Height;
            int height = 1 + Math.Max(height1, height2);
            Debug.Assert(node.Height == height);

            AABB AABB = new AABB();
            AABB.Combine(ref _nodes[child1].AABB, ref _nodes[child2].AABB);

            Debug.Assert(AABB.LowerBound == node.AABB.LowerBound);
            Debug.Assert(AABB.UpperBound == node.AABB.UpperBound);

            ValidateMetrics(child1);
            ValidateMetrics(child2);
        }

        public void Validate()
        {
            ValidateStructure(_root);
            ValidateMetrics(_root);

            int freeCount = 0;
            int freeIndex = _freeList;
            while (freeIndex != NullNode)
            {
                Debug.Assert(0 <= freeIndex && freeIndex < _nodeCapacity);
                freeIndex = _nodes[freeIndex].ParentOrNext;
                ++freeCount;
            }

            Debug.Assert(Height == ComputeHeight());

            Debug.Assert(_nodeCount + freeCount == _nodeCapacity);
        }

        public void RebuildBottomUp()
        {
            int[] nodes = new int[_nodeCount];
            int count = 0;

            // Build array of leaves. Free the rest.
            for (int i = 0; i < _nodeCapacity; ++i)
            {
                if (_nodes[i].Height < 0)
                {
                    // free node in pool
                    continue;
                }

                if (_nodes[i].IsLeaf())
                {
                    _nodes[i].ParentOrNext = NullNode;
                    nodes[count] = i;
                    ++count;
                }
                else
                    FreeNode(i);
            }

            while (count > 1)
            {
                float minCost = MathConstants.MaxFloat;
                int iMin = -1, jMin = -1;
                for (int i = 0; i < count; ++i)
                {
                    AABB AABBi = _nodes[nodes[i]].AABB;

                    for (int j = i + 1; j < count; ++j)
                    {
                        AABB AABBj = _nodes[nodes[j]].AABB;
                        AABB b = new AABB();
                        b.Combine(ref AABBi, ref AABBj);
                        float cost = b.Perimeter;
                        if (cost < minCost)
                        {
                            iMin = i;
                            jMin = j;
                            minCost = cost;
                        }
                    }
                }

                int index1 = nodes[iMin];
                int index2 = nodes[jMin];
                TreeNode<T> child1 = _nodes[index1];
                TreeNode<T> child2 = _nodes[index2];

                int parentIndex = AllocateNode();
                TreeNode<T> parent = _nodes[parentIndex];
                parent.Child1 = index1;
                parent.Child2 = index2;
                parent.Height = 1 + Math.Max(child1.Height, child2.Height);
                parent.AABB.Combine(ref child1.AABB, ref child2.AABB);
                parent.ParentOrNext = NullNode;

                child1.ParentOrNext = parentIndex;
                child2.ParentOrNext = parentIndex;

                nodes[jMin] = nodes[count - 1];
                nodes[iMin] = parentIndex;
                --count;
            }

            _root = nodes[0];

            Validate();
        }

        public void ShiftOrigin(ref Vector2 newOrigin)
        {
            // Build array of leaves. Free the rest.
            for (int i = 0; i < _nodeCapacity; ++i)
            {
                _nodes[i].AABB.LowerBound -= newOrigin;
                _nodes[i].AABB.UpperBound -= newOrigin;
            }
        }
    }
}
