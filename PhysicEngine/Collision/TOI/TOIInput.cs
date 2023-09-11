using PhysicEngine.Collision.Distance;

namespace PhysicEngine.Collision.TOI
{
    public struct TOIInput
    {
        public DistanceProxy ProxyA;
        public DistanceProxy ProxyB;
        public Sweep SweepA;
        public Sweep SweepB;
        public float TMax; 
    }
}
