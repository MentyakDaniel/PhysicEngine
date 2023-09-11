using Microsoft.Xna.Framework;
using PhysicEngine.Collision.Shapes;
using PhysicEngine.Shared;

namespace PhysicEngine.Definition.Shapes
{
    public sealed class ChainShapeDef : ShapeDef
    {
        public ChainShapeDef() : base(ShapeType.Chain)
        {
            SetDefaults();
        }

        /// <summary>Establish connectivity to a vertex that follows the last vertex.
        /// <remarks>Don't call this for loops.</remarks>
        /// </summary>
        public Vector2 NextVertex { get; set; }

        /// <summary>Establish connectivity to a vertex that precedes the first vertex.
        /// <remarks>Don't call this for loops.</remarks>
        /// </summary>
        public Vector2 PrevVertex { get; set; }

        /// <summary>The vertices. These are not owned/freed by the chain Shape.</summary>
        public Vertices Vertices { get; set; }

        public override void SetDefaults()
        {
            NextVertex = Vector2.Zero;
            PrevVertex = Vector2.Zero;
            Vertices = null;

            base.SetDefaults();
        }
    }
}
