using PhysicEngine.Collision.ContactSystem;
using PhysicEngine.Dynamics;

namespace PhysicEngine.Collision.Handlers
{
    public delegate void OnSeparationHandler(Fixture fixtureA, Fixture fixtureB, Contact contact);
}
