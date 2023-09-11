using PhysicEngine.Shared;

namespace PhysicEngine.Collision.Distance
{
    public struct DistanceInput
    {
        public DistanceProxy ProxyA;
        public DistanceProxy ProxyB;
        public Transform TransformA;
        public Transform TransformB;
        public bool UseRadii;
    }
}
