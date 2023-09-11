using PhysicEngine.Tools.Triangulation.Delaunary.Delaunay;
using System.Collections.Generic;

namespace PhysicEngine.Tools.Triangulation.Delaunary.Sets
{
    internal class PointSet : Triangulatable
    {
        public PointSet(List<TriangulationPoint> points)
        {
            Points = new List<TriangulationPoint>(points);
        }

        public IList<TriangulationPoint> Points { get; private set; }
        public IList<DelaunayTriangle> Triangles { get; private set; }

        public virtual TriangulationMode TriangulationMode => TriangulationMode.Unconstrained;

        public void AddTriangle(DelaunayTriangle t)
        {
            Triangles.Add(t);
        }

        public void AddTriangles(IEnumerable<DelaunayTriangle> list)
        {
            foreach (DelaunayTriangle tri in list)
                Triangles.Add(tri);
        }

        public void ClearTriangles() => Triangles.Clear();

        public virtual void PrepareTriangulation(TriangulationContext tcx)
        {
            if (Triangles == null)
                Triangles = new List<DelaunayTriangle>(Points.Count);
            else
                Triangles.Clear();
            tcx.Points.AddRange(Points);
        }
    }
}
