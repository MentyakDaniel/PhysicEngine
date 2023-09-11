using PhysicEngine.Dynamics;

namespace PhysicEngine.Collision.Handlers
{
    public delegate void BroadphaseHandler(ref FixtureProxy proxyA, ref FixtureProxy proxyB);
}
