using Microsoft.Xna.Framework;
using PhysicEngine.Collision.Raycast;
using PhysicEngine.Shared;
using System.Diagnostics;

namespace PhysicEngine.Collision.Shapes
{
    public abstract class Shape
    {
        internal float _density;
        internal float _radius;
        internal ShapeType _shapeType;
        internal MassData _massData;

        protected Shape(ShapeType type, float radius = 0, float density = 0)
        {
            Debug.Assert(radius >= 0);
            Debug.Assert(density >= 0);

            _shapeType = type;
            _radius = radius;
            _density = density;
            _massData = new MassData();
        }

        public void GetMassData(out MassData massData)
        {
            massData = _massData;
        }
        public ShapeType ShapeType => _shapeType;

        public abstract int ChildCount { get; }

        public float Radius
        {
            get => _radius;
            set
            {
                Debug.Assert(value >= 0);

                if (_radius != value)
                {
                    _radius = value;
                    ComputeProperties();
                }
            }
        }

        public float Density
        {
            get => _density;
            set
            {
                Debug.Assert(value >= 0);

                if (_density != value)
                {
                    _density = value;
                    ComputeProperties();
                }
            }
        }

        public abstract Shape Clone();

        public abstract bool TestPoint(ref Transform transform, ref Vector2 point);

        public abstract bool RayCast(ref RaycastInput input, ref Transform transform, int childIndex, out RaycastOutput output);

        public abstract void ComputeAABB(ref Transform transform, int childIndex, out AABB aabb);

        protected abstract void ComputeProperties();
    }
}
