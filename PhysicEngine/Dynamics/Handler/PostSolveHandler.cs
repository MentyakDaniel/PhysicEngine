using PhysicEngine.Collision.ContactSystem;
using PhysicEngine.Dynamics.Solver;

namespace PhysicEngine.Dynamics.Handler
{
    public delegate void PostSolveHandler(Contact contact, ContactVelocityConstraint impulse);
}
