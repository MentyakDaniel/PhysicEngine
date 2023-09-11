using Microsoft.Xna.Framework;
using PhysicEngine.Shared;
using System;

namespace PhysicEngine.Utilities
{
    public static class LineUtils
    {
        public static float DistanceBetweenPointAndLineSegment(ref Vector2 point, ref Vector2 start, ref Vector2 end)
        {
            if (start == end)
                return Vector2.Distance(point, start);

            Vector2 v = Vector2.Subtract(end, start);
            Vector2 w = Vector2.Subtract(point, start);

            float c1 = Vector2.Dot(w, v);
            if (c1 <= 0)
                return Vector2.Distance(point, start);

            float c2 = Vector2.Dot(v, v);
            if (c2 <= c1)
                return Vector2.Distance(point, end);

            float b = c1 / c2;
            Vector2 pointOnLine = Vector2.Add(start, Vector2.Multiply(v, b));
            return Vector2.Distance(point, pointOnLine);
        }

        public static bool LineIntersect2(ref Vector2 a0, ref Vector2 a1, ref Vector2 b0, ref Vector2 b1, out Vector2 intersectionPoint)
        {
            intersectionPoint = Vector2.Zero;

            if (a0 == b0 || a0 == b1 || a1 == b0 || a1 == b1)
                return false;

            float x1 = a0.X;
            float y1 = a0.Y;
            float x2 = a1.X;
            float y2 = a1.Y;
            float x3 = b0.X;
            float y3 = b0.Y;
            float x4 = b1.X;
            float y4 = b1.Y;

            //AABB early exit
            if (Math.Max(x1, x2) < Math.Min(x3, x4) || Math.Max(x3, x4) < Math.Min(x1, x2))
                return false;

            if (Math.Max(y1, y2) < Math.Min(y3, y4) || Math.Max(y3, y4) < Math.Min(y1, y2))
                return false;

            float ua = (x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3);
            float ub = (x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3);
            float denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
            if (Math.Abs(denom) < MathConstants.Epsilon)
            {
                //Lines are too close to parallel to call
                return false;
            }
            ua /= denom;
            ub /= denom;

            if (0 < ua && ua < 1 && 0 < ub && ub < 1)
            {
                intersectionPoint.X = x1 + ua * (x2 - x1);
                intersectionPoint.Y = y1 + ua * (y2 - y1);
                return true;
            }

            return false;
        }

        public static Vector2 LineIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
        {
            Vector2 i = Vector2.Zero;
            float a1 = p2.Y - p1.Y;
            float b1 = p1.X - p2.X;
            float c1 = a1 * p1.X + b1 * p1.Y;
            float a2 = q2.Y - q1.Y;
            float b2 = q1.X - q2.X;
            float c2 = a2 * q1.X + b2 * q1.Y;
            float det = a1 * b2 - a2 * b1;

            if (!MathUtils.FloatEquals(det, 0))
            {
                // lines are not parallel
                i.X = (b2 * c1 - b1 * c2) / det;
                i.Y = (a1 * c2 - a2 * c1) / det;
            }
            return i;
        }

        public static bool LineIntersect(ref Vector2 point1, ref Vector2 point2, ref Vector2 point3, ref Vector2 point4, bool firstIsSegment, bool secondIsSegment, out Vector2 point)
        {
            point = new Vector2();

            // these are reused later.
            // each lettered sub-calculation is used twice, except
            // for b and d, which are used 3 times
            float a = point4.Y - point3.Y;
            float b = point2.X - point1.X;
            float c = point4.X - point3.X;
            float d = point2.Y - point1.Y;

            // denominator to solution of linear system
            float denom = (a * b) - (c * d);

            // if denominator is 0, then lines are parallel
            if (!(denom >= -MathConstants.Epsilon && denom <= MathConstants.Epsilon))
            {
                float e = point1.Y - point3.Y;
                float f = point1.X - point3.X;
                float oneOverDenom = 1.0f / denom;

                // numerator of first equation
                float ua = (c * e) - (a * f);
                ua *= oneOverDenom;

                // check if intersection point of the two lines is on line segment 1
                if (!firstIsSegment || ua >= 0.0f && ua <= 1.0f)
                {
                    // numerator of second equation
                    float ub = (b * e) - (d * f);
                    ub *= oneOverDenom;

                    // check if intersection point of the two lines is on line segment 2
                    // means the line segments intersect, since we know it is on
                    // segment 1 as well.
                    if (!secondIsSegment || ub >= 0.0f && ub <= 1.0f)
                    {
                        // check if they are coincident (no collision in this case)
                        if (ua != 0f || ub != 0f)
                        {
                            //There is an intersection
                            point.X = point1.X + ua * b;
                            point.Y = point1.Y + ua * d;
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        public static bool LineIntersect(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, bool firstIsSegment, bool secondIsSegment, out Vector2 intersectionPoint)
        {
            return LineIntersect(ref point1, ref point2, ref point3, ref point4, firstIsSegment, secondIsSegment, out intersectionPoint);
        }

        public static Vertices LineSegmentVerticesIntersect(ref Vector2 point1, ref Vector2 point2, Vertices vertices)
        {
            Vertices intersectionPoints = new Vertices();

            for (int i = 0; i < vertices.Count; i++)
            {
                if (LineIntersect(vertices[i], vertices[vertices.NextIndex(i)], point1, point2, true, true, out Vector2 point))
                    intersectionPoints.Add(point);
            }

            return intersectionPoints;
        }

        public static Vertices LineSegmentAABBIntersect(ref Vector2 point1, ref Vector2 point2, AABB aabb)                                              
            => LineSegmentVerticesIntersect(ref point1, ref point2, aabb.Vertices);
        public static bool LineIntersect(ref Vector2 point1, ref Vector2 point2, ref Vector2 point3, ref Vector2 point4, out Vector2 intersectionPoint) 
            => LineIntersect(ref point1, ref point2, ref point3, ref point4, true, true, out intersectionPoint);
        public static bool LineIntersect(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, out Vector2 intersectionPoint)                 
            => LineIntersect(ref point1, ref point2, ref point3, ref point4, true, true, out intersectionPoint);
    }
}
