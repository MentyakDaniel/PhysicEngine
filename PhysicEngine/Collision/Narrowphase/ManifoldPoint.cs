using Microsoft.Xna.Framework;
using PhysicEngine.Collision.ContactSystem;

namespace PhysicEngine.Collision.Narrowphase
{
    public struct ManifoldPoint
    {
        public ContactId Id;

        public Vector2 LocalPoint;

        public float NormalImpulse;

        public float TangentImpulse;
    }
}
