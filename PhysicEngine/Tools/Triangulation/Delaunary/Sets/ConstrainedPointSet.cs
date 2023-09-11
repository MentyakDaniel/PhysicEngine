using System.Collections.Generic;

namespace PhysicEngine.Tools.Triangulation.Delaunary.Sets
{
    internal class ConstrainedPointSet : PointSet
    {
        private List<TriangulationPoint> _constrainedPointList;

        public ConstrainedPointSet(List<TriangulationPoint> points, int[] index)
            : base(points)
        {
            EdgeIndex = index;
        }

        /**
         * 
         * @param points - A list of all points in PointSet
         * @param constraints - Pairs of two points defining a constraint, all points <b>must</b> be part of given PointSet!
         */
        public ConstrainedPointSet(List<TriangulationPoint> points, IEnumerable<TriangulationPoint> constraints)
            : base(points)
        {
            _constrainedPointList = new List<TriangulationPoint>();
            _constrainedPointList.AddRange(constraints);
        }

        public int[] EdgeIndex { get; private set; }

        public override TriangulationMode TriangulationMode => TriangulationMode.Constrained;

        public override void PrepareTriangulation(TriangulationContext tcx)
        {
            base.PrepareTriangulation(tcx);
            if (_constrainedPointList != null)
            {
                TriangulationPoint p1, p2;
                using (List<TriangulationPoint>.Enumerator iterator = _constrainedPointList.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        p1 = iterator.Current;
                        iterator.MoveNext();
                        p2 = iterator.Current;
                        tcx.NewConstraint(p1, p2);
                    }
                }
            }
            else
            {
                for (int i = 0; i < EdgeIndex.Length; i += 2)
                {
                    // XXX: must change!!
                    tcx.NewConstraint(Points[EdgeIndex[i]], Points[EdgeIndex[i + 1]]);
                }
            }
        }
    }
}
