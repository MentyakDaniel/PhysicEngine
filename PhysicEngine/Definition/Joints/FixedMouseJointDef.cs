﻿using Microsoft.Xna.Framework;
using PhysicEngine.Dynamics.Joints.Misc;

namespace PhysicEngine.Definition.Joints
{
    public sealed class FixedMouseJointDef : JointDef
    {
        public FixedMouseJointDef() : base(JointType.FixedMouse)
        {
            SetDefaults();
        }

        /// <summary>The linear damping in N*s/m</summary>
        public float Damping { get; set; }

        /// <summary>The linear stiffness in N/m</summary>
        public float Stiffness { get; set; }

        /// <summary>The maximum constraint force that can be exerted to move the candidate body. Usually you will express as some
        /// multiple of the weight (multiplier * mass * gravity).</summary>
        public float MaxForce { get; set; }

        /// <summary>The initial world target point. This is assumed to coincide with the body anchor initially.</summary>
        public Vector2 Target { get; set; }

        public override void SetDefaults()
        {
            Target = Vector2.Zero;
            MaxForce = 0.0f;
            Stiffness = 0.0f;
            Damping = 0.0f;
        }
    }
}
