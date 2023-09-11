using Microsoft.Xna.Framework;
using System;
using PhysicEngine.Utilities;
using PhysicEngine.Collision.Raycast;

namespace PhysicEngine.Shared
{
    public struct AABB
    {
        public Vector2 LowerBound;
        public Vector2 UpperBound;
        public Vector2 Center => 0.5f * (LowerBound + UpperBound);
        public Vector2 Extents => 0.5f * (UpperBound - LowerBound);
        public float Width => UpperBound.X - LowerBound.X;
        public float Height => UpperBound.Y - LowerBound.Y;
        public float Perimeter
        {
            get
            {
                float wx = UpperBound.X - LowerBound.X;
                float wy = UpperBound.Y - LowerBound.Y;
                return 2.0f * (wx + wy);
            }
        }

        public Vertices Vertices
        {
            get
            {
                Vertices vertices = new Vertices(4);
                vertices.Add(UpperBound);
                vertices.Add(new Vector2(UpperBound.X, LowerBound.Y));
                vertices.Add(LowerBound);
                vertices.Add(new Vector2(LowerBound.X, UpperBound.Y));
                return vertices;
            }
        }

        public AABB Q1 => new AABB(Center, UpperBound);
        public AABB Q2 => new AABB(new Vector2(LowerBound.X, Center.Y), new Vector2(Center.X, UpperBound.Y));
        public AABB Q3 => new AABB(LowerBound, Center);
        public AABB Q4 => new AABB(new Vector2(Center.X, LowerBound.Y), new Vector2(UpperBound.X, Center.Y));

        public AABB(Vector2 min, Vector2 max) 
            : this(ref min, ref max) 
        { }
        public AABB(Vector2 center, float width, float height) 
            : this(center - new Vector2(width / 2, height / 2), 
                  center + new Vector2(width / 2, height / 2)) 
        { }
        public AABB(ref Vector2 min, ref Vector2 max)
        {
            LowerBound = new Vector2(Math.Min(min.X, max.X), 
                Math.Min(min.Y, max.Y));
            UpperBound = new Vector2(Math.Max(min.X, max.X), 
                Math.Max(min.Y, max.Y));
        }

        public bool IsValid()
        {
            Vector2 d = UpperBound - LowerBound;
            bool valid = d.X >= 0.0f && d.Y >= 0.0f;
            return valid && LowerBound.IsValid() && UpperBound.IsValid();
        }
        public void Combine(ref AABB aabb)
        {
            LowerBound = Vector2.Min(LowerBound, aabb.LowerBound);
            UpperBound = Vector2.Max(UpperBound, aabb.UpperBound);
        }
        public void Combine(ref AABB aabb1, ref AABB aabb2)
        {
            LowerBound = Vector2.Min(aabb1.LowerBound, aabb2.LowerBound);
            UpperBound = Vector2.Max(aabb1.UpperBound, aabb2.UpperBound);
        }
        public bool Contains(ref AABB aabb)
        {
            bool result = LowerBound.X <= aabb.LowerBound.X;
            result = result && LowerBound.Y <= aabb.LowerBound.Y;
            result = result && aabb.UpperBound.X <= UpperBound.X;
            result = result && aabb.UpperBound.Y <= UpperBound.Y;
            return result;
        }
        public bool Contains(ref Vector2 point)
        {
            //using epsilon to try and guard against float rounding errors.
            return (point.X > (LowerBound.X + float.Epsilon) && point.X < (UpperBound.X - float.Epsilon) &&
                    (point.Y > (LowerBound.Y + float.Epsilon) && point.Y < (UpperBound.Y - float.Epsilon)));
        }
        public static bool TestOverlap(ref AABB a, ref AABB b)
        {
            Vector2 d1 = b.LowerBound - a.UpperBound;
            Vector2 d2 = a.LowerBound - b.UpperBound;

            return d1.X <= 0 && d1.Y <= 0 && d2.X <= 0 && d2.Y <= 0;
        }
        public bool RayCast(ref RaycastInput input, out RaycastOutput output, bool doInteriorCheck = true)
        {
            // From Real-time Collision Detection, p179.

            output = new RaycastOutput();

            float tmin = -MathConstants.MaxFloat;
            float tmax = MathConstants.MaxFloat;

            Vector2 p = input.Point1;
            Vector2 d = input.Point2 - input.Point1;
            Vector2 absD = MathUtils.Abs(d);

            Vector2 normal = Vector2.Zero;

            for (int i = 0; i < 2; ++i)
            {
                float absD_i = i == 0 ? absD.X : absD.Y;
                float lowerBound_i = i == 0 ? LowerBound.X : LowerBound.Y;
                float upperBound_i = i == 0 ? UpperBound.X : UpperBound.Y;
                float p_i = i == 0 ? p.X : p.Y;

                if (absD_i < MathConstants.Epsilon)
                {
                    // Parallel.
                    if (p_i < lowerBound_i || upperBound_i < p_i)
                        return false;
                }
                else
                {
                    float d_i = i == 0 ? d.X : d.Y;

                    float inv_d = 1.0f / d_i;
                    float t1 = (lowerBound_i - p_i) * inv_d;
                    float t2 = (upperBound_i - p_i) * inv_d;

                    // Sign of the normal vector.
                    float s = -1.0f;

                    if (t1 > t2)
                    {
                        MathUtils.Swap(ref t1, ref t2);
                        s = 1.0f;
                    }

                    // Push the min up
                    if (t1 > tmin)
                    {
                        if (i == 0)
                            normal.X = s;
                        else
                            normal.Y = s;

                        tmin = t1;
                    }

                    // Pull the max down
                    tmax = Math.Min(tmax, t2);

                    if (tmin > tmax)
                        return false;
                }
            }

            // Does the ray start inside the box?
            // Does the ray intersect beyond the max fraction?
            if (doInteriorCheck && (tmin < 0.0f || input.MaxFraction < tmin))
                return false;

            // Intersection.
            output.Fraction = tmin;
            output.Normal = normal;
            return true;
        }
    }
}
