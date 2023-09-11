using PhysicEngine.Shared;

namespace PhysicEngine.Dynamics
{
    public struct FixtureProxy
    {
        public AABB AABB;
        public int ChildIndex;
        public Fixture Fixture;
        public int ProxyId;
    }
}
