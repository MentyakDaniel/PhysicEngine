namespace PhysicEngine.Dynamics
{
    public struct Profile
    {
        public long Step;

        public long Collide;

        public long Solve;

        public long SolveInit;

        public long SolveVelocity;

        public long SolvePosition;

        public long Broadphase;

        public long SolveTOI;

        public long AddRemoveTime;

        public long NewContactsTime;

        public long ControllersUpdateTime;

        public long BreakableBodies;
    }
}
