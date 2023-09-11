using PhysicEngine.Shared;

namespace PhysicEngine.Collision.Broadphase
{
    internal class TreeNode<T>
    {
        internal AABB AABB;

        internal int Child1;
        internal int Child2;

        internal int Height;
        internal int ParentOrNext;

        internal T? UserData;

        internal bool Moved;

        internal bool IsLeaf() => Child1 == DynamicTree<T>.NullNode;
    }
}
