//======================================================================
// XNA Terrain Editor
// Copyright (C) 2008 Eric Grossinger
// http://psycad007.spaces.live.com/
//======================================================================

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SceneEngine;

namespace Eli.Ecs3D.Terrain
{
    public class QuadTree
    {
        private float gridWidth;
        private float gridHeight;

        private int totalTreeID = 0;

        private float cellSize;

        private List<QuadNode> nodeList;

        private BoundingSquare boundingCoordinates;
        private BoundingBox boundingBox;

        private HeightMap heightMap;

        public QuadTree(HeightMap heightMap)
        {
            this.heightMap = heightMap;
            this.cellSize = heightMap.CellSize;

            gridWidth = heightMap.Width / cellSize;
            gridHeight = heightMap.Height / cellSize;

            int numberOfLeaves = ((int)gridWidth / 4) * ((int)gridHeight / 4);
            int numberOfNodes = CalculateNodeCount(numberOfLeaves, 4);

            nodeList = new List<QuadNode>(numberOfNodes);

            for (int i = 0; i < numberOfNodes; i++)
            {
                nodeList.Add(null);
            }

            Vector3 A = Vector3.Zero;
            Vector3 B = new Vector3(heightMap.Width, 0, 0);
            Vector3 C = new Vector3(0, 0, heightMap.Height);
            Vector3 D = new Vector3(heightMap.Width, 0, heightMap.Height);

            boundingCoordinates.LowerLeft = A;
            boundingCoordinates.LowerRight = B;
            boundingCoordinates.UpperLeft = C;
            boundingCoordinates.UpperRight = D;

            boundingBox = new BoundingBox(new Vector3(boundingCoordinates.LowerLeft.X, 
                                                      heightMap.LowestHeight, 
                                                      boundingCoordinates.LowerLeft.Z), 
                                          
                                          new Vector3(boundingCoordinates.UpperRight.X, 
                                                      heightMap.HighestHeight, 
                                                      boundingCoordinates.UpperRight.Z));

            CreateNode(boundingCoordinates, 0, 0, heightMap.Triangles);
        }

        private int CalculateNodeCount(int numberOfLeaves, int leafWidth)
        {
            int counter = 0;
            int var = 0;

            while (var == 0)
            {
                counter += numberOfLeaves;
                numberOfLeaves /= leafWidth;

                if (numberOfLeaves == 1)
                { var = 1; }
            }
            counter++;

            return counter;
        }

        private void CreateNode(BoundingSquare boundingSquare, int parentIndex, int index, Triangle[] triangles)
        {
            NodeType nodeType;

            float width;
            float height;

            width = boundingSquare.UpperRight.X - boundingSquare.UpperLeft.X; //X
            height = boundingSquare.UpperLeft.Z - boundingSquare.LowerLeft.Z; //Z

            if (width / 2 == (2 * (int)cellSize))
            {
                nodeType = NodeType.Leaf;
            }
            else
            {
                nodeType = NodeType.Node;
            }

            QuadNode node = new QuadNode();

            node.Index = index;
            node.ParentIndex = parentIndex;

            node.BoundingCoordinates.UpperLeft = boundingSquare.UpperLeft;
            node.BoundingCoordinates.UpperRight = boundingSquare.UpperRight;
            node.BoundingCoordinates.LowerLeft = boundingSquare.LowerLeft;
            node.BoundingCoordinates.LowerRight = boundingSquare.LowerRight;

            Vector3 min = new Vector3(node.BoundingCoordinates.LowerLeft.X, 
                                      heightMap.LowestHeight, 
                                      node.BoundingCoordinates.LowerLeft.Z);

            Vector3 max = new Vector3(node.BoundingCoordinates.UpperRight.X, 
                                      heightMap.HighestHeight, 
                                      node.BoundingCoordinates.UpperRight.Z);

            node.boundingBox = new BoundingBox(min, max);

            node.Type = nodeType;

            if (nodeType == NodeType.Leaf)
            {
                int tID;
                int o = 0;
                float lowestPoint = heightMap.MaxHeight;
                float highestPoint = 0f;

                for (int y = (int)node.BoundingCoordinates.LowerLeft.Z / (int)cellSize; y < ((node.BoundingCoordinates.UpperRight.Z / cellSize) - 0); y++)
                {
                    for (int x = (int)node.BoundingCoordinates.LowerLeft.X / (int)cellSize; x < ((node.BoundingCoordinates.UpperRight.X / cellSize) - 0); x++)
                    {
                        tID = (x + y * (heightMap.Size - 1)) * 2;

                        if (tID >= triangles.Length - 0)
                        {
                            o++;
                        }
                        if (tID < triangles.Length)
                        {
                            node.TriangleIDs.Add(tID);
                            node.TriangleIDs.Add(tID + 1);

                            if (triangles[tID].V0.Y > highestPoint)
                                highestPoint = triangles[tID].V0.Y;
                            if (triangles[tID].V1.Y > highestPoint)
                                highestPoint = triangles[tID].V1.Y;
                            if (triangles[tID].V2.Y > highestPoint)
                                highestPoint = triangles[tID].V2.Y;

                            if (triangles[tID].V0.Y < lowestPoint)
                                lowestPoint = triangles[tID].V0.Y;
                            if (triangles[tID].V1.Y < lowestPoint)
                                lowestPoint = triangles[tID].V1.Y;
                            if (triangles[tID].V2.Y < lowestPoint)
                                lowestPoint = triangles[tID].V2.Y;

                            if (triangles[tID + 1].V0.Y > highestPoint)
                                highestPoint = triangles[tID + 1].V0.Y;
                            if (triangles[tID + 1].V1.Y > highestPoint)
                                highestPoint = triangles[tID + 1].V1.Y;
                            if (triangles[tID + 1].V2.Y > highestPoint)
                                highestPoint = triangles[tID + 1].V2.Y;

                            if (triangles[tID + 1].V0.Y < lowestPoint)
                                lowestPoint = triangles[tID + 1].V0.Y;
                            if (triangles[tID + 1].V1.Y < lowestPoint)
                                lowestPoint = triangles[tID + 1].V1.Y;
                            if (triangles[tID + 1].V2.Y < lowestPoint)
                                lowestPoint = triangles[tID + 1].V2.Y;
                        }
                    }

                    //Determine the height of the bounding box for this Leaf Node
                    node.boundingBox.Min.Y = lowestPoint;
                    node.boundingBox.Max.Y = highestPoint;
                }
            }
            else
            {
                BoundingSquare BoundingBox = new BoundingSquare();
                totalTreeID++;
                node.Children[0] = totalTreeID;

                BoundingBox.LowerLeft = boundingSquare.LowerLeft;
                BoundingBox.LowerRight = boundingSquare.LowerLeft + ((boundingSquare.LowerRight - boundingSquare.LowerLeft) / 2);
                BoundingBox.UpperLeft = boundingSquare.LowerLeft + ((boundingSquare.UpperLeft - boundingSquare.LowerLeft) / 2);
                BoundingBox.UpperRight = boundingSquare.LowerLeft + ((boundingSquare.LowerRight - boundingSquare.LowerLeft) / 2) + ((boundingSquare.UpperLeft - boundingSquare.LowerLeft) / 2);

                CreateNode(BoundingBox, index, totalTreeID, triangles);

                //Determine the height of the bounding box for this Node
                if (nodeList[totalTreeID].boundingBox.Max.Y > node.boundingBox.Max.Y)
                    node.boundingBox.Max.Y = nodeList[totalTreeID].boundingBox.Max.Y;

                if (nodeList[totalTreeID].boundingBox.Min.Y < node.boundingBox.Min.Y)
                    node.boundingBox.Min.Y = nodeList[totalTreeID].boundingBox.Min.Y;

                //**************************************************************************

                totalTreeID++;
                node.Children[1] = totalTreeID;

                BoundingBox.LowerLeft = boundingSquare.LowerLeft + ((boundingSquare.LowerRight - boundingSquare.LowerLeft) / 2);
                BoundingBox.LowerRight = boundingSquare.LowerRight;
                BoundingBox.UpperLeft = boundingSquare.LowerLeft + ((boundingSquare.LowerRight - boundingSquare.LowerLeft) / 2) + ((boundingSquare.UpperLeft - boundingSquare.LowerLeft) / 2);
                BoundingBox.UpperRight = boundingSquare.LowerLeft + ((boundingSquare.UpperLeft - boundingSquare.LowerLeft) / 2) + ((boundingSquare.LowerRight - boundingSquare.LowerLeft));

                CreateNode(BoundingBox, index, totalTreeID, triangles);

                //Determine the height of the bounding box for this Node
                if (nodeList[totalTreeID].boundingBox.Max.Y > node.boundingBox.Max.Y)
                    node.boundingBox.Max.Y = nodeList[totalTreeID].boundingBox.Max.Y;

                if (nodeList[totalTreeID].boundingBox.Min.Y < node.boundingBox.Min.Y)
                    node.boundingBox.Min.Y = nodeList[totalTreeID].boundingBox.Min.Y;

                //**************************************************************************

                totalTreeID++;
                node.Children[2] = totalTreeID;

                //LowerLeft
                BoundingBox.LowerLeft = boundingSquare.LowerLeft + ((boundingSquare.UpperLeft - boundingSquare.LowerLeft) / 2);
                //LowerRight
                BoundingBox.LowerRight = boundingSquare.LowerLeft + ((boundingSquare.UpperLeft - boundingSquare.LowerLeft) / 2)
                                            + ((boundingSquare.LowerRight - boundingSquare.LowerLeft) / 2);
                //UpperLeft
                BoundingBox.UpperLeft = boundingSquare.UpperLeft;
                //UpperRight
                BoundingBox.UpperRight = boundingSquare.LowerLeft + ((boundingSquare.LowerRight - boundingSquare.LowerLeft) / 2) + ((boundingSquare.UpperLeft - boundingSquare.LowerLeft));

                CreateNode(BoundingBox, index, totalTreeID, triangles);

                //Determine the height of the bounding box for this Node
                if (nodeList[totalTreeID].boundingBox.Max.Y > node.boundingBox.Max.Y)
                    node.boundingBox.Max.Y = nodeList[totalTreeID].boundingBox.Max.Y;

                if (nodeList[totalTreeID].boundingBox.Min.Y < node.boundingBox.Min.Y)
                    node.boundingBox.Min.Y = nodeList[totalTreeID].boundingBox.Min.Y;

                //**************************************************************************

                totalTreeID++;
                node.Children[3] = totalTreeID;

                //LowerLeft
                BoundingBox.LowerLeft = boundingSquare.LowerLeft + ((boundingSquare.LowerRight - boundingSquare.LowerLeft) / 2) + ((boundingSquare.UpperLeft - boundingSquare.LowerLeft) / 2);
                //LowerRight
                BoundingBox.LowerRight = boundingSquare.LowerLeft + ((boundingSquare.UpperLeft - boundingSquare.LowerLeft) / 2) + ((boundingSquare.LowerRight - boundingSquare.LowerLeft));
                //UpperLeft
                BoundingBox.UpperLeft = boundingSquare.LowerLeft + ((boundingSquare.LowerRight - boundingSquare.LowerLeft) / 2) + ((boundingSquare.UpperLeft - boundingSquare.LowerLeft));
                //UpperRight
                BoundingBox.UpperRight = boundingSquare.UpperRight;

                CreateNode(BoundingBox, index, totalTreeID, triangles);

                //Determine the height of the bounding box for this Node
                if (nodeList[totalTreeID].boundingBox.Max.Y > node.boundingBox.Max.Y)
                    node.boundingBox.Max.Y = nodeList[totalTreeID].boundingBox.Max.Y;

                if (nodeList[totalTreeID].boundingBox.Min.Y < node.boundingBox.Min.Y)
                    node.boundingBox.Min.Y = nodeList[totalTreeID].boundingBox.Min.Y;


            }
            nodeList[index] = node;

            //Determine the height of the bounding box for the QuadTree
            if (node.boundingBox.Max.Y > boundingBox.Max.Y)
                boundingBox.Max.Y = node.boundingBox.Max.Y;

            if (node.boundingBox.Min.Y < boundingBox.Min.Y)
                boundingBox.Min.Y = node.boundingBox.Min.Y;


            return;
        }

        private void GetTriangleIndexes(QuadNode ParentNode, ref List<int> tempIndexes, ref Ray ray)
        {
            for (int i = 0; i < 4; i++)
            {
                QuadNode branchNode = nodeList[ParentNode.Children[i]];

                float? rayLength;
                branchNode.boundingBox.Intersects(ref ray, out rayLength);

                //Does the Ray intersect the Bounding Box of this Node
                if (rayLength != null || branchNode.boundingBox.Contains(ray.Position) != ContainmentType.Disjoint)
                {
                    if (branchNode.Type == NodeType.Node)
                    {
                        GetTriangleIndexes(branchNode, ref tempIndexes, ref ray);
                    }
                    else
                    {
                        tempIndexes.AddRange(branchNode.TriangleIDs);
                    }
                }
            }
        }

        private void UpdateBoundingBox(QuadNode parentNode, int traingleIndex)
        {
            float lowestPoint = heightMap.MaxHeight;
            float highestPoint = 0f;

            UpdateBoundingBox(parentNode, traingleIndex, ref lowestPoint, ref highestPoint);
        }
        private void UpdateBoundingBox(QuadNode parentNode, int triangleIndex, ref float lowestPoint, ref float highestPoint)
        {
            for (int i = 0; i < 4; i++)
            {
                QuadNode branchNode = nodeList[parentNode.Children[i]];

                if (heightMap.Triangles[triangleIndex].V0.X > branchNode.boundingBox.Min.X && 
                    heightMap.Triangles[triangleIndex].V0.X < branchNode.boundingBox.Max.X &&
                    heightMap.Triangles[triangleIndex].V0.Z > branchNode.boundingBox.Min.Z &&
                    heightMap.Triangles[triangleIndex].V0.Z < branchNode.boundingBox.Max.Z)
                {
                    if (branchNode.Type == NodeType.Node)
                    {
                        UpdateBoundingBox(branchNode, triangleIndex, ref lowestPoint, ref highestPoint);

                        //Update the bounding box for the Branch Nodes
                        if (highestPoint > branchNode.boundingBox.Max.Y)
                            branchNode.boundingBox.Max.Y = highestPoint;

                        if (lowestPoint < branchNode.boundingBox.Min.Y)
                            branchNode.boundingBox.Min.Y = lowestPoint;

                    }
                    else
                    {

                        foreach (int tID in branchNode.TriangleIDs)
                        {
                            if (heightMap.Triangles[tID].V0.Y > highestPoint)
                                highestPoint = heightMap.Triangles[tID].V0.Y;
                            if (heightMap.Triangles[tID].V1.Y > highestPoint)
                                highestPoint = heightMap.Triangles[tID].V1.Y;
                            if (heightMap.Triangles[tID].V2.Y > highestPoint)
                                highestPoint = heightMap.Triangles[tID].V2.Y;

                            if (heightMap.Triangles[tID].V0.Y < lowestPoint)
                                lowestPoint = heightMap.Triangles[tID].V0.Y;
                            if (heightMap.Triangles[tID].V1.Y < lowestPoint)
                                lowestPoint = heightMap.Triangles[tID].V1.Y;
                            if (heightMap.Triangles[tID].V2.Y < lowestPoint)
                                lowestPoint = heightMap.Triangles[tID].V2.Y;
                        }

                        //Update the bounding box for the Leaf Node
                        branchNode.boundingBox.Max.Y = highestPoint;
                        branchNode.boundingBox.Min.Y = lowestPoint;

                    }
                }
            }
            return;
        }

        public void UpdateBoundingBox(int traingleIndex)
        {
            UpdateBoundingBox(nodeList[0], traingleIndex);
        }

        public bool InsideBoundingBox(Triangle triangle, BoundingSquare box)
        {
            bool inside = false;

            if (triangle.V0.X >= box.UpperLeft.X && triangle.V0.X <= box.UpperRight.X)
            {
                if (triangle.V0.Z >= box.LowerLeft.Z && triangle.V0.X <= box.UpperLeft.Z)
                {
                    inside = true;
                }
            }

            if (triangle.V1.X >= box.UpperLeft.X && triangle.V1.X <= box.UpperRight.X)
            {
                if (triangle.V1.Z >= box.LowerLeft.Z && triangle.V1.X <= box.UpperLeft.Z)
                {
                    inside = true;
                }
            }

            if (triangle.V2.X >= box.UpperLeft.X && triangle.V2.X <= box.UpperRight.X)
            {
                if (triangle.V2.Z >= box.LowerLeft.Z && triangle.V2.X <= box.UpperLeft.Z)
                {
                    inside = true;
                }
            }
            return inside;
        }

        public float? Intersects(ref Ray ray, out Triangle triangle, Matrix world)
        {
            float? rayLength = null;
            triangle = new Triangle();
            ray.Position = Vector3.Transform(ray.Position, Matrix.Invert(world));
            ray.Direction = Vector3.TransformNormal(ray.Direction, Matrix.Invert(world));
            boundingBox.Intersects(ref ray, out rayLength);

            if ((rayLength.HasValue && rayLength.Value > 0.0f) ||
                boundingBox.Contains(ray.Position) != ContainmentType.Disjoint)
            {
                List<int> triangleList = new List<int>();
                GetTriangleIndexes(nodeList[0], ref triangleList, ref ray);

                foreach (int index in triangleList)
                {
                    triangle = heightMap.Triangles[index];
                    rayLength = null;

                    CollisionHelper.RayIntersectsTriangle(ref ray, ref triangle.V0,
                        ref triangle.V1, ref triangle.V2, out rayLength);

                    if (rayLength.HasValue)
                    {
                        return rayLength.Value > 0.0f ? rayLength : null;
                    }
                }
            }

            return null;
        }

        public float? Intersects(ref Ray ray, out Triangle triangle)
        {
            float? rayLength = null;
            triangle = new Triangle();
            boundingBox.Intersects(ref ray, out rayLength);

            if ((rayLength.HasValue && rayLength.Value > 0.0f) || 
                boundingBox.Contains(ray.Position) != ContainmentType.Disjoint)
            {
                List<int> triangleList = new List<int>();
                GetTriangleIndexes(nodeList[0], ref triangleList, ref ray);

                foreach (int index in triangleList)
                {
                    triangle = heightMap.Triangles[index];
                    rayLength = null;

                    CollisionHelper.RayIntersectsTriangle(ref ray, ref triangle.V0,
                        ref triangle.V1, ref triangle.V2, out rayLength);

                    if (rayLength.HasValue)
                    {
                        return rayLength.Value > 0.0f ? rayLength : null;
                    }
                }
            }

            return null;
        }
        public float? IntersectsClosest(ref Ray ray)
        {
            float? rayLength = null;
            
            boundingBox.Intersects(ref ray, out rayLength);

            if (rayLength.HasValue ||
                boundingBox.Contains(ray.Position) != ContainmentType.Disjoint)
            {
                List<int> triangleList = new List<int>();
                GetTriangleIndexes(nodeList[0], ref triangleList, ref ray);

                float? shortestLength = float.MaxValue;
                bool collided = false;

                foreach (int index in triangleList)
                {
                    Triangle triangle = heightMap.Triangles[index];
                    rayLength = null;

                    CollisionHelper.RayIntersectsTriangle(ref ray, ref triangle.V0,
                        ref triangle.V1, ref triangle.V2, out rayLength);

                    if (rayLength.HasValue && rayLength.Value < shortestLength)
                    {
                        shortestLength = rayLength;
                        collided = true;
                    }
                }

                return collided ? shortestLength : null;
            }

            return null;
        }
    }
}
