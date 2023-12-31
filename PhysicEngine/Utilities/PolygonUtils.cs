﻿using Microsoft.Xna.Framework;
using PhysicEngine.Shared;
using PhysicEngine.Tools.TextureTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PhysicEngine.Utilities
{
    public static class PolygonUtils
    {
        public static Vertices CreateRectangle(float hx, float hy)
        {
            Vertices vertices = new Vertices(4);
            vertices.Add(new Vector2(-hx, -hy));
            vertices.Add(new Vector2(hx, -hy));
            vertices.Add(new Vector2(hx, hy));
            vertices.Add(new Vector2(-hx, hy));

            return vertices;
        }

        public static Vertices CreateRectangle(float hx, float hy, Vector2 center, float angle)
        {
            Vertices vertices = CreateRectangle(hx, hy);

            Transform xf = new Transform();
            xf.p = center;
            xf.q.Set(angle);

            // Transform vertices
            for (int i = 0; i < 4; ++i)
            {
                vertices[i] = MathUtils.Mul(ref xf, vertices[i]);
            }

            return vertices;
        }

        public static Vertices CreateRoundedRectangle(float width, float height, float xRadius, float yRadius, int segments)
        {
            if (yRadius > height / 2 || xRadius > width / 2)
                throw new Exception("Rounding amount can't be more than half the height and width respectively.");
            if (segments < 0)
                throw new Exception("Segments must be zero or more.");

            Vertices vertices = new Vertices();
            if (segments == 0)
            {
                vertices.Add(new Vector2(width * .5f - xRadius, -height * .5f));
                vertices.Add(new Vector2(width * .5f, -height * .5f + yRadius));

                vertices.Add(new Vector2(width * .5f, height * .5f - yRadius));
                vertices.Add(new Vector2(width * .5f - xRadius, height * .5f));

                vertices.Add(new Vector2(-width * .5f + xRadius, height * .5f));
                vertices.Add(new Vector2(-width * .5f, height * .5f - yRadius));

                vertices.Add(new Vector2(-width * .5f, -height * .5f + yRadius));
                vertices.Add(new Vector2(-width * .5f + xRadius, -height * .5f));
            }
            else
            {
                int numberOfEdges = segments * 4 + 8;

                float stepSize = MathConstants.TwoPi / (numberOfEdges - 4);
                int perPhase = numberOfEdges / 4;

                Vector2 posOffset = new Vector2(width / 2 - xRadius, height / 2 - yRadius);
                vertices.Add(posOffset + new Vector2(xRadius, -yRadius + yRadius));
                short phase = 0;
                for (int i = 1; i < numberOfEdges; i++)
                {
                    if (i - perPhase == 0 || i - perPhase * 3 == 0)
                    {
                        posOffset.X *= -1;
                        phase--;
                    }
                    else if (i - perPhase * 2 == 0)
                    {
                        posOffset.Y *= -1;
                        phase--;
                    }

                    vertices.Add(posOffset + new Vector2(xRadius * (float)Math.Cos(stepSize * -(i + phase)),
                        -yRadius * (float)Math.Sin(stepSize * -(i + phase))));
                }
            }

            return vertices;
        }

        public static Vertices CreateLine(Vector2 start, Vector2 end)
        {
            Vertices vertices = new Vertices(2);
            vertices.Add(start);
            vertices.Add(end);

            return vertices;
        }

        public static Vertices CreateCircle(float radius, int numberOfEdges) => CreateEllipse(radius, radius, numberOfEdges);

        public static Vertices CreateEllipse(float xRadius, float yRadius, int numberOfEdges)
        {
            Vertices vertices = new Vertices();

            float stepSize = MathConstants.TwoPi / numberOfEdges;

            vertices.Add(new Vector2(xRadius, 0));
            for (int i = numberOfEdges - 1; i > 0; --i)
            {
                vertices.Add(new Vector2(xRadius * (float)Math.Cos(stepSize * i),
                    -yRadius * (float)Math.Sin(stepSize * i)));
            }

            return vertices;
        }

        public static Vertices CreateArc(float radians, int sides, float radius)
        {
            Debug.Assert(radians > 0, "The arc needs to be larger than 0");
            Debug.Assert(sides > 1, "The arc needs to have more than 1 sides");
            Debug.Assert(radius > 0, "The arc needs to have a radius larger than 0");

            Vertices vertices = new Vertices();

            float stepSize = radians / sides;
            for (int i = sides - 1; i > 0; i--)
            {
                vertices.Add(new Vector2(radius * (float)Math.Cos(stepSize * i),
                    radius * (float)Math.Sin(stepSize * i)));
            }

            return vertices;
        }

        public static Vertices CreateCapsule(float height, float endRadius, int edges)
        {
            if (endRadius >= height / 2)
                throw new ArgumentException("The radius must be lower than height / 2. Higher values of radius would create a circle, and not a half circle.", nameof(endRadius));

            return CreateCapsule(height, endRadius, edges, endRadius, edges);
        }

        public static Vertices CreateCapsule(float height, float topRadius, int topEdges, float bottomRadius, int bottomEdges)
        {
            if (height <= 0)
                throw new ArgumentException("Height must be longer than 0", nameof(height));

            if (topRadius <= 0)
                throw new ArgumentException("The top radius must be more than 0", nameof(topRadius));

            if (topEdges <= 0)
                throw new ArgumentException("Top edges must be more than 0", nameof(topEdges));

            if (bottomRadius <= 0)
                throw new ArgumentException("The bottom radius must be more than 0", nameof(bottomRadius));

            if (bottomEdges <= 0)
                throw new ArgumentException("Bottom edges must be more than 0", nameof(bottomEdges));

            if (topRadius >= height / 2)
                throw new ArgumentException("The top radius must be lower than height / 2. Higher values of top radius would create a circle, and not a half circle.", nameof(topRadius));

            if (bottomRadius >= height / 2)
                throw new ArgumentException("The bottom radius must be lower than height / 2. Higher values of bottom radius would create a circle, and not a half circle.", nameof(bottomRadius));

            Vertices vertices = new Vertices();

            float newHeight = (height - topRadius - bottomRadius) * 0.5f;

            // top
            vertices.Add(new Vector2(topRadius, newHeight));

            float stepSize = MathConstants.Pi / topEdges;
            for (int i = 1; i < topEdges; i++)
            {
                vertices.Add(new Vector2(topRadius * (float)Math.Cos(stepSize * i),
                    topRadius * (float)Math.Sin(stepSize * i) + newHeight));
            }

            vertices.Add(new Vector2(-topRadius, newHeight));

            // bottom
            vertices.Add(new Vector2(-bottomRadius, -newHeight));

            stepSize = MathConstants.Pi / bottomEdges;
            for (int i = 1; i < bottomEdges; i++)
            {
                vertices.Add(new Vector2(-bottomRadius * (float)Math.Cos(stepSize * i),
                    -bottomRadius * (float)Math.Sin(stepSize * i) - newHeight));
            }

            vertices.Add(new Vector2(bottomRadius, -newHeight));

            return vertices;
        }

        public static Vertices CreateGear(float radius, int numberOfTeeth, float tipPercentage, float toothHeight)
        {
            Vertices vertices = new Vertices();

            float stepSize = MathConstants.TwoPi / numberOfTeeth;
            tipPercentage /= 100f;
            MathHelper.Clamp(tipPercentage, 0f, 1f);
            float toothTipStepSize = (stepSize / 2f) * tipPercentage;

            float toothAngleStepSize = (stepSize - (toothTipStepSize * 2f)) / 2f;

            for (int i = numberOfTeeth - 1; i >= 0; --i)
            {
                if (toothTipStepSize > 0f)
                {
                    vertices.Add(
                        new Vector2(radius *
                                    (float)Math.Cos(stepSize * i + toothAngleStepSize * 2f + toothTipStepSize),
                            -radius *
                            (float)Math.Sin(stepSize * i + toothAngleStepSize * 2f + toothTipStepSize)));

                    vertices.Add(
                        new Vector2((radius + toothHeight) *
                                    (float)Math.Cos(stepSize * i + toothAngleStepSize + toothTipStepSize),
                            -(radius + toothHeight) *
                            (float)Math.Sin(stepSize * i + toothAngleStepSize + toothTipStepSize)));
                }

                vertices.Add(new Vector2((radius + toothHeight) *
                                         (float)Math.Cos(stepSize * i + toothAngleStepSize),
                    -(radius + toothHeight) *
                    (float)Math.Sin(stepSize * i + toothAngleStepSize)));

                vertices.Add(new Vector2(radius * (float)Math.Cos(stepSize * i),
                    -radius * (float)Math.Sin(stepSize * i)));
            }

            return vertices;
        }

        public static Vertices CreatePolygon(uint[] data, int width) => TextureConverter.DetectVertices(data, width);

        public static Vertices CreatePolygon(uint[] data, int width, bool holeDetection) => TextureConverter.DetectVertices(data, width, holeDetection);

        public static List<Vertices> CreatePolygon(uint[] data, int width, float hullTolerance, byte alphaTolerance, bool multiPartDetection, bool holeDetection) 
            => TextureConverter.DetectVertices(data, width, hullTolerance, alphaTolerance, multiPartDetection, holeDetection);
    }
}
