using Microsoft.Xna.Framework;
using PhysicEngine.Collision.Raycast;
using PhysicEngine.Shared;
using PhysicEngine.Utilities;

namespace PhysicEngine.Collision.Shapes
{
    public class CircleShape : Shape
    {
        internal Vector2 _position;

        /// <summary>Create a new circle with the desired radius and density.</summary>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="density">The density of the circle.</param>
        /// <param name="position">Position of the shape</param>
        public CircleShape(float radius, float density, Vector2 position = default) : base(ShapeType.Circle, radius, density)
        {
            _position = position;
            ComputeProperties();
        }

        public CircleShape(float density) : base(ShapeType.Circle, 0, density)
        {
            ComputeProperties();
        }

        private CircleShape() : base(ShapeType.Circle) { }

        public override int ChildCount => 1;

        /// <summary>Get or set the position of the circle</summary>
        public Vector2 Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    ComputeInertia();
                }
            }
        }

        public override bool TestPoint(ref Transform transform, ref Vector2 point)
        {
            return TestPointHelper.TestPointCircle(ref _position, _radius, ref point, ref transform);
        }

        public override bool RayCast(ref RaycastInput input, ref Transform transform, int childIndex, out RaycastOutput output)
        {
            return RaycastHelper.RayCastCircle(ref _position, _radius, ref input, ref transform, out output);
        }

        public override void ComputeAABB(ref Transform transform, int childIndex, out AABB aabb)
        {
            AABBHelper.ComputeCircleAABB(ref _position, _radius, ref transform, out aabb);
        }

        protected sealed override void ComputeProperties()
        {
            ComputeMass();
            ComputeInertia();
        }

        private void ComputeMass()
        {
            //Velcro: We calculate area for later consumption
            float area = MathConstants.Pi * _radius * _radius;
            _massData._area = area;
            _massData._mass = _density * area;
        }

        private void ComputeInertia()
        {
            _massData._centroid = _position;

            // inertia about the local origin
            _massData._inertia = _massData._mass * (0.5f * _radius * _radius + Vector2.Dot(_position, _position));
        }

        public override Shape Clone()
        {
            CircleShape clone = new CircleShape();
            clone._shapeType = _shapeType;
            clone._radius = _radius;
            clone._density = _density;
            clone._position = _position;
            clone._massData = _massData;
            return clone;
        }
    }
}
