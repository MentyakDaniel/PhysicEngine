using System;

namespace PhysicEngine.Tools.Triangulation.Delaunary.Delaunay.Sweep
{
    internal class PointOnEdgeException : NotImplementedException
    {
        public PointOnEdgeException(string message)
            : base(message) { }
    }
}
