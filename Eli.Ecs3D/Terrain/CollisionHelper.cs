#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;

namespace Eli.Ecs3D.Terrain
{
    /// <summary>
    /// Represents a simple triangle by the vertices at each corner.
    /// </summary>
    public struct Triangle
    {
        public Vector3 V0;
        public Vector3 V1;
        public Vector3 V2;

        public Vector3 Normal;

        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;

            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v0 - v2;

            Normal = Vector3.Cross(edge1, edge2);
            Normal.Normalize();
        }
    }

    public static class CollisionHelper
    {
        /*
        public static BoundingSphere CreateFromModel(Model model)
        {
            List<Vector3> vertices = null;

            if (model.Tag is ModelInformation)
            {
                ModelInformation modelInfo = (ModelInformation)model.Tag;
                vertices = modelInfo.Vertices;
            }

            else
            {
                Dictionary<string, object> data = (Dictionary<string, object>)model.Tag;
                vertices = (List<Vector3>)data["Vertices"];
            }

            return BoundingSphere.CreateFromPoints(vertices);
        }
        */
        /// <summary>
        /// Do a full perspective transform of the given vector by the given matrix,
        /// dividing out the w coordinate to return a Vector3 result.
        /// </summary>
        /// <param name="position">Vector3 of a point in space</param>
        /// <param name="matrix">4x4 matrix</param>
        /// <param name="result">Transformed vector after perspective divide</param>
        public static void PerspectiveTransform(ref Vector3 position, ref Matrix matrix, out Vector3 result)
        {
            float w = position.X * matrix.M14 + position.Y * matrix.M24 + position.Z * matrix.M34 + matrix.M44;
            float winv = 1.0f / w;

            float x = position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41;
            float y = position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42;
            float z = position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43;

            result = new Vector3();
            result.X = x * winv;
            result.Y = y * winv;
            result.Z = z * winv;
        }

        /// <summary>
        /// Returns true if the given frustum intersects the triangle (v0,v1,v2).
        /// </summary>
        public static bool Intersects(this BoundingFrustum frustum, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            // A BoundingFrustum is defined by a matrix that projects the frustum shape
            // into the box from (-1,-1,0) to (1,1,1). We will project the triangle
            // through this matrix, and then do a simpler box-triangle test.
            Matrix m = frustum.Matrix;
            Triangle localTri;
            PerspectiveTransform(ref v0, ref m, out localTri.V0);
            PerspectiveTransform(ref v1, ref m, out localTri.V1);
            PerspectiveTransform(ref v2, ref m, out localTri.V2);

            BoundingBox box;
            box.Min = new Vector3(-1, -1, 0);
            box.Max = new Vector3(1, 1, 1);

            return Intersects(ref box, ref localTri.V0, ref localTri.V1, ref localTri.V2);
        }

        /// <summary>
        /// Returns true if the given box intersects the triangle (v0,v1,v2).
        /// </summary>
        public static bool Intersects(ref BoundingBox box, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            Vector3 boxCenter = (box.Max + box.Min) * 0.5f;
            Vector3 boxHalfExtent = (box.Max - box.Min) * 0.5f;

            // Transform the triangle into the local space with the box center at the origin
            Triangle localTri = new Triangle();
            Vector3.Subtract(ref v0, ref boxCenter, out localTri.V0);
            Vector3.Subtract(ref v1, ref boxCenter, out localTri.V1);
            Vector3.Subtract(ref v2, ref boxCenter, out localTri.V2);

            return OriginBoxContains(ref boxHalfExtent, ref localTri) != ContainmentType.Disjoint;
        }

        /// <summary>
        /// Check if an origin-centered, axis-aligned box with the given half extents contains,
        /// intersects, or is disjoint from the given triangle. This is used for the box and
        /// frustum vs. triangle tests.
        /// </summary>
        public static ContainmentType OriginBoxContains(ref Vector3 halfExtent, ref Triangle tri)
        {
            BoundingBox triBounds = new BoundingBox(); // 'new' to work around NetCF bug
            triBounds.Min.X = Math.Min((float) tri.V0.X, Math.Min((float) tri.V1.X, (float) tri.V2.X));
            triBounds.Min.Y = Math.Min((float) tri.V0.Y, Math.Min((float) tri.V1.Y, (float) tri.V2.Y));
            triBounds.Min.Z = Math.Min((float) tri.V0.Z, Math.Min((float) tri.V1.Z, (float) tri.V2.Z));

            triBounds.Max.X = Math.Max((float) tri.V0.X, Math.Max((float) tri.V1.X, (float) tri.V2.X));
            triBounds.Max.Y = Math.Max((float) tri.V0.Y, Math.Max((float) tri.V1.Y, (float) tri.V2.Y));
            triBounds.Max.Z = Math.Max((float) tri.V0.Z, Math.Max((float) tri.V1.Z, (float) tri.V2.Z));

            Vector3 triBoundhalfExtent;
            triBoundhalfExtent.X = (triBounds.Max.X - triBounds.Min.X) * 0.5f;
            triBoundhalfExtent.Y = (triBounds.Max.Y - triBounds.Min.Y) * 0.5f;
            triBoundhalfExtent.Z = (triBounds.Max.Z - triBounds.Min.Z) * 0.5f;

            Vector3 triBoundCenter;
            triBoundCenter.X = (triBounds.Max.X + triBounds.Min.X) * 0.5f;
            triBoundCenter.Y = (triBounds.Max.Y + triBounds.Min.Y) * 0.5f;
            triBoundCenter.Z = (triBounds.Max.Z + triBounds.Min.Z) * 0.5f;

            if (triBoundhalfExtent.X + halfExtent.X <= Math.Abs(triBoundCenter.X) ||
                triBoundhalfExtent.Y + halfExtent.Y <= Math.Abs(triBoundCenter.Y) ||
                triBoundhalfExtent.Z + halfExtent.Z <= Math.Abs(triBoundCenter.Z))
            {
                return ContainmentType.Disjoint;
            }

            if (triBoundhalfExtent.X + Math.Abs(triBoundCenter.X) <= halfExtent.X &&
                triBoundhalfExtent.Y + Math.Abs(triBoundCenter.Y) <= halfExtent.Y &&
                triBoundhalfExtent.Z + Math.Abs(triBoundCenter.Z) <= halfExtent.Z)
            {
                return ContainmentType.Contains;
            }

            Vector3 edge1, edge2, edge3;
            Vector3.Subtract(ref tri.V1, ref tri.V0, out edge1);
            Vector3.Subtract(ref tri.V2, ref tri.V0, out edge2);

            Vector3 normal;
            Vector3.Cross(ref edge1, ref edge2, out normal);
            float triangleDist = Vector3.Dot(tri.V0, normal);
            if (Math.Abs(normal.X * halfExtent.X) + Math.Abs(normal.Y * halfExtent.Y) + Math.Abs(normal.Z * halfExtent.Z) <= Math.Abs(triangleDist))
            {
                return ContainmentType.Disjoint;
            }

            // Worst case: we need to check all 9 possible separating planes
            // defined by Cross(box edge,triangle edge)
            // Check for separation in plane containing an axis of box A and and axis of box B
            //
            // We need to compute all 9 cross products to find them, but a lot of terms drop out
            // since we're working in A's local space. Also, since each such plane is parallel
            // to the defining axis in each box, we know those dot products will be 0 and can
            // omit them.
            Vector3.Subtract(ref tri.V1, ref tri.V2, out edge3);
            float dv0, dv1, dv2, dhalf;

            // a.X ^ b.X = (1,0,0) ^ edge1
            // axis = Vector3(0, -edge1.Z, edge1.Y);
            dv0 = tri.V0.Z * edge1.Y - tri.V0.Y * edge1.Z;
            dv1 = tri.V1.Z * edge1.Y - tri.V1.Y * edge1.Z;
            dv2 = tri.V2.Z * edge1.Y - tri.V2.Y * edge1.Z;
            dhalf = Math.Abs(halfExtent.Y * edge1.Z) + Math.Abs(halfExtent.Z * edge1.Y);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.X ^ b.Y = (1,0,0) ^ edge2
            // axis = Vector3(0, -edge2.Z, edge2.Y);
            dv0 = tri.V0.Z * edge2.Y - tri.V0.Y * edge2.Z;
            dv1 = tri.V1.Z * edge2.Y - tri.V1.Y * edge2.Z;
            dv2 = tri.V2.Z * edge2.Y - tri.V2.Y * edge2.Z;
            dhalf = Math.Abs(halfExtent.Y * edge2.Z) + Math.Abs(halfExtent.Z * edge2.Y);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.X ^ b.Y = (1,0,0) ^ edge3
            // axis = Vector3(0, -edge3.Z, edge3.Y);
            dv0 = tri.V0.Z * edge3.Y - tri.V0.Y * edge3.Z;
            dv1 = tri.V1.Z * edge3.Y - tri.V1.Y * edge3.Z;
            dv2 = tri.V2.Z * edge3.Y - tri.V2.Y * edge3.Z;
            dhalf = Math.Abs(halfExtent.Y * edge3.Z) + Math.Abs(halfExtent.Z * edge3.Y);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.Y ^ b.X = (0,1,0) ^ edge1
            // axis = Vector3(edge1.Z, 0, -edge1.X);
            dv0 = tri.V0.X * edge1.Z - tri.V0.Z * edge1.X;
            dv1 = tri.V1.X * edge1.Z - tri.V1.Z * edge1.X;
            dv2 = tri.V2.X * edge1.Z - tri.V2.Z * edge1.X;
            dhalf = Math.Abs(halfExtent.X * edge1.Z) + Math.Abs(halfExtent.Z * edge1.X);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.Y ^ b.X = (0,1,0) ^ edge2
            // axis = Vector3(edge2.Z, 0, -edge2.X);
            dv0 = tri.V0.X * edge2.Z - tri.V0.Z * edge2.X;
            dv1 = tri.V1.X * edge2.Z - tri.V1.Z * edge2.X;
            dv2 = tri.V2.X * edge2.Z - tri.V2.Z * edge2.X;
            dhalf = Math.Abs(halfExtent.X * edge2.Z) + Math.Abs(halfExtent.Z * edge2.X);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.Y ^ b.X = (0,1,0) ^ bX
            // axis = Vector3(edge3.Z, 0, -edge3.X);
            dv0 = tri.V0.X * edge3.Z - tri.V0.Z * edge3.X;
            dv1 = tri.V1.X * edge3.Z - tri.V1.Z * edge3.X;
            dv2 = tri.V2.X * edge3.Z - tri.V2.Z * edge3.X;
            dhalf = Math.Abs(halfExtent.X * edge3.Z) + Math.Abs(halfExtent.Z * edge3.X);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.Y ^ b.X = (0,0,1) ^ edge1
            // axis = Vector3(-edge1.Y, edge1.X, 0);
            dv0 = tri.V0.Y * edge1.X - tri.V0.X * edge1.Y;
            dv1 = tri.V1.Y * edge1.X - tri.V1.X * edge1.Y;
            dv2 = tri.V2.Y * edge1.X - tri.V2.X * edge1.Y;
            dhalf = Math.Abs(halfExtent.Y * edge1.X) + Math.Abs(halfExtent.X * edge1.Y);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.Y ^ b.X = (0,0,1) ^ edge2
            // axis = Vector3(-edge2.Y, edge2.X, 0);
            dv0 = tri.V0.Y * edge2.X - tri.V0.X * edge2.Y;
            dv1 = tri.V1.Y * edge2.X - tri.V1.X * edge2.Y;
            dv2 = tri.V2.Y * edge2.X - tri.V2.X * edge2.Y;
            dhalf = Math.Abs(halfExtent.Y * edge2.X) + Math.Abs(halfExtent.X * edge2.Y);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            // a.Y ^ b.X = (0,0,1) ^ edge3
            // axis = Vector3(-edge3.Y, edge3.X, 0);
            dv0 = tri.V0.Y * edge3.X - tri.V0.X * edge3.Y;
            dv1 = tri.V1.Y * edge3.X - tri.V1.X * edge3.Y;
            dv2 = tri.V2.Y * edge3.X - tri.V2.X * edge3.Y;
            dhalf = Math.Abs(halfExtent.Y * edge3.X) + Math.Abs(halfExtent.X * edge3.Y);
            if (Math.Min(dv0, Math.Min(dv1, dv2)) >= dhalf || Math.Max(dv0, Math.Max(dv1, dv2)) <= -dhalf)
                return ContainmentType.Disjoint;

            return ContainmentType.Intersects;
        }

        public static float? RayIntersectsModel(Vector3[] vertices, Matrix sceneTransform, Ray pickRay)
        {
            Matrix inverseSceneTransform = Matrix.Invert(sceneTransform);

            Vector3 rayPosition = Vector3.Transform(pickRay.Position,
                                                    inverseSceneTransform);

            Vector3 rayDirection = Vector3.TransformNormal(pickRay.Direction,
                                                           inverseSceneTransform);

            pickRay.Position = rayPosition;
            pickRay.Direction = rayDirection;

            float? closestIntersection = null;

            for (int i = 0; i < vertices.Length; i += 3)
            {
                float? intersection = null;

                if (i + 1 < vertices.Length && i + 2 < vertices.Length)
                {
                    RayIntersectsTriangle(ref pickRay,
                                          ref vertices[i],
                                          ref vertices[i + 1],
                                          ref vertices[i + 2],
                                          out intersection);
                }

                if (intersection != null)
                {
                    if (closestIntersection == null || intersection < closestIntersection)
                        closestIntersection = intersection;
                }
            }

            return closestIntersection;
        }

        public static float? RayIntersectsModel(Vector3[] vertices, BoundingBox bounds, Matrix sceneTransform, Ray pickRay)
        {
            if (pickRay.Intersects(bounds) == null)
                return null;

            else
            {
                Matrix inverseSceneTransform = Matrix.Invert(sceneTransform);

                Vector3 rayPosition = Vector3.Transform(pickRay.Position,
                                                        inverseSceneTransform);

                Vector3 rayDirection = Vector3.TransformNormal(pickRay.Direction,
                                                               inverseSceneTransform);

                pickRay.Position = rayPosition;
                pickRay.Direction = rayDirection;

                float? closestIntersection = null;

                for (int i = 0; i < vertices.Length; i += 3)
                {
                    float? intersection = null;

                    if (i + 1 < vertices.Length && i + 2 < vertices.Length)
                    {
                        RayIntersectsTriangle(ref pickRay,
                                              ref vertices[i],
                                              ref vertices[i + 1],
                                              ref vertices[i + 2],
                                              out intersection);
                    }

                    if (intersection != null)
                    {
                        if ((closestIntersection == null) ||
                            (intersection < closestIntersection))
                        {
                            closestIntersection = intersection;
                        }
                    }
                }

                return closestIntersection;
            }
        }

        public static void RayIntersectsTriangle(ref Ray ray,
                                  ref Vector3 vertex1,
                                  ref Vector3 vertex2,
                                  ref Vector3 vertex3, out float? result)
        {
            // Compute vectors along two edges of the triangle.
            Vector3 edge1, edge2;

            Vector3.Subtract(ref vertex2, ref vertex1, out edge1);
            Vector3.Subtract(ref vertex3, ref vertex1, out edge2);

            // Compute the determinant.
            Vector3 directionCrossEdge2;
            Vector3.Cross(ref ray.Direction, ref edge2, out directionCrossEdge2);

            float determinant;
            Vector3.Dot(ref edge1, ref directionCrossEdge2, out determinant);

            // If the ray is parallel to the triangle plane, there is no collision.
            if (determinant > -float.Epsilon && determinant < float.Epsilon)
            {
                result = null;
                return;
            }

            // If CullBackFaces And det < 0 Then Exit Function
            //if (determinant > 0)
            //{
            //    result = null;
            //    return;
            //}

            float inverseDeterminant = 1.0f / determinant;

            // Calculate the U parameter of the intersection point.
            Vector3 distanceVector;
            Vector3.Subtract(ref ray.Position, ref vertex1, out distanceVector);

            float triangleU;
            Vector3.Dot(ref distanceVector, ref directionCrossEdge2, out triangleU);
            triangleU *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleU < 0 || triangleU > 1)
            {
                result = null;
                return;
            }

            // Calculate the V parameter of the intersection point.
            Vector3 distanceCrossEdge1;
            Vector3.Cross(ref distanceVector, ref edge1, out distanceCrossEdge1);

            float triangleV;
            Vector3.Dot(ref ray.Direction, ref distanceCrossEdge1, out triangleV);
            triangleV *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleV < 0 || triangleU + triangleV > 1)
            {
                result = null;
                return;
            }

            // Compute the distance along the ray to the triangle.
            float rayDistance;
            Vector3.Dot(ref edge2, ref distanceCrossEdge1, out rayDistance);
            rayDistance *= inverseDeterminant;

            // Is the triangle behind the ray origin?
            if (rayDistance < 0)
            {
                result = null;
                return;
            }

            result = rayDistance;
        }
    }
}
