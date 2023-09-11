using Microsoft.Xna.Framework;
using PhysicEngine.Dynamics;
using PhysicEngine.Dynamics.Joints.Misc;

namespace PhysicEngine.Definition.Joints
{
    public sealed class PrismaticJointDef : JointDef
    {
        public PrismaticJointDef() : base(JointType.Prismatic)
        {
            SetDefaults();
        }

        /// <summary>Enable/disable the joint limit.</summary>
        public bool EnableLimit { get; set; }

        /// <summary>Enable/disable the joint motor.</summary>
        public bool EnableMotor { get; set; }

        /// <summary>The local anchor point relative to bodyA's origin.</summary>
        public Vector2 LocalAnchorA { get; set; }

        /// <summary>The local anchor point relative to bodyB's origin.</summary>
        public Vector2 LocalAnchorB { get; set; }

        /// <summary>The local translation unit axis in bodyA.</summary>
        public Vector2 LocalAxisA { get; set; }

        /// <summary>The lower translation limit, usually in meters.</summary>
        public float LowerTranslation { get; set; }

        /// <summary>The maximum motor torque, usually in N-m.</summary>
        public float MaxMotorForce { get; set; }

        /// <summary>The desired motor speed in radians per second.</summary>
        public float MotorSpeed { get; set; }

        /// <summary>The constrained angle between the bodies: bodyB_angle - bodyA_angle.</summary>
        public float ReferenceAngle { get; set; }

        /// <summary>The upper translation limit, usually in meters.</summary>
        public float UpperTranslation { get; set; }

        public void Initialize(Body bA, Body bB, Vector2 anchor, Vector2 axis)
        {
            BodyA = bA;
            BodyB = bB;
            LocalAnchorA = BodyA.GetLocalPoint(anchor);
            LocalAnchorB = BodyB.GetLocalPoint(anchor);
            LocalAxisA = BodyA.GetLocalVector(axis);
            ReferenceAngle = BodyB.Rotation - BodyA.Rotation;
        }

        public override void SetDefaults()
        {
            LocalAnchorA = Vector2.Zero;
            LocalAnchorB = Vector2.Zero;
            LocalAxisA = new Vector2(1.0f, 0.0f);
            ReferenceAngle = 0.0f;
            EnableLimit = false;
            LowerTranslation = 0.0f;
            UpperTranslation = 0.0f;
            EnableMotor = false;
            MaxMotorForce = 0.0f;
            MotorSpeed = 0.0f;
        }
    }
}
