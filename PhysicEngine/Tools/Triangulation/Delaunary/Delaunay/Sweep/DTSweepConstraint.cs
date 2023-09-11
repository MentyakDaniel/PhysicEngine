namespace PhysicEngine.Tools.Triangulation.Delaunary.Delaunay.Sweep
{
    internal class DTSweepConstraint : TriangulationConstraint
    {
        /// <summary>Give two points in any order. Will always be ordered so that q.y > p.y and q.x > p.x if same y value</summary>
        public DTSweepConstraint(TriangulationPoint p1, TriangulationPoint p2)
        {
            P = p1;
            Q = p2;
            if (p1.Y > p2.Y)
            {
                Q = p1;
                P = p2;
            }
            else if (p1.Y == p2.Y)
            {
                if (p1.X > p2.X)
                {
                    Q = p1;
                    P = p2;
                }
                else if (p1.X == p2.X)
                {
                    //                logger.info( "Failed to create constraint {}={}", p1, p2 );
                    //                throw new DuplicatePointException( p1 + "=" + p2 );
                    //                return;
                }
            }
            Q.AddEdge(this);
        }
    }
}
