using Microsoft.Xna.Framework;

namespace PhysicEngine.Collision.Narrowphase
{
    public struct ReferenceFace
    {
        public int i1, i2;
        public Vector2 v1, v2;
        public Vector2 Normal;

        public Vector2 SideNormal1;
        public float SideOffset1;

        public Vector2 SideNormal2;
        public float SideOffset2;
    }
}
