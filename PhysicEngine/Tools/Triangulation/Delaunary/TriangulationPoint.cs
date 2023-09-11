using PhysicEngine.Tools.Triangulation.Delaunary.Delaunay.Sweep;
using System.Collections.Generic;

namespace PhysicEngine.Tools.Triangulation.Delaunary
{
    internal class TriangulationPoint
    {
        // List of edges this point constitutes an upper ending point (CDT)

        public double X, Y;

        public TriangulationPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public List<DTSweepConstraint> Edges { get; private set; }

        public float Xf
        {
            get => (float)X;
            set => X = value;
        }

        public float Yf
        {
            get => (float)Y;
            set => Y = value;
        }

        public bool HasEdges => Edges != null;

        public override string ToString() => "[" + X + "," + Y + "]";

        public void AddEdge(DTSweepConstraint e)
        {
            if (Edges == null)
                Edges = new List<DTSweepConstraint>();
            Edges.Add(e);
        }
    }
}
