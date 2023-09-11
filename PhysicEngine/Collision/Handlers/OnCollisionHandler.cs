using PhysicEngine.Collision.ContactSystem;
using PhysicEngine.Dynamics;

namespace PhysicEngine.Collision.Handlers
{
    public delegate void OnCollisionHandler(Fixture fixtureA, Fixture fixtureB, Contact contact);
}
