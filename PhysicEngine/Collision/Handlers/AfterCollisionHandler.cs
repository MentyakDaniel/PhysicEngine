using PhysicEngine.Collision.ContactSystem;
using PhysicEngine.Dynamics;
using PhysicEngine.Dynamics.Solver;

namespace PhysicEngine.Collision.Handlers
{
    public delegate void AfterCollisionHandler(Fixture fixtureA, Fixture fixtureB, Contact contact, ContactVelocityConstraint impulse);
}
