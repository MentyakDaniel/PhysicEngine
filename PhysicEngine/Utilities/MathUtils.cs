using Microsoft.Xna.Framework;
using PhysicEngine.Shared;
using System;

namespace PhysicEngine.Utilities
{
    public static class MathUtils
    {
        public static int Clamp(int a, int low, int high)   => Math.Max(low, Math.Min(a, high));
        public static int Max(int valueA, int valueB)       => Math.Max(valueA, valueB);
        public static int Min(int valueA, int valueB)       => Math.Min(valueA, valueB);
        public static int Sign(float value)                 => Math.Sign(value);

        public static float Cross(ref Vector2 a, ref Vector2 b)                 => a.X * b.Y - a.Y * b.X;
        public static float Abs(float value)                                    => Math.Abs(value);
        public static float Cross(Vector2 a, Vector2 b)                         => Cross(ref a, ref b);
        public static float Clamp(float a, float low, float high)               => Max(low, Min(a, high));
        public static float Dot(Vector3 a, Vector3 b)                           => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        public static float Dot(ref Vector2 a, ref Vector2 b)                   => a.X * b.X + a.Y * b.Y;
        public static float Dot(Vector2 a, Vector2 b)                           => a.X * b.X + a.Y * b.Y;
        public static float Area(Vector2 a, Vector2 b, Vector2 c)               => Area(ref a, ref b, ref c);
        public static float Area(ref Vector2 a, ref Vector2 b, ref Vector2 c)   => a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y);
        public static float Distance(Vector2 a, Vector2 b)                      => (a - b).Length();
        public static float Distance(ref Vector2 a, ref Vector2 b)              => (a - b).Length();
        public static float Max(float valueA, float valueB)                     => Math.Max(valueA, valueB);
        public static float Min(float valueA, float valueB)                     => Math.Min(valueA, valueB);
        public static float Sqrt(float value)                                   => (float)Math.Sqrt(value);
        public static float Cosf(float value)                                   => (float)Math.Cos(value);
        public static float Sinf(float value)                                   => (float)Math.Sin(value);
        public static float Ceil(float log)                                     => (float)Math.Ceiling(log);
        public static float Log(float log)                                      => (float)Math.Log(log);

        public static double VectorAngle(Vector2 p1, Vector2 p2) => VectorAngle(ref p1, ref p2);

        public static Vector2 Cross(Vector2 a, float s)                     => new Vector2(s * a.Y, -s * a.X);
        public static Vector2 Cross(float s, Vector2 a)                     => new Vector2(-s * a.Y, s * a.X);
        public static Vector2 Abs(Vector2 v)                                => new Vector2(Math.Abs(v.X), Math.Abs(v.Y));
        public static Vector2 Mul(ref Matrix2x2 a, Vector2 v)               => Mul(ref a, ref v);
        public static Vector2 Mul(ref Matrix2x2 a, ref Vector2 v)           => new Vector2(a.ex.X * v.X + a.ey.X * v.Y, a.ex.Y * v.X + a.ey.Y * v.Y);
        public static Vector2 Mul(ref Transform T, Vector2 v)               => Mul(ref T, ref v);
        public static Vector2 MulT(ref Matrix2x2 a, Vector2 v)              => MulT(ref a, ref v);
        public static Vector2 MulT(ref Matrix2x2 a, ref Vector2 v)          => new Vector2(v.X * a.ex.X + v.Y * a.ex.Y, v.X * a.ey.X + v.Y * a.ey.Y);
        public static Vector2 MulT(ref Transform T, Vector2 v)              => MulT(ref T, ref v);
        public static Vector2 Mul22(Matrix3x3 a, Vector2 v)                 => new Vector2(a.ex.X * v.X + a.ey.X * v.Y, a.ex.Y * v.X + a.ey.Y * v.Y);
        public static Vector2 Mul(Rot q, Vector2 v)                         => new Vector2(q.cos * v.X - q.sin * v.Y, q.sin * v.X + q.cos * v.Y);
        public static Vector2 MulT(Rot q, Vector2 v)                        => new Vector2(q.cos * v.X + q.sin * v.Y, -q.sin * v.X + q.cos * v.Y);
        public static Vector2 Skew(Vector2 input)                           => new Vector2(-input.Y, input.X);
        public static Vector2 Clamp(Vector2 a, Vector2 low, Vector2 high)   => Vector2.Max(low, Vector2.Min(a, high));
        public static Vector2 Mul(ref Rot rot, Vector2 axis)                => Mul(rot, axis);
        public static Vector2 MulT(ref Rot rot, Vector2 axis)               => MulT(rot, axis);

        public static Vector3 Cross(Vector3 a, Vector3 b) => new Vector3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        public static Vector3 Mul(Matrix3x3 a, Vector3 v) => v.X * a.ex + v.Y * a.ey + v.Z * a.ez;

        public static void Cross(ref Vector2 a, ref Vector2 b, out float c)     => c = a.X * b.Y - a.Y * b.X;
        public static void Cross(float s, ref Vector2 a, out Vector2 b)         => b = new Vector2(-s * a.Y, s * a.X);

        public static bool IsValid(this Vector2 x)                                                          => IsValid(x.X) && IsValid(x.Y);
        public static bool IsCollinear(ref Vector2 a, ref Vector2 b, ref Vector2 c, float tolerance = 0)    => FloatInRange(Area(ref a, ref b, ref c), -tolerance, tolerance);
        public static bool FloatEquals(float value1, float value2)                                          => Math.Abs(value1 - value2) <= MathConstants.Epsilon;
        public static bool FloatEquals(float value1, float value2, float delta)                             => FloatInRange(value1, value2 - delta, value2 + delta);
        public static bool FloatInRange(float value, float min, float max)                                  => value >= min && value <= max;

        public static bool IsValid(float x)
        {
            if (float.IsNaN(x))
                return false;

            return !float.IsInfinity(x);
        }

        public static Transform Mul(Transform a, Transform b)
        {
            Transform c = new Transform();
            c.q = Mul(a.q, b.q);
            c.p = Mul(a.q, b.p) + a.p;
            return c;
        }
        public static Transform MulT(Transform a, Transform b)
        {
            Transform c = new Transform();
            c.q = MulT(a.q, b.q);
            c.p = MulT(a.q, b.p - a.p);
            return c;
        }

        public static Rot Mul(Rot q, Rot r)
        {
            Rot qr;
            qr.sin = q.sin * r.cos + q.cos * r.sin;
            qr.cos = q.cos * r.cos - q.sin * r.sin;
            return qr;
        }
        public static Rot MulT(Rot q, Rot r)
        {
            Rot qr;
            qr.sin = q.cos * r.sin - q.sin * r.cos;
            qr.cos = q.cos * r.cos + q.sin * r.sin;
            return qr;
        }

        public static void MulT(ref Matrix2x2 a, ref Matrix2x2 b, out Matrix2x2 c)
        {
            c = new Matrix2x2();
            c.ex.X = a.ex.X * b.ex.X + a.ex.Y * b.ex.Y;
            c.ex.Y = a.ey.X * b.ex.X + a.ey.Y * b.ex.Y;
            c.ey.X = a.ex.X * b.ey.X + a.ex.Y * b.ey.Y;
            c.ey.Y = a.ey.X * b.ey.X + a.ey.Y * b.ey.Y;
        }
        public static void MulT(ref Transform a, ref Transform b, out Transform c)
        {
            c = new Transform();
            c.q = MulT(a.q, b.q);
            c.p = MulT(a.q, b.p - a.p);
        }
        public static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        public static Vector2 MulT(Transform T, Vector2 v)
        {
            float px = v.X - T.p.X;
            float py = v.Y - T.p.Y;
            float x = T.q.cos * px + T.q.sin * py;
            float y = -T.q.sin * px + T.q.cos * py;

            return new Vector2(x, y);
        }
        public static Vector2 MulT(ref Transform T, ref Vector2 v)
        {
            float px = v.X - T.p.X;
            float py = v.Y - T.p.Y;
            float x = T.q.cos * px + T.q.sin * py;
            float y = -T.q.sin * px + T.q.cos * py;

            return new Vector2(x, y);
        }
        public static Vector2 Mul(ref Transform T, ref Vector2 v)
        {
            float x = T.q.cos * v.X - T.q.sin * v.Y + T.p.X;
            float y = T.q.sin * v.X + T.q.cos * v.Y + T.p.Y;

            return new Vector2(x, y);
        }

        public static double VectorAngle(ref Vector2 p1, ref Vector2 p2)
        {
            double theta1 = Math.Atan2(p1.Y, p1.X);
            double theta2 = Math.Atan2(p2.Y, p2.X);
            double dtheta = theta2 - theta1;

            while (dtheta > MathConstants.Pi)
            {
                dtheta -= MathConstants.TwoPi;
            }

            while (dtheta < -MathConstants.Pi)
            {
                dtheta += MathConstants.TwoPi;
            }

            return dtheta;
        }

        public static float Normalize(ref Vector2 v)
        {
            float length = v.Length();
            if (length < MathConstants.Epsilon)
            {
                return 0.0f;
            }
            float invLength = 1.0f / length;
            v.X *= invLength;
            v.Y *= invLength;

            return length;
        }
        public static float DistanceSquared(ref Vector2 a, ref Vector2 b)
        {
            Vector2 c = a - b;
            return Dot(ref c, ref c);
        }
    }
}