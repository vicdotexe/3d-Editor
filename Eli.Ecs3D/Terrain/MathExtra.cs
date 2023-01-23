//======================================================================
// XNA Terrain Editor
// Copyright (C) 2008 Eric Grossinger
// http://psycad007.spaces.live.com/
//======================================================================

using System;
using Microsoft.Xna.Framework;

namespace Eli.Ecs3D.Terrain
{
    public class MathExtra
    {
        // <summary Special Thanks>
        // to minahito http://lablab.jp/
        // ref: http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=1144568&SiteID=1
        // </summary>
        public static bool Intersects(Ray ray, Vector3 a, Vector3 b, Vector3 c, Vector3 normal, bool positiveSide, bool negativeSide, out float t)
        {
            t = 0;
            {
                float denom = Vector3.Dot(normal, ray.Direction);

                if (denom > float.Epsilon)
                {
                    if (!negativeSide)
                        return false;
                }
                else if (denom < -float.Epsilon)
                {
                    if (!positiveSide)
                        return false;
                }
                else
                {
                    return false;
                }

                t = Vector3.Dot(normal, a - ray.Position) / denom;

                if (t < 0)
                {
                    // Interersection is behind origin
                    return false;
                }
            }

            // Calculate the largest area projection plane in X, Y or Z.
            int i0, i1;
            {
                float n0 = Math.Abs(normal.X);
                float n1 = Math.Abs(normal.Y);
                float n2 = Math.Abs(normal.Z);

                i0 = 1;
                i1 = 2;

                if (n1 > n2)
                {
                    if (n1 > n0) i0 = 0;
                }
                else
                {
                    if (n2 > n0) i1 = 0;
                }
            }

            float[] A = { a.X, a.Y, a.Z };
            float[] B = { b.X, b.Y, b.Z };
            float[] C = { c.X, c.Y, c.Z };
            float[] R = { ray.Direction.X, ray.Direction.Y, ray.Direction.Z };
            float[] RO = { ray.Position.X, ray.Position.Y, ray.Position.Z };

            // Check the intersection point is inside the triangle.
            {
                float u1 = B[i0] - A[i0];
                float v1 = B[i1] - A[i1];
                float u2 = C[i0] - A[i0];
                float v2 = C[i1] - A[i1];
                float u0 = t * R[i0] + RO[i0] - A[i0];
                float v0 = t * R[i1] + RO[i1] - A[i1];

                float alpha = u0 * v2 - u2 * v0;
                float beta = u1 * v0 - u0 * v1;
                float area = u1 * v2 - u2 * v1;

                float EPSILON = 1e-3f;

                float tolerance = EPSILON * area;

                if (area > 0)
                {
                    if (alpha < tolerance || beta < tolerance || alpha + beta > area - tolerance)
                        return false;
                }
                else
                {
                    if (alpha > tolerance || beta > tolerance || alpha + beta < area - tolerance)
                        return false;
                }
            }

            return true;
        }
    }
}
