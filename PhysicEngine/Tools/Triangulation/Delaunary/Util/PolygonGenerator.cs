using PhysicEngine.Tools.Triangulation.Delaunary.Polygon;
using PhysicEngine.Utilities;
using System;

namespace PhysicEngine.Tools.Triangulation.Delaunary.Util
{
    internal class PolygonGenerator
    {
        private static readonly Random _rng = new Random();

        public static Polygon.Polygon RandomCircleSweep(double scale, int vertexCount)
        {
            double radius = scale / 4;

            PolygonPoint[] points = new PolygonPoint[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                do
                {
                    if (i % 250 == 0)
                        radius += scale / 2 * (0.5 - _rng.NextDouble());
                    else if (i % 50 == 0)
                        radius += scale / 5 * (0.5 - _rng.NextDouble());
                    else
                        radius += 25 * scale / vertexCount * (0.5 - _rng.NextDouble());

                    radius = radius > scale / 2 ? scale / 2 : radius;
                    radius = radius < scale / 10 ? scale / 10 : radius;
                } while (radius < scale / 10 || radius > scale / 2);

                points[i] = new PolygonPoint(radius * Math.Cos(MathConstants.TwoPi * i / vertexCount), radius * Math.Sin(MathConstants.TwoPi * i / vertexCount));
            }
            return new Polygon.Polygon(points);
        }

        public static Polygon.Polygon RandomCircleSweep2(double scale, int vertexCount)
        {
            double radius = scale / 4;
            PolygonPoint[] points = new PolygonPoint[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                do
                {
                    radius += scale / 5 * (0.5 - _rng.NextDouble());
                    radius = radius > scale / 2 ? scale / 2 : radius;
                    radius = radius < scale / 10 ? scale / 10 : radius;
                } while (radius < scale / 10 || radius > scale / 2);

                points[i] = new PolygonPoint(radius * Math.Cos(MathConstants.TwoPi * i / vertexCount), radius * Math.Sin(MathConstants.TwoPi * i / vertexCount));
            }
            return new Polygon.Polygon(points);
        }
    }
}
