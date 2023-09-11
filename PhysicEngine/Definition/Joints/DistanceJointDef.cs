using Microsoft.Xna.Framework;
using PhysicEngine.Dynamics;
using PhysicEngine.Dynamics.Joints.Misc;
using PhysicEngine.Utilities;

namespace PhysicEngine.Definition.Joints
{
    public sealed class DistanceJointDef : JointDef
    {
        public DistanceJointDef() : base(JointType.Distance)
        {
            SetDefaults();
        }

        /// <summary>The linear damping in N*s/m.</summary>
        public float Damping { get; set; }

        /// <summary>The linear stiffness in N/m.</summary>
        public float Stiffness { get; set; }

        /// <summary>The rest length of this joint. Clamped to a stable minimum value.</summary>
        public float Length { get; set; }

        /// <summary>Minimum length. Clamped to a stable minimum value.</summary>
        public float MinLength { get; set; }

        /// <summary>Maximum length. Must be greater than or equal to the minimum length.</summary>
        public float MaxLength { get; set; }

        /// <summary>The local anchor point relative to bodyA's origin.</summary>
        public Vector2 LocalAnchorA { get; set; }

        /// <summary>The local anchor point relative to bodyB's origin.</summary>
        public Vector2 LocalAnchorB { get; set; }

        public void Initialize(Body b1, Body b2, Vector2 anchor1, Vector2 anchor2)
        {
            BodyA = b1;
            BodyB = b2;
            LocalAnchorA = BodyA.GetLocalPoint(anchor1);
            LocalAnchorB = BodyB.GetLocalPoint(anchor2);
            Vector2 d = anchor2 - anchor1;
            Length = MathUtils.Max(d.Length(), Settings.LinearSlop);
            MinLength = Length;
            MaxLength = Length;
        }

        public override void SetDefaults()
        {
            LocalAnchorA = Vector2.Zero;
            LocalAnchorB = Vector2.Zero;
            Length = 1.0f;
            MinLength = 0.0f;
            MaxLength = float.MaxValue;
            Stiffness = 0.0f;
            Damping = 0.0f;
        }
    }
}
