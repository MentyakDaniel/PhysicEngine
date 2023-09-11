using PhysicEngine.Collision.ContactSystem;
using PhysicEngine.Collision.Narrowphase;

namespace PhysicEngine.Dynamics.Handler
{
    public delegate void PreSolveHandler(Contact contact, ref Manifold oldManifold);
}
