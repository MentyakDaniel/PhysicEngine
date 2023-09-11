using Microsoft.Xna.Framework;
using PhysicEngine.Collision.Shapes;

namespace PhysicEngine.Definition.Shapes
{
    public sealed class EdgeShapeDef : ShapeDef
    {
        public EdgeShapeDef() : base(ShapeType.Edge)
        {
            SetDefaults();
        }

        /// <summary>Is true if the edge is connected to an adjacent vertex before vertex 1.</summary>
        public bool HasVertex0 { get; set; }

        /// <summary>Is true if the edge is connected to an adjacent vertex after vertex2.</summary>
        public bool HasVertex3 { get; set; }

        /// <summary>Optional adjacent vertices. These are used for smooth collision.</summary>
        public Vector2 Vertex0 { get; set; }

        /// <summary>These are the edge vertices</summary>
        public Vector2 Vertex1 { get; set; }

        /// <summary>These are the edge vertices</summary>
        public Vector2 Vertex2 { get; set; }

        /// <summary>Optional adjacent vertices. These are used for smooth collision.</summary>
        public Vector2 Vertex3 { get; set; }

        public override void SetDefaults()
        {
            HasVertex0 = false;
            HasVertex3 = false;
            Vertex0 = Vector2.Zero;
            Vertex1 = Vector2.Zero;
            Vertex2 = Vector2.Zero;
            Vertex3 = Vector2.Zero;

            base.SetDefaults();
        }
    }
}
