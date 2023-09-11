using Microsoft.Xna.Framework;
using PhysicEngine.Shared;

namespace PhysicEngine.Collision.Distance
{
    public struct ShapeCastInput
    {
        public DistanceProxy ProxyA;
        public DistanceProxy ProxyB;
        public Transform TransformA;
        public Transform TransformB;
        public Vector2 TranslationB;
    }
}
