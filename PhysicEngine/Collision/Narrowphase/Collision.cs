using Microsoft.Xna.Framework;
using PhysicEngine.Collision.ContactSystem;
using PhysicEngine.Collision.Distance;
using PhysicEngine.Collision.Shapes;
using PhysicEngine.Shared;
using PhysicEngine.Shared.Optimization;
using PhysicEngine.Utilities;

namespace PhysicEngine.Collision.Narrowphase
{
    public static class Collision
    {
        /// <summary>Test overlap between the two shapes.</summary>
        /// <param name="shapeA">The first shape.</param>
        /// <param name="indexA">The index for the first shape.</param>
        /// <param name="shapeB">The second shape.</param>
        /// <param name="indexB">The index for the second shape.</param>
        /// <param name="xfA">The transform for the first shape.</param>
        /// <param name="xfB">The transform for the seconds shape.</param>
        public static bool TestOverlap(Shape shapeA, int indexA, Shape shapeB, int indexB, ref Transform xfA, ref Transform xfB)
        {
            DistanceInput input = new DistanceInput();
            input.ProxyA = new DistanceProxy(shapeA, indexA);
            input.ProxyB = new DistanceProxy(shapeB, indexB);
            input.TransformA = xfA;
            input.TransformB = xfB;
            input.UseRadii = true;

            DistanceGJK.ComputeDistance(ref input, out DistanceOutput output, out _);

            return output.Distance < 10.0f * MathConstants.Epsilon;
        }

        public static void GetPointStates(out FixedArray2<PointState> state1, out FixedArray2<PointState> state2, ref Manifold manifold1, ref Manifold manifold2)
        {
            state1 = new FixedArray2<PointState>();
            state2 = new FixedArray2<PointState>();

            for (int i = 0; i < Settings.MaxManifoldPoints; ++i)
            {
                state1[i] = PointState.Null;
                state2[i] = PointState.Null;
            }

            // Detect persists and removes.
            for (int i = 0; i < manifold1.PointCount; ++i)
            {
                ContactId id = manifold1.Points[i].Id;

                state1[i] = PointState.Remove;

                for (int j = 0; j < manifold2.PointCount; ++j)
                {
                    if (manifold2.Points[j].Id.Key == id.Key)
                    {
                        state1[i] = PointState.Persist;
                        break;
                    }
                }
            }

            // Detect persists and adds.
            for (int i = 0; i < manifold2.PointCount; ++i)
            {
                ContactId id = manifold2.Points[i].Id;

                state2[i] = PointState.Add;

                for (int j = 0; j < manifold1.PointCount; ++j)
                {
                    if (manifold1.Points[j].Id.Key == id.Key)
                    {
                        state2[i] = PointState.Persist;
                        break;
                    }
                }
            }
        }

        /// <summary>Clipping for contact manifolds.</summary>
        /// <param name="vOut">The v out.</param>
        /// <param name="vIn">The v in.</param>
        /// <param name="normal">The normal.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="vertexIndexA">The vertex index A.</param>
        /// <returns></returns>
        internal static int ClipSegmentToLine(out FixedArray2<ClipVertex> vOut, ref FixedArray2<ClipVertex> vIn, Vector2 normal, float offset, int vertexIndexA)
        {
            vOut = new FixedArray2<ClipVertex>();

            // Start with no output points
            int count = 0;

            // Calculate the distance of end points to the line
            float distance0 = Vector2.Dot(normal, vIn.Value0.V) - offset;
            float distance1 = Vector2.Dot(normal, vIn.Value1.V) - offset;

            // If the points are behind the plane
            if (distance0 <= 0.0f) vOut[count++] = vIn.Value0;
            if (distance1 <= 0.0f) vOut[count++] = vIn.Value1;

            // If the points are on different sides of the plane
            if (distance0 * distance1 < 0.0f)
            {
                // Find intersection point of edge and plane
                float interp = distance0 / (distance0 - distance1);

                ClipVertex cv = vOut[count];
                cv.V = vIn.Value0.V + interp * (vIn.Value1.V - vIn.Value0.V);

                // VertexA is hitting edgeB.
                cv.Id.ContactFeature.IndexA = (byte)vertexIndexA;
                cv.Id.ContactFeature.IndexB = vIn.Value0.Id.ContactFeature.IndexB;
                cv.Id.ContactFeature.TypeA = ContactFeatureType.Vertex;
                cv.Id.ContactFeature.TypeB = ContactFeatureType.Face;
                vOut[count] = cv;

                ++count;
            }

            return count;
        }
    }
}
