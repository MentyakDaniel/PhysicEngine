﻿namespace PhysicEngine.Dynamics.Joints.Misc
{
    public sealed class JointEdge
    {
        public Joint Joint;

        public JointEdge Next;

        public Body Other;

        public JointEdge Prev;
    }
}
