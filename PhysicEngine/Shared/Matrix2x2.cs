using Microsoft.Xna.Framework;

namespace PhysicEngine.Shared
{
    public struct Matrix2x2
    {
        public Vector2 ex, ey;

        public Matrix2x2 Inverse
        {
            get
            {
                float a = ex.X, b = ey.X, c = ex.Y, d = ey.Y;
                float det = a * d - b * c;
                if (det != 0.0f)
                    det = 1.0f / det;

                Matrix2x2 result = new();

                result.ex.X = det * d;
                result.ex.Y = -det * c;

                result.ey.X = -det * b;
                result.ey.Y = det * a;

                return result;
            }
        }

        public Matrix2x2(Vector2 c1, Vector2 c2)
        {
            ex = c1;
            ey = c2;
        }

        public Matrix2x2(float a11, float a12, float a21, float a22)
        {
            ex = new Vector2(a11, a21);
            ey = new Vector2(a12, a22);
        }

        public void Set(Vector2 c1, Vector2 c2)
        {
            ex = c1;
            ey = c2;
        }
        public void SetIdentity()
        {
            ex.X = 1.0f;
            ey.X = 0.0f;
            ex.Y = 0.0f;
            ey.Y = 1.0f;
        }
        public void SetZero()
        {
            ex.X = 0.0f;
            ey.X = 0.0f;
            ex.Y = 0.0f;
            ey.Y = 0.0f;
        }

        public Vector2 Solve(Vector2 b)
        {
            float a11 = ex.X, a12 = ey.X, a21 = ex.Y, a22 = ey.Y;
            float det = a11 * a22 - a12 * a21;
            if (det != 0.0f)
                det = 1.0f / det;

            return new Vector2(det * (a22 * b.X - a12 * b.Y), det * (a11 * b.Y - a21 * b.X));
        }

        public static void Add(ref Matrix2x2 A, ref Matrix2x2 B, out Matrix2x2 R)
        {
            R.ex = A.ex + B.ex;
            R.ey = A.ey + B.ey;
        }
    }
}
