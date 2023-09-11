using Microsoft.Xna.Framework;

namespace PhysicEngine.Collision.Narrowphase
{
    internal struct SimplexVertex
    {
        public float A;

        public int IndexA;

        public int IndexB;

        public Vector2 W;

        public Vector2 WA;

        public Vector2 WB;
    }
}
