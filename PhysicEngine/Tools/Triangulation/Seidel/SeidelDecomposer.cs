using Microsoft.Xna.Framework;
using PhysicEngine.Shared;
using System.Collections.Generic;
using System.Diagnostics;

namespace PhysicEngine.Tools.Triangulation.Seidel
{
    internal static class SeidelDecomposer
    {
        /// <summary>Decompose the polygon into several smaller non-concave polygons.</summary>
        /// <param name="vertices">The polygon to decompose.</param>
        /// <param name="sheer">The sheer to use if you get bad results, try using a higher value.</param>
        /// <returns>A list of triangles</returns>
        public static List<Vertices> ConvexPartition(Vertices vertices, float sheer = 0.001f)
        {
            Debug.Assert(vertices.Count > 3);

            List<Point> compatList = new List<Point>(vertices.Count);

            foreach (Vector2 vertex in vertices)
            {
                compatList.Add(new Point(vertex.X, vertex.Y));
            }

            Triangulator t = new Triangulator(compatList, sheer);

            List<Vertices> list = new List<Vertices>();

            foreach (List<Point> triangle in t.Triangles)
            {
                Vertices outTriangles = new Vertices(triangle.Count);

                foreach (Point outTriangle in triangle)
                {
                    outTriangles.Add(new Vector2(outTriangle.X, outTriangle.Y));
                }

                list.Add(outTriangles);
            }

            return list;
        }

        /// <summary>Decompose the polygon into several smaller non-concave polygons.</summary>
        /// <param name="vertices">The polygon to decompose.</param>
        /// <param name="sheer">The sheer to use if you get bad results, try using a higher value.</param>
        /// <returns>A list of trapezoids</returns>
        public static List<Vertices> ConvexPartitionTrapezoid(Vertices vertices, float sheer = 0.001f)
        {
            List<Point> compatList = new List<Point>(vertices.Count);

            foreach (Vector2 vertex in vertices)
            {
                compatList.Add(new Point(vertex.X, vertex.Y));
            }

            Triangulator t = new Triangulator(compatList, sheer);

            List<Vertices> list = new List<Vertices>();

            foreach (Trapezoid trapezoid in t.Trapezoids)
            {
                Vertices verts = new Vertices();

                List<Point> points = trapezoid.GetVertices();
                foreach (Point point in points)
                {
                    verts.Add(new Vector2(point.X, point.Y));
                }

                list.Add(verts);
            }

            return list;
        }
    }
}
