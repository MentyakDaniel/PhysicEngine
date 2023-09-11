using Microsoft.Xna.Framework;
using PhysicEngine.Shared.Optimization;

namespace PhysicEngine.Collision.Narrowphase
{
    public struct Manifold
    {
        public Vector2 LocalNormal;

        public Vector2 LocalPoint;

        public int PointCount;

        public FixedArray2<ManifoldPoint> Points;

        public ManifoldType Type;
    }
}
