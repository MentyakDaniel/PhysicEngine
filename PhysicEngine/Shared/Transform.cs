using Microsoft.Xna.Framework;

namespace PhysicEngine.Shared
{
    public struct Transform
    {
        public Vector2 p;
        public Rot q;

        public Transform(ref Vector2 position, ref Rot rotation)
        {
            p = position;
            q = rotation;
        }
        public void SetIdentity()
        {
            p = Vector2.Zero;
            q.SetIdentity();
        }
        public void Set(Vector2 position, float angle)
        {
            p = position;
            q.Set(angle);
        }
    }
}
