using Microsoft.Xna.Framework;
using PhysicEngine.Definition.Joints;
using PhysicEngine.Shared;
using PhysicEngine.Dynamics.Joints.Misc;
using System.Diagnostics;
using PhysicEngine.Dynamics.Solver;
using PhysicEngine.Utilities;

namespace PhysicEngine.Dynamics.Joints
{
    public class MotorJoint : Joint
    {
        private float _angularError;
        private float _angularImpulse;
        private float _angularMass;
        private float _angularOffset;

        // Solver temp
        private int _indexA;

        private int _indexB;
        private float _invIA;
        private float _invIB;
        private float _invMassA;
        private float _invMassB;
        private Vector2 _linearError;
        private Vector2 _linearImpulse;

        private Matrix2x2 _linearMass;

        // Solver shared
        private Vector2 _linearOffset;

        private Vector2 _localCenterA;
        private Vector2 _localCenterB;
        private float _maxForce;
        private float _maxTorque;
        private Vector2 _rA;
        private Vector2 _rB;

        private float _correctionFactor;

        public MotorJoint(MotorJointDef def)
            : base(def)
        {
            _linearOffset = def.LinearOffset;
            _angularOffset = def.AngularOffset;

            _maxForce = def.MaxForce;
            _maxTorque = def.MaxTorque;
            _correctionFactor = def.CorrectionFactor;
        }

        /// <summary>Constructor for MotorJoint.</summary>
        /// <param name="bodyA">The first body</param>
        /// <param name="bodyB">The second body</param>
        /// <param name="useWorldCoordinates">Set to true if you are using world coordinates as anchors.</param>
        public MotorJoint(Body bodyA, Body bodyB, bool useWorldCoordinates = false)
            : base(bodyA, bodyB, JointType.Motor)
        {
            Vector2 xB = bodyB.Position;

            if (useWorldCoordinates)
                _linearOffset = bodyA.GetLocalPoint(xB);
            else
                _linearOffset = xB;

            _maxForce = 1.0f;
            _maxTorque = 1.0f;
            _correctionFactor = 0.3f;

            _angularOffset = bodyB.Rotation - bodyA.Rotation;
        }

        public override Vector2 WorldAnchorA
        {
            get => _bodyA.Position;
            set => Debug.Assert(false, "You can't set the world anchor on this joint type.");
        }

        public override Vector2 WorldAnchorB
        {
            get => _bodyB.Position;
            set => Debug.Assert(false, "You can't set the world anchor on this joint type.");
        }

        /// <summary>Get/set the maximum friction force in N.</summary>
        public float MaxForce
        {
            set => _maxForce = value;
            get => _maxForce;
        }

        /// <summary>Get/set the maximum friction torque in N*m.</summary>
        public float MaxTorque
        {
            set => _maxTorque = value;
            get => _maxTorque;
        }

        /// <summary>
        /// Get/set the position correction factor in the range [0,1].
        /// </summary>
        public float CorrectionFactor
        {
            set => _correctionFactor = value;
            get => _correctionFactor;
        }

        /// <summary>The linear (translation) offset.</summary>
        public Vector2 LinearOffset
        {
            set
            {
                if (_linearOffset != value)
                {
                    WakeBodies();
                    _linearOffset = value;
                }
            }
            get => _linearOffset;
        }

        /// <summary>Get or set the angular offset.</summary>
        public float AngularOffset
        {
            set
            {
                if (_angularOffset != value)
                {
                    WakeBodies();
                    _angularOffset = value;
                }
            }
            get => _angularOffset;
        }

        public override Vector2 GetReactionForce(float invDt)
        {
            return invDt * _linearImpulse;
        }

        public override float GetReactionTorque(float invDt)
        {
            return invDt * _angularImpulse;
        }

        internal override void InitVelocityConstraints(ref SolverData data)
        {
            _indexA = BodyA.IslandIndex;
            _indexB = BodyB.IslandIndex;
            _localCenterA = BodyA._sweep.LocalCenter;
            _localCenterB = BodyB._sweep.LocalCenter;
            _invMassA = BodyA._invMass;
            _invMassB = BodyB._invMass;
            _invIA = BodyA._invI;
            _invIB = BodyB._invI;

            Vector2 cA = data.Positions[_indexA].C;
            float aA = data.Positions[_indexA].A;
            Vector2 vA = data.Velocities[_indexA].V;
            float wA = data.Velocities[_indexA].W;

            Vector2 cB = data.Positions[_indexB].C;
            float aB = data.Positions[_indexB].A;
            Vector2 vB = data.Velocities[_indexB].V;
            float wB = data.Velocities[_indexB].W;

            Rot qA = new Rot(aA);
            Rot qB = new Rot(aB);

            // Compute the effective mass matrix.
            _rA = MathUtils.Mul(qA, _linearOffset - _localCenterA);
            _rB = MathUtils.Mul(qB, -_localCenterB);

            // J = [-I -r1_skew I r2_skew]
            //     [ 0       -1 0       1]
            // r_skew = [-ry; rx]

            // Matlab
            // K = [ mA+r1y^2*iA+mB+r2y^2*iB,  -r1y*iA*r1x-r2y*iB*r2x,          -r1y*iA-r2y*iB]
            //     [  -r1y*iA*r1x-r2y*iB*r2x, mA+r1x^2*iA+mB+r2x^2*iB,           r1x*iA+r2x*iB]
            //     [          -r1y*iA-r2y*iB,           r1x*iA+r2x*iB,                   iA+iB]

            float mA = _invMassA, mB = _invMassB;
            float iA = _invIA, iB = _invIB;

            // Upper 2 by 2 of K for point to point
            Matrix2x2 K = new ();
            K.ex.X = mA + mB + iA * _rA.Y * _rA.Y + iB * _rB.Y * _rB.Y;
            K.ex.Y = -iA * _rA.X * _rA.Y - iB * _rB.X * _rB.Y;
            K.ey.X = K.ex.Y;
            K.ey.Y = mA + mB + iA * _rA.X * _rA.X + iB * _rB.X * _rB.X;

            _linearMass = K.Inverse;

            _angularMass = iA + iB;
            if (_angularMass > 0.0f)
                _angularMass = 1.0f / _angularMass;

            _linearError = cB + _rB - cA - _rA;
            _angularError = aB - aA - _angularOffset;

            if (data.Step.WarmStarting)
            {
                // Scale impulses to support a variable time step.
                _linearImpulse *= data.Step.DeltaTimeRatio;
                _angularImpulse *= data.Step.DeltaTimeRatio;

                Vector2 P = new Vector2(_linearImpulse.X, _linearImpulse.Y);
                vA -= mA * P;
                wA -= iA * (MathUtils.Cross(_rA, P) + _angularImpulse);
                vB += mB * P;
                wB += iB * (MathUtils.Cross(_rB, P) + _angularImpulse);
            }
            else
            {
                _linearImpulse = Vector2.Zero;
                _angularImpulse = 0.0f;
            }

            data.Velocities[_indexA].V = vA;
            data.Velocities[_indexA].W = wA;
            data.Velocities[_indexB].V = vB;
            data.Velocities[_indexB].W = wB;
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            Vector2 vA = data.Velocities[_indexA].V;
            float wA = data.Velocities[_indexA].W;
            Vector2 vB = data.Velocities[_indexB].V;
            float wB = data.Velocities[_indexB].W;

            float mA = _invMassA, mB = _invMassB;
            float iA = _invIA, iB = _invIB;

            float h = data.Step.DeltaTime;
            float inv_h = data.Step.InvertedDeltaTime;

            // Solve angular friction
            {
                float Cdot = wB - wA + inv_h * _correctionFactor * _angularError;
                float impulse = -_angularMass * Cdot;

                float oldImpulse = _angularImpulse;
                float maxImpulse = h * _maxTorque;
                _angularImpulse = MathUtils.Clamp(_angularImpulse + impulse, -maxImpulse, maxImpulse);
                impulse = _angularImpulse - oldImpulse;

                wA -= iA * impulse;
                wB += iB * impulse;
            }

            // Solve linear friction
            {
                Vector2 Cdot = vB + MathUtils.Cross(wB, _rB) - vA - MathUtils.Cross(wA, _rA) + inv_h * _correctionFactor * _linearError;

                Vector2 impulse = -MathUtils.Mul(ref _linearMass, ref Cdot);
                Vector2 oldImpulse = _linearImpulse;
                _linearImpulse += impulse;

                float maxImpulse = h * _maxForce;

                if (_linearImpulse.LengthSquared() > maxImpulse * maxImpulse)
                {
                    _linearImpulse.Normalize();
                    _linearImpulse *= maxImpulse;
                }

                impulse = _linearImpulse - oldImpulse;

                vA -= mA * impulse;
                wA -= iA * MathUtils.Cross(_rA, impulse);

                vB += mB * impulse;
                wB += iB * MathUtils.Cross(_rB, impulse);
            }

            data.Velocities[_indexA].V = vA;
            data.Velocities[_indexA].W = wA;
            data.Velocities[_indexB].V = vB;
            data.Velocities[_indexB].W = wB;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            return true;
        }
    }
}
