using Microsoft.Xna.Framework;
using PhysicEngine.Shared;
using PhysicEngine.Tools.Triangulation.Delaunary.Delaunay;
using PhysicEngine.Tools.Triangulation.Delaunary.Delaunay.Sweep;
using System.Collections.Generic;
using System.Diagnostics;

namespace PhysicEngine.Tools.Triangulation.Delaunary
{
    internal static class CDTDecomposer
    {
        /// <summary>Decompose the polygon into several smaller non-concave polygon.</summary>
        public static List<Vertices> ConvexPartition(Vertices vertices)
        {
            Debug.Assert(vertices.Count > 3);

            Polygon.Polygon poly = new Polygon.Polygon();

            foreach (Vector2 vertex in vertices)
            {
                poly.Points.Add(new TriangulationPoint(vertex.X, vertex.Y));
            }

            if (vertices.Holes != null)
            {
                foreach (Vertices holeVertices in vertices.Holes)
                {
                    Polygon.Polygon hole = new Polygon.Polygon();

                    foreach (Vector2 vertex in holeVertices)
                    {
                        hole.Points.Add(new TriangulationPoint(vertex.X, vertex.Y));
                    }

                    poly.AddHole(hole);
                }
            }

            DTSweepContext tcx = new DTSweepContext();
            tcx.PrepareTriangulation(poly);
            DTSweep.Triangulate(tcx);

            List<Vertices> results = new List<Vertices>();

            foreach (DelaunayTriangle triangle in poly.Triangles)
            {
                Vertices v = new Vertices();
                foreach (TriangulationPoint p in triangle.Points)
                {
                    v.Add(new Vector2((float)p.X, (float)p.Y));
                }
                results.Add(v);
            }

            return results;
        }
    }
}
