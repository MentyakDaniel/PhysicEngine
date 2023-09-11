using Microsoft.Xna.Framework;
using System;

namespace PhysicEngine.Shared
{
    public struct Rot
    {
        /// Sine and cosine
        public float sin, cos;

        public Rot(float angle)
        {
            sin = (float)Math.Sin(angle);
            cos = (float)Math.Cos(angle);
        }
        public void Set(float angle)
        {
            //Velcro: Optimization
            if (angle == 0)
            {
                sin = 0;
                cos = 1;
            }
            else
            {
                // TODO_ERIN optimize
                sin = (float)Math.Sin(angle);
                cos = (float)Math.Cos(angle);
            }
        }

        /// <summary>Set to the identity rotation</summary>
        public void SetIdentity()
        {
            sin = 0.0f;
            cos = 1.0f;
        }

        /// <summary>Get the angle in radians</summary>
        public float GetAngle()
        {
            return (float)Math.Atan2(sin, cos);
        }

        /// <summary>Get the x-axis</summary>
        public Vector2 GetXAxis()
        {
            return new Vector2(cos, sin);
        }

        /// <summary>Get the y-axis</summary>
        public Vector2 GetYAxis()
        {
            return new Vector2(-sin, cos);
        }
    }
}
