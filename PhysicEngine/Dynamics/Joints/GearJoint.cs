﻿using Microsoft.Xna.Framework;
using PhysicEngine.Definition.Joints;
using PhysicEngine.Dynamics.Joints.Misc;
using PhysicEngine.Dynamics.Solver;
using PhysicEngine.Shared;
using PhysicEngine.Utilities;
using System.Diagnostics;

namespace PhysicEngine.Dynamics.Joints
{
    public class GearJoint : Joint
    {
        private Body _bodyC;
        private Body _bodyD;

        private float _constant;
        private float _iA, _iB, _iC, _iD;

        private float _impulse;

        // Solver temp
        private int _indexA, _indexB, _indexC, _indexD;

        private Vector2 _JvAC, _JvBD;
        private float _JwA, _JwB, _JwC, _JwD;
        private Vector2 _lcA, _lcB, _lcC, _lcD;

        // Solver shared
        private Vector2 _localAnchorA;
        private Vector2 _localAnchorB;
        private Vector2 _localAnchorC;
        private Vector2 _localAnchorD;

        private Vector2 _localAxisC;
        private Vector2 _localAxisD;
        private float _mA, _mB, _mC, _mD;
        private float _mass;
        private float _ratio;

        private float _referenceAngleA;
        private float _referenceAngleB;
        private JointType _typeA;
        private JointType _typeB;
        private readonly Joint _jointB;
        private readonly Joint _jointA;

        public GearJoint(GearJointDef def) : base(def)
        {
            _jointA = def.JointA;
            _jointB = def.JointB;

            _typeA = _jointA.JointType;
            _typeB = _jointB.JointType;

            Debug.Assert(_typeA == JointType.Revolute || _typeA == JointType.Prismatic);
            Debug.Assert(_typeB == JointType.Revolute || _typeB == JointType.Prismatic);

            float coordinateA, coordinateB;

            // TODO_ERIN there might be some problem with the joint edges in b2Joint.

            _bodyC = _jointA.BodyA;
            _bodyA = _jointA.BodyB;

            // Body B on joint1 must be dynamic
            Debug.Assert(_bodyA._type == BodyType.Dynamic);

            // Get geometry of joint1
            Transform xfA = _bodyA._xf;
            float aA = _bodyA._sweep.A;
            Transform xfC = _bodyC._xf;
            float aC = _bodyC._sweep.A;

            if (_typeA == JointType.Revolute)
            {
                RevoluteJoint revolute = (RevoluteJoint)def.JointA;
                _localAnchorC = revolute._localAnchorA;
                _localAnchorA = revolute._localAnchorB;
                _referenceAngleA = revolute._referenceAngle;
                _localAxisC = Vector2.Zero;

                coordinateA = aA - aC - _referenceAngleA;
            }
            else
            {
                PrismaticJoint prismatic = (PrismaticJoint)def.JointA;
                _localAnchorC = prismatic._localAnchorA;
                _localAnchorA = prismatic._localAnchorB;
                _referenceAngleA = prismatic._referenceAngle;
                _localAxisC = prismatic._localXAxisA;

                Vector2 pC = _localAnchorC;
                Vector2 pA = MathUtils.MulT(xfC.q, MathUtils.Mul(xfA.q, _localAnchorA) + (xfA.p - xfC.p));
                coordinateA = MathUtils.Dot(pA - pC, _localAxisC);
            }

            _bodyD = _jointB.BodyA;
            _bodyB = _jointB.BodyB;

            // Body B on joint2 must be dynamic
            Debug.Assert(_bodyB._type == BodyType.Dynamic);

            // Get geometry of joint2
            Transform xfB = _bodyB._xf;
            float aB = _bodyB._sweep.A;
            Transform xfD = _bodyD._xf;
            float aD = _bodyD._sweep.A;

            if (_typeB == JointType.Revolute)
            {
                RevoluteJoint revolute = (RevoluteJoint)def.JointB;
                _localAnchorD = revolute._localAnchorA;
                _localAnchorB = revolute._localAnchorB;
                _referenceAngleB = revolute._referenceAngle;
                _localAxisD = Vector2.Zero;

                coordinateB = aB - aD - _referenceAngleB;
            }
            else
            {
                PrismaticJoint prismatic = (PrismaticJoint)def.JointB;
                _localAnchorD = prismatic._localAnchorA;
                _localAnchorB = prismatic._localAnchorB;
                _referenceAngleB = prismatic._referenceAngle;
                _localAxisD = prismatic._localXAxisA;

                Vector2 pD = _localAnchorD;
                Vector2 pB = MathUtils.MulT(xfD.q, MathUtils.Mul(xfB.q, _localAnchorB) + (xfB.p - xfD.p));
                coordinateB = MathUtils.Dot(pB - pD, _localAxisD);
            }

            _ratio = def.Ratio;

            _constant = coordinateA + _ratio * coordinateB;

            _impulse = 0.0f;
        }

        /// <summary>
        /// Requires two existing revolute or prismatic joints (any combination will work). The provided joints must
        /// attach a dynamic body to a static body.
        /// </summary>
        /// <param name="jointA">The first joint.</param>
        /// <param name="jointB">The second joint.</param>
        /// <param name="ratio">The ratio.</param>
        /// <param name="bodyA">The first body</param>
        /// <param name="bodyB">The second body</param>
        public GearJoint(Body bodyA, Body bodyB, Joint jointA, Joint jointB, float ratio = 1f) : base(bodyA, bodyB, JointType.Gear)
        {
            _jointA = jointA;
            _jointB = jointB;

            _typeA = jointA.JointType;
            _typeB = jointB.JointType;

            Debug.Assert(_typeA == JointType.Revolute || _typeA == JointType.Prismatic || _typeA == JointType.FixedRevolute || _typeA == JointType.FixedPrismatic);
            Debug.Assert(_typeB == JointType.Revolute || _typeB == JointType.Prismatic || _typeB == JointType.FixedRevolute || _typeB == JointType.FixedPrismatic);

            float coordinateA, coordinateB;

            // TODO_ERIN there might be some problem with the joint edges in b2Joint.

            _bodyC = JointA.BodyA;
            _bodyA = JointA.BodyB;

            // Body B on joint1 must be dynamic
            Debug.Assert(_bodyA._type == BodyType.Dynamic);

            // Get geometry of joint1
            Transform xfA = _bodyA._xf;
            float aA = _bodyA._sweep.A;
            Transform xfC = _bodyC._xf;
            float aC = _bodyC._sweep.A;

            if (_typeA == JointType.Revolute)
            {
                RevoluteJoint revolute = (RevoluteJoint)jointA;
                _localAnchorC = revolute._localAnchorA;
                _localAnchorA = revolute._localAnchorB;
                _referenceAngleA = revolute._referenceAngle;
                _localAxisC = Vector2.Zero;

                coordinateA = aA - aC - _referenceAngleA;
            }
            else
            {
                PrismaticJoint prismatic = (PrismaticJoint)jointA;
                _localAnchorC = prismatic._localAnchorA;
                _localAnchorA = prismatic._localAnchorB;
                _referenceAngleA = prismatic._referenceAngle;
                _localAxisC = prismatic._localXAxisA;

                Vector2 pC = _localAnchorC;
                Vector2 pA = MathUtils.MulT(xfC.q, MathUtils.Mul(xfA.q, _localAnchorA) + (xfA.p - xfC.p));
                coordinateA = Vector2.Dot(pA - pC, _localAxisC);
            }

            _bodyD = JointB.BodyA;
            _bodyB = JointB.BodyB;

            // Body B on joint2 must be dynamic
            Debug.Assert(_bodyB._type == BodyType.Dynamic);

            // Get geometry of joint2
            Transform xfB = _bodyB._xf;
            float aB = _bodyB._sweep.A;
            Transform xfD = _bodyD._xf;
            float aD = _bodyD._sweep.A;

            if (_typeB == JointType.Revolute)
            {
                RevoluteJoint revolute = (RevoluteJoint)jointB;
                _localAnchorD = revolute._localAnchorA;
                _localAnchorB = revolute._localAnchorB;
                _referenceAngleB = revolute._referenceAngle;
                _localAxisD = Vector2.Zero;

                coordinateB = aB - aD - _referenceAngleB;
            }
            else
            {
                PrismaticJoint prismatic = (PrismaticJoint)jointB;
                _localAnchorD = prismatic._localAnchorA;
                _localAnchorB = prismatic._localAnchorB;
                _referenceAngleB = prismatic._referenceAngle;
                _localAxisD = prismatic._localXAxisA;

                Vector2 pD = _localAnchorD;
                Vector2 pB = MathUtils.MulT(xfD.q, MathUtils.Mul(xfB.q, _localAnchorB) + (xfB.p - xfD.p));
                coordinateB = Vector2.Dot(pB - pD, _localAxisD);
            }

            _ratio = ratio;
            _constant = coordinateA + _ratio * coordinateB;
            _impulse = 0.0f;
        }

        public override Vector2 WorldAnchorA
        {
            get => _bodyA.GetWorldPoint(_localAnchorA);
            set => Debug.Assert(false, "You can't set the world anchor on this joint type.");
        }

        public override Vector2 WorldAnchorB
        {
            get => _bodyB.GetWorldPoint(_localAnchorB);
            set => Debug.Assert(false, "You can't set the world anchor on this joint type.");
        }

        /// <summary>The gear ratio.</summary>
        public float Ratio
        {
            get => _ratio;
            set => _ratio = value;
        }

        /// <summary>The first revolute/prismatic joint attached to the gear joint.</summary>
        public Joint JointA => _jointA;

        /// <summary>The second revolute/prismatic joint attached to the gear joint.</summary>
        public Joint JointB => _jointB;

        public override Vector2 GetReactionForce(float invDt)
        {
            Vector2 P = _impulse * _JvAC;
            return invDt * P;
        }

        public override float GetReactionTorque(float invDt)
        {
            float L = _impulse * _JwA;
            return invDt * L;
        }

        internal override void InitVelocityConstraints(ref SolverData data)
        {
            _indexA = _bodyA.IslandIndex;
            _indexB = _bodyB.IslandIndex;
            _indexC = _bodyC.IslandIndex;
            _indexD = _bodyD.IslandIndex;
            _lcA = _bodyA._sweep.LocalCenter;
            _lcB = _bodyB._sweep.LocalCenter;
            _lcC = _bodyC._sweep.LocalCenter;
            _lcD = _bodyD._sweep.LocalCenter;
            _mA = _bodyA._invMass;
            _mB = _bodyB._invMass;
            _mC = _bodyC._invMass;
            _mD = _bodyD._invMass;
            _iA = _bodyA._invI;
            _iB = _bodyB._invI;
            _iC = _bodyC._invI;
            _iD = _bodyD._invI;

            float aA = data.Positions[_indexA].A;
            Vector2 vA = data.Velocities[_indexA].V;
            float wA = data.Velocities[_indexA].W;

            float aB = data.Positions[_indexB].A;
            Vector2 vB = data.Velocities[_indexB].V;
            float wB = data.Velocities[_indexB].W;

            float aC = data.Positions[_indexC].A;
            Vector2 vC = data.Velocities[_indexC].V;
            float wC = data.Velocities[_indexC].W;

            float aD = data.Positions[_indexD].A;
            Vector2 vD = data.Velocities[_indexD].V;
            float wD = data.Velocities[_indexD].W;

            Rot qA = new Rot(aA), qB = new Rot(aB), qC = new Rot(aC), qD = new Rot(aD);

            _mass = 0.0f;

            if (_typeA == JointType.Revolute)
            {
                _JvAC = Vector2.Zero;
                _JwA = 1.0f;
                _JwC = 1.0f;
                _mass += _iA + _iC;
            }
            else
            {
                Vector2 u = MathUtils.Mul(qC, _localAxisC);
                Vector2 rC = MathUtils.Mul(qC, _localAnchorC - _lcC);
                Vector2 rA = MathUtils.Mul(qA, _localAnchorA - _lcA);
                _JvAC = u;
                _JwC = MathUtils.Cross(rC, u);
                _JwA = MathUtils.Cross(rA, u);
                _mass += _mC + _mA + _iC * _JwC * _JwC + _iA * _JwA * _JwA;
            }

            if (_typeB == JointType.Revolute)
            {
                _JvBD = Vector2.Zero;
                _JwB = _ratio;
                _JwD = _ratio;
                _mass += _ratio * _ratio * (_iB + _iD);
            }
            else
            {
                Vector2 u = MathUtils.Mul(qD, _localAxisD);
                Vector2 rD = MathUtils.Mul(qD, _localAnchorD - _lcD);
                Vector2 rB = MathUtils.Mul(qB, _localAnchorB - _lcB);
                _JvBD = _ratio * u;
                _JwD = _ratio * MathUtils.Cross(rD, u);
                _JwB = _ratio * MathUtils.Cross(rB, u);
                _mass += _ratio * _ratio * (_mD + _mB) + _iD * _JwD * _JwD + _iB * _JwB * _JwB;
            }

            // Compute effective mass.
            _mass = _mass > 0.0f ? 1.0f / _mass : 0.0f;

            if (data.Step.WarmStarting)
            {
                vA += (_mA * _impulse) * _JvAC;
                wA += _iA * _impulse * _JwA;
                vB += (_mB * _impulse) * _JvBD;
                wB += _iB * _impulse * _JwB;
                vC -= (_mC * _impulse) * _JvAC;
                wC -= _iC * _impulse * _JwC;
                vD -= (_mD * _impulse) * _JvBD;
                wD -= _iD * _impulse * _JwD;
            }
            else
                _impulse = 0.0f;

            data.Velocities[_indexA].V = vA;
            data.Velocities[_indexA].W = wA;
            data.Velocities[_indexB].V = vB;
            data.Velocities[_indexB].W = wB;
            data.Velocities[_indexC].V = vC;
            data.Velocities[_indexC].W = wC;
            data.Velocities[_indexD].V = vD;
            data.Velocities[_indexD].W = wD;
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            Vector2 vA = data.Velocities[_indexA].V;
            float wA = data.Velocities[_indexA].W;
            Vector2 vB = data.Velocities[_indexB].V;
            float wB = data.Velocities[_indexB].W;
            Vector2 vC = data.Velocities[_indexC].V;
            float wC = data.Velocities[_indexC].W;
            Vector2 vD = data.Velocities[_indexD].V;
            float wD = data.Velocities[_indexD].W;

            float Cdot = Vector2.Dot(_JvAC, vA - vC) + Vector2.Dot(_JvBD, vB - vD);
            Cdot += (_JwA * wA - _JwC * wC) + (_JwB * wB - _JwD * wD);

            float impulse = -_mass * Cdot;
            _impulse += impulse;

            vA += (_mA * impulse) * _JvAC;
            wA += _iA * impulse * _JwA;
            vB += (_mB * impulse) * _JvBD;
            wB += _iB * impulse * _JwB;
            vC -= (_mC * impulse) * _JvAC;
            wC -= _iC * impulse * _JwC;
            vD -= (_mD * impulse) * _JvBD;
            wD -= _iD * impulse * _JwD;

            data.Velocities[_indexA].V = vA;
            data.Velocities[_indexA].W = wA;
            data.Velocities[_indexB].V = vB;
            data.Velocities[_indexB].W = wB;
            data.Velocities[_indexC].V = vC;
            data.Velocities[_indexC].W = wC;
            data.Velocities[_indexD].V = vD;
            data.Velocities[_indexD].W = wD;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            Vector2 cA = data.Positions[_indexA].C;
            float aA = data.Positions[_indexA].A;
            Vector2 cB = data.Positions[_indexB].C;
            float aB = data.Positions[_indexB].A;
            Vector2 cC = data.Positions[_indexC].C;
            float aC = data.Positions[_indexC].A;
            Vector2 cD = data.Positions[_indexD].C;
            float aD = data.Positions[_indexD].A;

            Rot qA = new Rot(aA), qB = new Rot(aB), qC = new Rot(aC), qD = new Rot(aD);

            const float linearError = 0.0f;

            float coordinateA, coordinateB;

            Vector2 JvAC, JvBD;
            float JwA, JwB, JwC, JwD;
            float mass = 0.0f;

            if (_typeA == JointType.Revolute)
            {
                JvAC = Vector2.Zero;
                JwA = 1.0f;
                JwC = 1.0f;
                mass += _iA + _iC;

                coordinateA = aA - aC - _referenceAngleA;
            }
            else
            {
                Vector2 u = MathUtils.Mul(qC, _localAxisC);
                Vector2 rC = MathUtils.Mul(qC, _localAnchorC - _lcC);
                Vector2 rA = MathUtils.Mul(qA, _localAnchorA - _lcA);
                JvAC = u;
                JwC = MathUtils.Cross(rC, u);
                JwA = MathUtils.Cross(rA, u);
                mass += _mC + _mA + _iC * JwC * JwC + _iA * JwA * JwA;

                Vector2 pC = _localAnchorC - _lcC;
                Vector2 pA = MathUtils.MulT(qC, rA + (cA - cC));
                coordinateA = Vector2.Dot(pA - pC, _localAxisC);
            }

            if (_typeB == JointType.Revolute)
            {
                JvBD = Vector2.Zero;
                JwB = _ratio;
                JwD = _ratio;
                mass += _ratio * _ratio * (_iB + _iD);

                coordinateB = aB - aD - _referenceAngleB;
            }
            else
            {
                Vector2 u = MathUtils.Mul(qD, _localAxisD);
                Vector2 rD = MathUtils.Mul(qD, _localAnchorD - _lcD);
                Vector2 rB = MathUtils.Mul(qB, _localAnchorB - _lcB);
                JvBD = _ratio * u;
                JwD = _ratio * MathUtils.Cross(rD, u);
                JwB = _ratio * MathUtils.Cross(rB, u);
                mass += _ratio * _ratio * (_mD + _mB) + _iD * JwD * JwD + _iB * JwB * JwB;

                Vector2 pD = _localAnchorD - _lcD;
                Vector2 pB = MathUtils.MulT(qD, rB + (cB - cD));
                coordinateB = Vector2.Dot(pB - pD, _localAxisD);
            }

            float C = (coordinateA + _ratio * coordinateB) - _constant;

            float impulse = 0.0f;
            if (mass > 0.0f)
                impulse = -C / mass;

            cA += _mA * impulse * JvAC;
            aA += _iA * impulse * JwA;
            cB += _mB * impulse * JvBD;
            aB += _iB * impulse * JwB;
            cC -= _mC * impulse * JvAC;
            aC -= _iC * impulse * JwC;
            cD -= _mD * impulse * JvBD;
            aD -= _iD * impulse * JwD;

            data.Positions[_indexA].C = cA;
            data.Positions[_indexA].A = aA;
            data.Positions[_indexB].C = cB;
            data.Positions[_indexB].A = aB;
            data.Positions[_indexC].C = cC;
            data.Positions[_indexC].A = aC;
            data.Positions[_indexD].C = cD;
            data.Positions[_indexD].A = aD;

            // TODO_ERIN not implemented
            return linearError < Settings.LinearSlop;
        }
    }
}
