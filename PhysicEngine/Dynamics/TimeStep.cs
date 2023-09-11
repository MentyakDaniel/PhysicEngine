namespace PhysicEngine.Dynamics
{
    internal struct TimeStep
    {
        public float DeltaTime;

        public float DeltaTimeRatio;

        public float InvertedDeltaTime;

        public int VelocityIterations;

        public int PositionIterations;

        public bool WarmStarting;
    }
}
