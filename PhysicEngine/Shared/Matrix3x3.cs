using Microsoft.Xna.Framework;
using PhysicEngine.Utilities;

namespace PhysicEngine.Shared
{
    public struct Matrix3x3
    {
        public Vector3 ex, ey, ez;

        public Matrix3x3(Vector3 c1, Vector3 c2, Vector3 c3)
        {
            ex = c1;
            ey = c2;
            ez = c3;
        }

        public void SetZero()
        {
            ex = Vector3.Zero;
            ey = Vector3.Zero;
            ez = Vector3.Zero;
        }
        public Vector3 Solve3x3(Vector3 b)
        {
            float det = Vector3.Dot(ex, Vector3.Cross(ey, ez));
            if (det != 0.0f)
                det = 1.0f / det;

            return new Vector3(det * Vector3.Dot(b, Vector3.Cross(ey, ez)), det * Vector3.Dot(ex, Vector3.Cross(b, ez)), det * Vector3.Dot(ex, Vector3.Cross(ey, b)));
        }
        public Vector2 Solve2x2(Vector2 b)
        {
            float a11 = ex.X, a12 = ey.X, a21 = ex.Y, a22 = ey.Y;
            float det = a11 * a22 - a12 * a21;

            if (det != 0.0f)
                det = 1.0f / det;

            return new Vector2(det * (a22 * b.X - a12 * b.Y), det * (a11 * b.Y - a21 * b.X));
        }
        public void GetInverse2x2(ref Matrix3x3 M)
        {
            float a = ex.X, b = ey.X, c = ex.Y, d = ey.Y;
            float det = a * d - b * c;
            if (det != 0.0f)
                det = 1.0f / det;

            M.ex.X = det * d;
            M.ey.X = -det * b;
            M.ex.Z = 0.0f;
            M.ex.Y = -det * c;
            M.ey.Y = det * a;
            M.ey.Z = 0.0f;
            M.ez.X = 0.0f;
            M.ez.Y = 0.0f;
            M.ez.Z = 0.0f;
        }
        public void GetSymInverse3x3(ref Matrix3x3 M)
        {
            float det = MathUtils.Dot(ex, MathUtils.Cross(ey, ez));
            if (det != 0.0f)
                det = 1.0f / det;

            float a11 = ex.X, a12 = ey.X, a13 = ez.X;
            float a22 = ey.Y, a23 = ez.Y;
            float a33 = ez.Z;

            M.ex.X = det * (a22 * a33 - a23 * a23);
            M.ex.Y = det * (a13 * a23 - a12 * a33);
            M.ex.Z = det * (a12 * a23 - a13 * a22);

            M.ey.X = M.ex.Y;
            M.ey.Y = det * (a11 * a33 - a13 * a13);
            M.ey.Z = det * (a13 * a12 - a11 * a23);

            M.ez.X = M.ex.Z;
            M.ez.Y = M.ey.Z;
            M.ez.Z = det * (a11 * a22 - a12 * a12);
        }
    }
}
