using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli.TopDown3D._3D_Stuff
{
    public class MultiColoredCube : Primitive3DBase<VertexPositionColor>
    {
        Vector3[] normals =
        {
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1),
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, -1, 0),
        };

        Color[] colors =
        {
            Color.Red,
            Color.Yellow,
            Color.Blue,
            Color.Violet,
            Color.Green,
            Color.Orange
        };

        public float Alpha = 1;

        public BoundingBox BoundingBox => new BoundingBox(Position - (Size * 0.5f),Position + (Size * 0.5f));

        public MultiColoredCube(Vector3 position, Vector3 size, float alpha = 1) : base (24, 36)
        {
            Position = position;
            Size = size;
            Alpha = alpha;

        }

        public override void Construct()
        {
            var index = 0;
            var vert = 0;
            for (var i = 0; i < normals.Length; i++)
            {
                var vertColor = colors[i] * Alpha;
                var normal = normals[i];
                var side1 = new Vector3(normal.Y, normal.Z, normal.X);
                var side2 = Vector3.Cross(normal, side1);

                _indices[index++] = (ushort)(vert + 0);
                _indices[index++] = (ushort)(vert + 1);
                _indices[index++] = (ushort)(vert + 2);

                _indices[index++] = (ushort)(vert + 0);
                _indices[index++] = (ushort)(vert + 2);
                _indices[index++] = (ushort)(vert + 3);

                /* original
                    _vertices[vert++] = new VertexPositionColor((normal - side1 - side2) / 2, vertColor);
                    _vertices[vert++] = new VertexPositionColor((normal - side1 + side2) / 2, vertColor);
                    _vertices[vert++] = new VertexPositionColor((normal + side1 + side2) / 2, vertColor);
                    _vertices[vert++] = new VertexPositionColor((normal + side1 - side2) / 2, vertColor);
                */

                    _vertices[vert++] = new VertexPositionColor(Vector3.Transform((normal - side1 - side2), Matrix.CreateScale(Size)) / 2 + Position, vertColor);
                    _vertices[vert++] = new VertexPositionColor(Vector3.Transform((normal - side1 + side2), Matrix.CreateScale(Size)) / 2 + Position , vertColor);
                    _vertices[vert++] = new VertexPositionColor(Vector3.Transform((normal + side1 + side2), Matrix.CreateScale(Size)) / 2 + Position , vertColor);
                    _vertices[vert++] = new VertexPositionColor(Vector3.Transform((normal + side1 - side2), Matrix.CreateScale(Size)) / 2 + Position , vertColor);

            }
        }

        protected override void InitializeEffect()
        {
            Effect.VertexColorEnabled = true;
            
        }
    }

    public class Cube
    {

    }

    public class ColoredCube : Primitive3DBase<VertexPositionColor>
    {
        public Color CubeColor;
        public BoundingBox BoundingBox => new BoundingBox(Position - (Size * 0.5f), Position + (Size * 0.5f));
        public ColoredCube(Vector3 position, Vector3 size, Color color, BasicEffect effect = null) : base(8, 36, effect)
        {
            CubeColor = color;
            Position = position;
            Size = size;
            RasterizerState = RasterizerState.CullClockwise;
        }

        public override void Construct()
        {
            _vertices = new VertexPositionColor[8];
            var half = Size * 0.5f;
            var color = CubeColor;

            //front left bottom corner
            _vertices[0] = new VertexPositionColor(new Vector3(-half.X, -half.Y, half.Z) + Position, color);
            //front left upper corner
            _vertices[1] = new VertexPositionColor(new Vector3(-half.X, half.Y, half.Z) + Position, color);
            //front right upper corner
            _vertices[2] = new VertexPositionColor(new Vector3(half.X, half.Y, half.Z) + Position, color);
            //front lower right corner
            _vertices[3] = new VertexPositionColor(new Vector3(half.X, -half.Y, half.Z) + Position, color);
            //back left lower corner
            _vertices[4] = new VertexPositionColor(new Vector3(-half.X, -half.Y, -half.Z) + Position, color);
            //back left upper corner
            _vertices[5] = new VertexPositionColor(new Vector3(-half.X, half.Y, -half.Z) + Position, color);
            //back right upper corner
            _vertices[6] = new VertexPositionColor(new Vector3(half.X, half.Y , -half.Z) + Position, color);
            //back right lower corner
            _vertices[7] = new VertexPositionColor(new Vector3(half.X, -half.Y, -half.Z) + Position, color);

            //Front face
            //bottom right triangle
            _indices[0] = 0;
            _indices[1] = 3;
            _indices[2] = 2;
            //top left triangle
            _indices[3] = 2;
            _indices[4] = 1;
            _indices[5] = 0;
            //back face
            //bottom right triangle
            _indices[6] = 4;
            _indices[7] = 7;
            _indices[8] = 6;
            //top left triangle
            _indices[9] = 6;
            _indices[10] = 5;
            _indices[11] = 4;
            //Top face
            //bottom right triangle
            _indices[12] = 1;
            _indices[13] = 2;
            _indices[14] = 6;
            //top left triangle
            _indices[15] = 6;
            _indices[16] = 5;
            _indices[17] = 1;
            //bottom face
            //bottom right triangle
            _indices[18] = 4;
            _indices[19] = 7;
            _indices[20] = 3;
            //top left triangle
            _indices[21] = 3;
            _indices[22] = 0;
            _indices[23] = 4;
            //left face
            //bottom right triangle
            _indices[24] = 4;
            _indices[25] = 0;
            _indices[26] = 1;
            //top left triangle
            _indices[27] = 1;
            _indices[28] = 5;
            _indices[29] = 4;
            //right face
            //bottom right triangle
            _indices[30] = 3;
            _indices[31] = 7;
            _indices[32] = 6;
            //top left triangle
            _indices[33] = 6;
            _indices[34] = 2;
            _indices[35] = 3;
        }

        protected override void InitializeEffect()
        {
            Effect.VertexColorEnabled = true;
            Effect.EnableDefaultLighting();
        }
    }
}
