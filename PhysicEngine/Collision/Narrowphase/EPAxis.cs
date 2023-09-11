using Microsoft.Xna.Framework;

namespace PhysicEngine.Collision.Narrowphase
{
    public struct EPAxis
    {
        public Vector2 Normal;
        public int Index;
        public float Separation;
        public EPAxisType Type;
    }
}
