using PhysicEngine.Dynamics;

namespace PhysicEngine.Collision.ContactSystem
{
    public sealed class ContactEdge
    {
        public Contact? Contact;

        public ContactEdge? Next;

        public Body? Other;

        public ContactEdge? Prev;
    }
}
