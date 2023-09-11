using PhysicEngine.Shared;
using PhysicEngine.Collision.Shapes;

namespace PhysicEngine.Definition.Shapes
{
    public sealed class PolygonShapeDef : ShapeDef
    {
        public PolygonShapeDef() : base(ShapeType.Polygon)
        {
            SetDefaults();
        }

        public Vertices Vertices { get; set; }

        public override void SetDefaults()
        {
            Vertices = null;
            base.SetDefaults();
        }
    }
}
