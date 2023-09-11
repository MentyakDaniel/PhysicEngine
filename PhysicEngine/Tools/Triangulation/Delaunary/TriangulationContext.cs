using PhysicEngine.Tools.Triangulation.Delaunary.Delaunay;
using System.Collections.Generic;

namespace PhysicEngine.Tools.Triangulation.Delaunary
{
    internal abstract class TriangulationContext
    {
        public readonly List<TriangulationPoint> Points = new List<TriangulationPoint>(200);
        public readonly List<DelaunayTriangle> Triangles = new List<DelaunayTriangle>();

        protected TriangulationContext()
        {
            Terminated = false;
        }

        public TriangulationMode TriangulationMode { get; protected set; }
        public Triangulatable Triangulatable { get; private set; }

        public bool Terminated { get; set; }

        public int StepCount { get; private set; }
        public bool IsDebugEnabled { get; protected set; }

        public void Done()
        {
            StepCount++;
        }

        public virtual void PrepareTriangulation(Triangulatable t)
        {
            Triangulatable = t;
            TriangulationMode = t.TriangulationMode;
            t.PrepareTriangulation(this);
        }

        public abstract TriangulationConstraint NewConstraint(TriangulationPoint a, TriangulationPoint b);

        public void Update(string message) { }

        public virtual void Clear()
        {
            Points.Clear();
            Terminated = false;
            StepCount = 0;
        }
    }
}
