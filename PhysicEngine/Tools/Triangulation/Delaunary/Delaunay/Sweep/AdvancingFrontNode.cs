namespace PhysicEngine.Tools.Triangulation.Delaunary.Delaunay.Sweep
{
    internal class AdvancingFrontNode
    {
        public AdvancingFrontNode Next;
        public TriangulationPoint Point;
        public AdvancingFrontNode Prev;
        public DelaunayTriangle Triangle;
        public double Value;

        public AdvancingFrontNode(TriangulationPoint point)
        {
            Point = point;
            Value = point.X;
        }

        public bool HasNext => Next != null;

        public bool HasPrev => Prev != null;
    }
}
