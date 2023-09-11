using Microsoft.Xna.Framework;
using System;

namespace PhysicEngine.Collision.Shapes
{
    public struct MassData : IEquatable<MassData>
    {
        internal float _area;
        internal Vector2 _centroid;
        internal float _inertia;
        internal float _mass;

        public float Area
        {
            get => _area;
            set => _area = value;
        }

        public Vector2 Centroid
        {
            get => _centroid;
            set => _centroid = value;
        }

        public float Inertia
        {
            get => _inertia;
            set => _inertia = value;
        }

        public float Mass
        {
            get => _mass;
            set => _mass = value;
        }

        public static bool operator ==(MassData left, MassData right)
        {
            return left._area == right._area && left._mass == right._mass && left._centroid == right._centroid && left._inertia == right._inertia;
        }

        public static bool operator !=(MassData left, MassData right)
        {
            return !(left == right);
        }

        public bool Equals(MassData other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (obj.GetType() != typeof(MassData))
                return false;

            return Equals((MassData)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = _area.GetHashCode();
                result = (result * 397) ^ _centroid.GetHashCode();
                result = (result * 397) ^ _inertia.GetHashCode();
                result = (result * 397) ^ _mass.GetHashCode();
                return result;
            }
        }
    }
}
