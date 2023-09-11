﻿namespace PhysicEngine.Tools.Triangulation.Delaunary.Polygon
{
    internal class PolygonPoint : TriangulationPoint
    {
        public PolygonPoint(double x, double y) : base(x, y) { }

        public PolygonPoint Next { get; set; }
        public PolygonPoint Previous { get; set; }
    }
}
