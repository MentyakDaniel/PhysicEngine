using PhysicEngine.Tools.Triangulation.Delaunary.Delaunay;
using System.Collections.Generic;

namespace PhysicEngine.Tools.Triangulation.Delaunary
{
    internal interface Triangulatable
    {
        IList<TriangulationPoint> Points { get; } // MM: Neither of these are used via interface (yet?)
        IList<DelaunayTriangle> Triangles { get; }
        TriangulationMode TriangulationMode { get; }
        void PrepareTriangulation(TriangulationContext tcx);

        void AddTriangle(DelaunayTriangle t);
        void AddTriangles(IEnumerable<DelaunayTriangle> list);
        void ClearTriangles();
    }
}
