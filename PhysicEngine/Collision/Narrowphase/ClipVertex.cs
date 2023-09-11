using Microsoft.Xna.Framework;
using PhysicEngine.Collision.ContactSystem;

namespace PhysicEngine.Collision.Narrowphase
{
    internal struct ClipVertex
    {
        public ContactId Id;
        public Vector2 V;
    }
}
