using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Eli.Ecs3D.Terrain
{
    public enum NodeType
    {
        Leaf = 0,
        Node = 1
    }

    public struct BoundingSquare
    {
        public Vector3 UpperRight;
        public Vector3 UpperLeft;
        public Vector3 LowerRight;
        public Vector3 LowerLeft;
    }

    public class QuadNode
    {
        public int Index;
        public int ParentIndex;

        public NodeType Type;
        public int[] Children = new int[4];

        public BoundingBox boundingBox;
        public BoundingSquare BoundingCoordinates;

        public List<int> TriangleIDs = new List<int>();

        public QuadNode()
        {

        }
    }
}
