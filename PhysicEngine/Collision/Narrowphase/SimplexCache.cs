using PhysicEngine.Shared.Optimization;

namespace PhysicEngine.Collision.Narrowphase
{
    public struct SimplexCache
    {
        public ushort Count;

        public FixedArray3<byte> IndexA;

        public FixedArray3<byte> IndexB;

        public float Metric;
    }
}
