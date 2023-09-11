using System.Collections.Generic;

namespace PhysicEngine.Tools.Triangulation.Delaunary.Polygon
{
    internal class PolygonSet
    {
        protected List<Polygon> _polygons = new List<Polygon>();

        public PolygonSet() { }

        public PolygonSet(Polygon poly) => _polygons.Add(poly);

        public IEnumerable<Polygon> Polygons => _polygons;

        public void Add(Polygon p) => _polygons.Add(p);
    }
}
