using Microsoft.Xna.Framework;
using PhysicEngine.Dynamics;
using PhysicEngine.Dynamics.Joints.Misc;

namespace PhysicEngine.Definition.Joints
{
    public sealed class RevoluteJointDef : JointDef
    {
        public RevoluteJointDef() : base(JointType.Revolute)
        {
            SetDefaults();
        }

        /// <summary>A flag to enable joint limits.</summary>
        public bool EnableLimit { get; set; }

        /// <summary>A flag to enable the joint motor.</summary>
        public bool EnableMotor { get; set; }

        /// <summary>The local anchor point relative to bodyA's origin.</summary>
        public Vector2 LocalAnchorA { get; set; }

        /// <summary>The local anchor point relative to bodyB's origin.</summary>
        public Vector2 LocalAnchorB { get; set; }

        /// <summary>The lower angle for the joint limit (radians).</summary>
        public float LowerAngle { get; set; }

        /// <summary>The maximum motor torque used to achieve the desired motor speed. Usually in N-m.</summary>
        public float MaxMotorTorque { get; set; }

        /// <summary>The desired motor speed. Usually in radians per second.</summary>
        public float MotorSpeed { get; set; }

        /// <summary>The bodyB angle minus bodyA angle in the reference state (radians).</summary>
        public float ReferenceAngle { get; set; }

        /// <summary>The upper angle for the joint limit (radians).</summary>
        public float UpperAngle { get; set; }

        public void Initialize(Body bA, Body bB, Vector2 anchor)
        {
            BodyA = bA;
            BodyB = bB;
            LocalAnchorA = BodyA.GetLocalPoint(anchor);
            LocalAnchorB = BodyB.GetLocalPoint(anchor);
            ReferenceAngle = BodyB.Rotation - BodyA.Rotation;
        }

        public override void SetDefaults()
        {
            LocalAnchorA = Vector2.Zero;
            LocalAnchorB = Vector2.Zero;
            ReferenceAngle = 0.0f;
            LowerAngle = 0.0f;
            UpperAngle = 0.0f;
            MaxMotorTorque = 0.0f;
            MotorSpeed = 0.0f;
            EnableLimit = false;
            EnableMotor = false;

            base.SetDefaults();
        }
    }
}
