using Microsoft.Xna.Framework;

namespace PhysicEngine.Collision.Distance
{
    public struct ShapeCastOutput
    {
        public Vector2 Point;
        public Vector2 Normal;
        public float Lambda;
        public int Iterations;
    }
}
