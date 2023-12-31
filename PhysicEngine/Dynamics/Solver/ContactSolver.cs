﻿using Microsoft.Xna.Framework;
using PhysicEngine.Collision.ContactSystem;
using PhysicEngine.Collision.Narrowphase;
using PhysicEngine.Collision.Shapes;
using PhysicEngine.Shared;
using PhysicEngine.Shared.Optimization;
using PhysicEngine.Utilities;
using System;
using System.Diagnostics;

namespace PhysicEngine.Dynamics.Solver
{
    internal class ContactSolver
    {
        private Contact[] _contacts;
        private int _count;
        private ContactPositionConstraint[] _positionConstraints;
        private Position[] _positions;
        private TimeStep _step;
        private Velocity[] _velocities;
        public ContactVelocityConstraint[] VelocityConstraints;

        public void Reset(TimeStep step, int count, Contact[] contacts, Position[] positions, Velocity[] velocities)
        {
            _step = step;
            _count = count;
            _positions = positions;
            _velocities = velocities;
            _contacts = contacts;

            // grow the array
            if (VelocityConstraints == null || VelocityConstraints.Length < count)
            {
                VelocityConstraints = new ContactVelocityConstraint[count * 2];
                _positionConstraints = new ContactPositionConstraint[count * 2];

                for (int i = 0; i < VelocityConstraints.Length; i++)
                {
                    VelocityConstraints[i] = new ContactVelocityConstraint();
                }

                for (int i = 0; i < _positionConstraints.Length; i++)
                {
                    _positionConstraints[i] = new ContactPositionConstraint();
                }
            }

            // Initialize position independent portions of the constraints.
            for (int i = 0; i < _count; ++i)
            {
                Contact contact = contacts[i];

                Fixture fixtureA = contact._fixtureA;
                Fixture fixtureB = contact._fixtureB;
                Shape shapeA = fixtureA.Shape;
                Shape shapeB = fixtureB.Shape;
                float radiusA = shapeA._radius;
                float radiusB = shapeB._radius;
                Body bodyA = fixtureA.Body;
                Body bodyB = fixtureB.Body;
                Manifold manifold = contact.Manifold;

                int pointCount = manifold.PointCount;
                Debug.Assert(pointCount > 0);

                ContactVelocityConstraint vc = VelocityConstraints[i];
                vc.Friction = contact._friction;
                vc.Restitution = contact._restitution;
                vc.Threshold = contact._restitutionThreshold;
                vc.TangentSpeed = contact._tangentSpeed;
                vc.IndexA = bodyA._islandIndex;
                vc.IndexB = bodyB._islandIndex;
                vc.InvMassA = bodyA._invMass;
                vc.InvMassB = bodyB._invMass;
                vc.InvIA = bodyA._invI;
                vc.InvIB = bodyB._invI;
                vc.ContactIndex = i;
                vc.PointCount = pointCount;
                vc.K.SetZero();
                vc.NormalMass.SetZero();

                ContactPositionConstraint pc = _positionConstraints[i];
                pc.IndexA = bodyA.IslandIndex;
                pc.IndexB = bodyB.IslandIndex;
                pc.InvMassA = bodyA._invMass;
                pc.InvMassB = bodyB._invMass;
                pc.LocalCenterA = bodyA._sweep.LocalCenter;
                pc.LocalCenterB = bodyB._sweep.LocalCenter;
                pc.InvIA = bodyA._invI;
                pc.InvIB = bodyB._invI;
                pc.LocalNormal = manifold.LocalNormal;
                pc.LocalPoint = manifold.LocalPoint;
                pc.PointCount = pointCount;
                pc.RadiusA = radiusA;
                pc.RadiusB = radiusB;
                pc.Type = manifold.Type;

                for (int j = 0; j < pointCount; ++j)
                {
                    ManifoldPoint cp = manifold.Points[j];
                    VelocityConstraintPoint vcp = vc.Points[j];

                    if (step.WarmStarting)
                    {
                        vcp.NormalImpulse = _step.DeltaTimeRatio * cp.NormalImpulse;
                        vcp.TangentImpulse = _step.DeltaTimeRatio * cp.TangentImpulse;
                    }
                    else
                    {
                        vcp.NormalImpulse = 0.0f;
                        vcp.TangentImpulse = 0.0f;
                    }

                    vcp.rA = Vector2.Zero;
                    vcp.rB = Vector2.Zero;
                    vcp.NormalMass = 0.0f;
                    vcp.TangentMass = 0.0f;
                    vcp.VelocityBias = 0.0f;

                    pc.LocalPoints[j] = cp.LocalPoint;
                }
            }
        }

        /// <summary>Initialize position dependent portions of the velocity constraints.</summary>
        public void InitializeVelocityConstraints()
        {
            for (int i = 0; i < _count; ++i)
            {
                ContactVelocityConstraint vc = VelocityConstraints[i];
                ContactPositionConstraint pc = _positionConstraints[i];

                float radiusA = pc.RadiusA;
                float radiusB = pc.RadiusB;
                Manifold manifold = _contacts[vc.ContactIndex].Manifold;

                int indexA = vc.IndexA;
                int indexB = vc.IndexB;

                float mA = vc.InvMassA;
                float mB = vc.InvMassB;
                float iA = vc.InvIA;
                float iB = vc.InvIB;
                Vector2 localCenterA = pc.LocalCenterA;
                Vector2 localCenterB = pc.LocalCenterB;

                Vector2 cA = _positions[indexA].C;
                float aA = _positions[indexA].A;
                Vector2 vA = _velocities[indexA].V;
                float wA = _velocities[indexA].W;

                Vector2 cB = _positions[indexB].C;
                float aB = _positions[indexB].A;
                Vector2 vB = _velocities[indexB].V;
                float wB = _velocities[indexB].W;

                Debug.Assert(manifold.PointCount > 0);

                Transform xfA = new Transform();
                Transform xfB = new Transform();
                xfA.q.Set(aA);
                xfB.q.Set(aB);
                xfA.p = cA - MathUtils.Mul(xfA.q, localCenterA);
                xfB.p = cB - MathUtils.Mul(xfB.q, localCenterB);

                WorldManifold.Initialize(ref manifold, ref xfA, radiusA, ref xfB, radiusB, out Vector2 normal, out FixedArray2<Vector2> points, out _);

                vc.Normal = normal;

                int pointCount = vc.PointCount;
                for (int j = 0; j < pointCount; ++j)
                {
                    VelocityConstraintPoint vcp = vc.Points[j];

                    vcp.rA = points[j] - cA;
                    vcp.rB = points[j] - cB;

                    float rnA = MathUtils.Cross(vcp.rA, vc.Normal);
                    float rnB = MathUtils.Cross(vcp.rB, vc.Normal);

                    float kNormal = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

                    vcp.NormalMass = kNormal > 0.0f ? 1.0f / kNormal : 0.0f;

                    Vector2 tangent = MathUtils.Cross(vc.Normal, 1.0f);

                    float rtA = MathUtils.Cross(vcp.rA, tangent);
                    float rtB = MathUtils.Cross(vcp.rB, tangent);

                    float kTangent = mA + mB + iA * rtA * rtA + iB * rtB * rtB;

                    vcp.TangentMass = kTangent > 0.0f ? 1.0f / kTangent : 0.0f;

                    // Setup a velocity bias for restitution.
                    vcp.VelocityBias = 0.0f;
                    float vRel = MathUtils.Dot(vc.Normal, vB + MathUtils.Cross(wB, vcp.rB) - vA - MathUtils.Cross(wA, vcp.rA));
                    if (vRel < -vc.Threshold)
                        vcp.VelocityBias = -vc.Restitution * vRel;
                }

                // If we have two points, then prepare the block solver.
                if (vc.PointCount == 2 && Settings.BlockSolve)
                {
                    VelocityConstraintPoint vcp1 = vc.Points[0];
                    VelocityConstraintPoint vcp2 = vc.Points[1];

                    float rn1A = MathUtils.Cross(vcp1.rA, vc.Normal);
                    float rn1B = MathUtils.Cross(vcp1.rB, vc.Normal);
                    float rn2A = MathUtils.Cross(vcp2.rA, vc.Normal);
                    float rn2B = MathUtils.Cross(vcp2.rB, vc.Normal);

                    float k11 = mA + mB + iA * rn1A * rn1A + iB * rn1B * rn1B;
                    float k22 = mA + mB + iA * rn2A * rn2A + iB * rn2B * rn2B;
                    float k12 = mA + mB + iA * rn1A * rn2A + iB * rn1B * rn2B;

                    // Ensure a reasonable condition number.
                    const float k_maxConditionNumber = 1000.0f;
                    if (k11 * k11 < k_maxConditionNumber * (k11 * k22 - k12 * k12))
                    {
                        // K is safe to invert.
                        vc.K.ex = new Vector2(k11, k12);
                        vc.K.ey = new Vector2(k12, k22);
                        vc.NormalMass = vc.K.Inverse;
                    }
                    else
                    {
                        // The constraints are redundant, just use one.
                        // TODO_ERIN use deepest?
                        vc.PointCount = 1;
                    }
                }
            }
        }

        public void WarmStart()
        {
            // Warm start.
            for (int i = 0; i < _count; ++i)
            {
                ContactVelocityConstraint vc = VelocityConstraints[i];

                int indexA = vc.IndexA;
                int indexB = vc.IndexB;
                float mA = vc.InvMassA;
                float iA = vc.InvIA;
                float mB = vc.InvMassB;
                float iB = vc.InvIB;
                int pointCount = vc.PointCount;

                Vector2 vA = _velocities[indexA].V;
                float wA = _velocities[indexA].W;
                Vector2 vB = _velocities[indexB].V;
                float wB = _velocities[indexB].W;

                Vector2 normal = vc.Normal;
                Vector2 tangent = MathUtils.Cross(normal, 1.0f);

                for (int j = 0; j < pointCount; ++j)
                {
                    VelocityConstraintPoint vcp = vc.Points[j];
                    Vector2 P = vcp.NormalImpulse * normal + vcp.TangentImpulse * tangent;
                    wA -= iA * MathUtils.Cross(vcp.rA, P);
                    vA -= mA * P;
                    wB += iB * MathUtils.Cross(vcp.rB, P);
                    vB += mB * P;
                }

                _velocities[indexA].V = vA;
                _velocities[indexA].W = wA;
                _velocities[indexB].V = vB;
                _velocities[indexB].W = wB;
            }
        }

        public void SolveVelocityConstraints()
        {
            for (int i = 0; i < _count; ++i)
            {
                ContactVelocityConstraint vc = VelocityConstraints[i];

                int indexA = vc.IndexA;
                int indexB = vc.IndexB;
                float mA = vc.InvMassA;
                float iA = vc.InvIA;
                float mB = vc.InvMassB;
                float iB = vc.InvIB;
                int pointCount = vc.PointCount;

                Vector2 vA = _velocities[indexA].V;
                float wA = _velocities[indexA].W;
                Vector2 vB = _velocities[indexB].V;
                float wB = _velocities[indexB].W;

                Vector2 normal = vc.Normal;
                Vector2 tangent = MathUtils.Cross(normal, 1.0f);
                float friction = vc.Friction;

                Debug.Assert(pointCount == 1 || pointCount == 2);

                // Solve tangent constraints first because non-penetration is more important
                // than friction.
                for (int j = 0; j < pointCount; ++j)
                {
                    VelocityConstraintPoint vcp = vc.Points[j];

                    // Relative velocity at contact
                    Vector2 dv = vB + MathUtils.Cross(wB, vcp.rB) - vA - MathUtils.Cross(wA, vcp.rA);

                    // Compute tangent force
                    float vt = Vector2.Dot(dv, tangent) - vc.TangentSpeed;
                    float lambda = vcp.TangentMass * -vt;

                    // b2Clamp the accumulated force
                    float maxFriction = friction * vcp.NormalImpulse;
                    float newImpulse = MathUtils.Clamp(vcp.TangentImpulse + lambda, -maxFriction, maxFriction);
                    lambda = newImpulse - vcp.TangentImpulse;
                    vcp.TangentImpulse = newImpulse;

                    // Apply contact impulse
                    Vector2 P = lambda * tangent;

                    vA -= mA * P;
                    wA -= iA * MathUtils.Cross(vcp.rA, P);

                    vB += mB * P;
                    wB += iB * MathUtils.Cross(vcp.rB, P);
                }

                // Solve normal constraints
                if (pointCount == 1 || !Settings.BlockSolve)
                {
                    for (int j = 0; j < pointCount; ++j)
                    {
                        VelocityConstraintPoint vcp = vc.Points[j];

                        // Relative velocity at contact
                        Vector2 dv = vB + MathUtils.Cross(wB, vcp.rB) - vA - MathUtils.Cross(wA, vcp.rA);

                        // Compute normal impulse
                        float vn = Vector2.Dot(dv, normal);
                        float lambda = -vcp.NormalMass * (vn - vcp.VelocityBias);

                        // b2Clamp the accumulated impulse
                        float newImpulse = Math.Max(vcp.NormalImpulse + lambda, 0.0f);
                        lambda = newImpulse - vcp.NormalImpulse;
                        vcp.NormalImpulse = newImpulse;

                        // Apply contact impulse
                        Vector2 P = lambda * normal;
                        vA -= mA * P;
                        wA -= iA * MathUtils.Cross(vcp.rA, P);

                        vB += mB * P;
                        wB += iB * MathUtils.Cross(vcp.rB, P);
                    }
                }
                else
                {
                    VelocityConstraintPoint cp1 = vc.Points[0];
                    VelocityConstraintPoint cp2 = vc.Points[1];

                    Vector2 a = new Vector2(cp1.NormalImpulse, cp2.NormalImpulse);
                    Debug.Assert(a.X >= 0.0f && a.Y >= 0.0f);

                    // Relative velocity at contact
                    Vector2 dv1 = vB + MathUtils.Cross(wB, cp1.rB) - vA - MathUtils.Cross(wA, cp1.rA);
                    Vector2 dv2 = vB + MathUtils.Cross(wB, cp2.rB) - vA - MathUtils.Cross(wA, cp2.rA);

                    // Compute normal velocity
                    float vn1 = Vector2.Dot(dv1, normal);
                    float vn2 = Vector2.Dot(dv2, normal);

                    Vector2 b = Vector2.Zero;
                    b.X = vn1 - cp1.VelocityBias;
                    b.Y = vn2 - cp2.VelocityBias;

                    // Compute b'
                    b -= MathUtils.Mul(ref vc.K, a);

                    for (; ; )
                    {
                        //
                        // Case 1: vn = 0
                        //
                        // 0 = A * x + b'
                        //
                        // Solve for x:
                        //
                        // x = - inv(A) * b'
                        //
                        Vector2 x = -MathUtils.Mul(ref vc.NormalMass, b);

                        if (x.X >= 0.0f && x.Y >= 0.0f)
                        {
                            // Get the incremental impulse
                            Vector2 d = x - a;

                            // Apply incremental impulse
                            Vector2 P1 = d.X * normal;
                            Vector2 P2 = d.Y * normal;
                            vA -= mA * (P1 + P2);
                            wA -= iA * (MathUtils.Cross(cp1.rA, P1) + MathUtils.Cross(cp2.rA, P2));

                            vB += mB * (P1 + P2);
                            wB += iB * (MathUtils.Cross(cp1.rB, P1) + MathUtils.Cross(cp2.rB, P2));

                            // Accumulate
                            cp1.NormalImpulse = x.X;
                            cp2.NormalImpulse = x.Y;

#if B2_DEBUG_SOLVER
                            // Postconditions
                            dv1 = vB + MathUtils.Cross(wB, cp1.rB) - vA - MathUtils.Cross(wA, cp1.rA);
                            dv2 = vB + MathUtils.Cross(wB, cp2.rB) - vA - MathUtils.Cross(wA, cp2.rA);

                            // Compute normal velocity
                            vn1 = Vector2.Dot(dv1, normal);
                            vn2 = Vector2.Dot(dv2, normal);

                            Debug.Assert(Math.Abs(vn1 - cp1.VelocityBias) < k_errorTol);
                            Debug.Assert(Math.Abs(vn2 - cp2.VelocityBias) < k_errorTol);
#endif
                            break;
                        }

                        //
                        // Case 2: vn1 = 0 and x2 = 0
                        //
                        //   0 = a11 * x1 + a12 * 0 + b1' 
                        // vn2 = a21 * x1 + a22 * 0 + b2'
                        //
                        x.X = -cp1.NormalMass * b.X;
                        x.Y = 0.0f;
                        vn1 = 0.0f;
                        vn2 = vc.K.ex.Y * x.X + b.Y;

                        if (x.X >= 0.0f && vn2 >= 0.0f)
                        {
                            // Get the incremental impulse
                            Vector2 d = x - a;

                            // Apply incremental impulse
                            Vector2 P1 = d.X * normal;
                            Vector2 P2 = d.Y * normal;
                            vA -= mA * (P1 + P2);
                            wA -= iA * (MathUtils.Cross(cp1.rA, P1) + MathUtils.Cross(cp2.rA, P2));

                            vB += mB * (P1 + P2);
                            wB += iB * (MathUtils.Cross(cp1.rB, P1) + MathUtils.Cross(cp2.rB, P2));

                            // Accumulate
                            cp1.NormalImpulse = x.X;
                            cp2.NormalImpulse = x.Y;

#if B2_DEBUG_SOLVER
                            // Postconditions
                            dv1 = vB + MathUtils.Cross(wB, cp1.rB) - vA - MathUtils.Cross(wA, cp1.rA);

                            // Compute normal velocity
                            vn1 = Vector2.Dot(dv1, normal);

                            Debug.Assert(Math.Abs(vn1 - cp1.VelocityBias) < k_errorTol);
#endif
                            break;
                        }

                        //
                        // Case 3: vn2 = 0 and x1 = 0
                        //
                        // vn1 = a11 * 0 + a12 * x2 + b1' 
                        //   0 = a21 * 0 + a22 * x2 + b2'
                        //
                        x.X = 0.0f;
                        x.Y = -cp2.NormalMass * b.Y;
                        vn1 = vc.K.ey.X * x.Y + b.X;
                        vn2 = 0.0f;

                        if (x.Y >= 0.0f && vn1 >= 0.0f)
                        {
                            // Resubstitute for the incremental impulse
                            Vector2 d = x - a;

                            // Apply incremental impulse
                            Vector2 P1 = d.X * normal;
                            Vector2 P2 = d.Y * normal;
                            vA -= mA * (P1 + P2);
                            wA -= iA * (MathUtils.Cross(cp1.rA, P1) + MathUtils.Cross(cp2.rA, P2));

                            vB += mB * (P1 + P2);
                            wB += iB * (MathUtils.Cross(cp1.rB, P1) + MathUtils.Cross(cp2.rB, P2));

                            // Accumulate
                            cp1.NormalImpulse = x.X;
                            cp2.NormalImpulse = x.Y;

#if B2_DEBUG_SOLVER
                            // Postconditions
                            dv2 = vB + MathUtils.Cross(wB, cp2.rB) - vA - MathUtils.Cross(wA, cp2.rA);

                            // Compute normal velocity
                            vn2 = Vector2.Dot(dv2, normal);

                            Debug.Assert(Math.Abs(vn2 - cp2.VelocityBias) < k_errorTol);
#endif
                            break;
                        }

                        //
                        // Case 4: x1 = 0 and x2 = 0
                        // 
                        // vn1 = b1
                        // vn2 = b2;
                        x.X = 0.0f;
                        x.Y = 0.0f;
                        vn1 = b.X;
                        vn2 = b.Y;

                        if (vn1 >= 0.0f && vn2 >= 0.0f)
                        {
                            // Resubstitute for the incremental impulse
                            Vector2 d = x - a;

                            // Apply incremental impulse
                            Vector2 P1 = d.X * normal;
                            Vector2 P2 = d.Y * normal;
                            vA -= mA * (P1 + P2);
                            wA -= iA * (MathUtils.Cross(cp1.rA, P1) + MathUtils.Cross(cp2.rA, P2));

                            vB += mB * (P1 + P2);
                            wB += iB * (MathUtils.Cross(cp1.rB, P1) + MathUtils.Cross(cp2.rB, P2));

                            // Accumulate
                            cp1.NormalImpulse = x.X;
                            cp2.NormalImpulse = x.Y;

                        }

                        // No solution, give up. This is hit sometimes, but it doesn't seem to matter.
                        break;
                    }
                }

                _velocities[indexA].V = vA;
                _velocities[indexA].W = wA;
                _velocities[indexB].V = vB;
                _velocities[indexB].W = wB;
            }
        }

        public void StoreImpulses()
        {
            for (int i = 0; i < _count; ++i)
            {
                ContactVelocityConstraint vc = VelocityConstraints[i];
                Manifold manifold = _contacts[vc.ContactIndex].Manifold;

                for (int j = 0; j < vc.PointCount; ++j)
                {
                    ManifoldPoint point = manifold.Points[j];
                    point.NormalImpulse = vc.Points[j].NormalImpulse;
                    point.TangentImpulse = vc.Points[j].TangentImpulse;
                    manifold.Points[j] = point;
                }

                _contacts[vc.ContactIndex]._manifold = manifold;
            }
        }

        public bool SolvePositionConstraints()
        {
            float minSeparation = 0.0f;

            for (int i = 0; i < _count; ++i)
            {
                ContactPositionConstraint pc = _positionConstraints[i];

                int indexA = pc.IndexA;
                int indexB = pc.IndexB;
                Vector2 localCenterA = pc.LocalCenterA;
                float mA = pc.InvMassA;
                float iA = pc.InvIA;
                Vector2 localCenterB = pc.LocalCenterB;
                float mB = pc.InvMassB;
                float iB = pc.InvIB;
                int pointCount = pc.PointCount;

                Vector2 cA = _positions[indexA].C;
                float aA = _positions[indexA].A;

                Vector2 cB = _positions[indexB].C;
                float aB = _positions[indexB].A;

                // Solve normal constraints
                for (int j = 0; j < pointCount; ++j)
                {
                    Transform xfA = new Transform();
                    Transform xfB = new Transform();
                    xfA.q.Set(aA);
                    xfB.q.Set(aB);
                    xfA.p = cA - MathUtils.Mul(xfA.q, localCenterA);
                    xfB.p = cB - MathUtils.Mul(xfB.q, localCenterB);

                    PositionSolverManifold.Initialize(pc, ref xfA, ref xfB, j, out Vector2 normal, out Vector2 point, out float separation);

                    Vector2 rA = point - cA;
                    Vector2 rB = point - cB;

                    // Track max constraint error.
                    minSeparation = Math.Min(minSeparation, separation);

                    // Prevent large corrections and allow slop.
                    float C = MathUtils.Clamp(Settings.Baumgarte * (separation + Settings.LinearSlop), -Settings.MaxLinearCorrection, 0.0f);

                    // Compute the effective mass.
                    float rnA = MathUtils.Cross(rA, normal);
                    float rnB = MathUtils.Cross(rB, normal);
                    float K = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

                    // Compute normal impulse
                    float impulse = K > 0.0f ? -C / K : 0.0f;

                    Vector2 P = impulse * normal;

                    cA -= mA * P;
                    aA -= iA * MathUtils.Cross(rA, P);

                    cB += mB * P;
                    aB += iB * MathUtils.Cross(rB, P);
                }

                _positions[indexA].C = cA;
                _positions[indexA].A = aA;

                _positions[indexB].C = cB;
                _positions[indexB].A = aB;
            }

            // We can't expect minSpeparation >= -b2_linearSlop because we don't
            // push the separation above -b2_linearSlop.
            return minSeparation >= -3.0f * Settings.LinearSlop;
        }

        // Sequential position solver for position constraints.
        public bool SolveTOIPositionConstraints(int toiIndexA, int toiIndexB)
        {
            float minSeparation = 0.0f;

            for (int i = 0; i < _count; ++i)
            {
                ContactPositionConstraint pc = _positionConstraints[i];

                int indexA = pc.IndexA;
                int indexB = pc.IndexB;
                Vector2 localCenterA = pc.LocalCenterA;
                Vector2 localCenterB = pc.LocalCenterB;
                int pointCount = pc.PointCount;

                float mA = 0.0f;
                float iA = 0.0f;
                if (indexA == toiIndexA || indexA == toiIndexB)
                {
                    mA = pc.InvMassA;
                    iA = pc.InvIA;
                }

                float mB = 0.0f;
                float iB = 0.0f;
                if (indexB == toiIndexA || indexB == toiIndexB)
                {
                    mB = pc.InvMassB;
                    iB = pc.InvIB;
                }

                Vector2 cA = _positions[indexA].C;
                float aA = _positions[indexA].A;

                Vector2 cB = _positions[indexB].C;
                float aB = _positions[indexB].A;

                // Solve normal constraints
                for (int j = 0; j < pointCount; ++j)
                {
                    Transform xfA = new Transform();
                    Transform xfB = new Transform();
                    xfA.q.Set(aA);
                    xfB.q.Set(aB);
                    xfA.p = cA - MathUtils.Mul(xfA.q, localCenterA);
                    xfB.p = cB - MathUtils.Mul(xfB.q, localCenterB);

                    PositionSolverManifold.Initialize(pc, ref xfA, ref xfB, j, out Vector2 normal, out Vector2 point, out float separation);

                    Vector2 rA = point - cA;
                    Vector2 rB = point - cB;

                    // Track max constraint error.
                    minSeparation = Math.Min(minSeparation, separation);

                    // Prevent large corrections and allow slop.
                    float C = MathUtils.Clamp(Settings.TOIBaumgarte * (separation + Settings.LinearSlop), -Settings.MaxLinearCorrection, 0.0f);

                    // Compute the effective mass.
                    float rnA = MathUtils.Cross(rA, normal);
                    float rnB = MathUtils.Cross(rB, normal);
                    float K = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

                    // Compute normal impulse
                    float impulse = K > 0.0f ? -C / K : 0.0f;

                    Vector2 P = impulse * normal;

                    cA -= mA * P;
                    aA -= iA * MathUtils.Cross(rA, P);

                    cB += mB * P;
                    aB += iB * MathUtils.Cross(rB, P);
                }

                _positions[indexA].C = cA;
                _positions[indexA].A = aA;

                _positions[indexB].C = cB;
                _positions[indexB].A = aB;
            }

            // We can't expect minSpeparation >= -b2_linearSlop because we don't
            // push the separation above -b2_linearSlop.
            return minSeparation >= -1.5f * Settings.LinearSlop;
        }
    }
}
