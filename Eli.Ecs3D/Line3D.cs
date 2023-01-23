using System;
using System.Collections.Generic;
using System.Text;
using Eli.Ecs3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli
{
    public class Line3D : Primitive3DBase<VertexPositionColor>
    {
        public Vector3 Start;
        public Vector3 End;
        public Color LineColor;
        public Line3D(Vector3 start, Vector3 end, Color lineColor) : base(2, 2)
        {
            Start = start;
            End = end;
            LineColor = lineColor;
        }

        public override void Construct()
        {
            _indices[0] = 0;
            _indices[1] = 1;
            _vertices[0] = new VertexPositionColor(Start, LineColor);
            _vertices[1] = new VertexPositionColor(End, LineColor);
        }

        protected override void InitializeEffect()
        {
            Effect.VertexColorEnabled = true;
            BlendState = BlendState.Opaque;
            RasterizerState = RasterizerState.CullNone;;
            DepthStencilState = DepthStencilState.None;
        }

        public override void Draw(Camera3D camera, GraphicsDevice graphicsDevice)
        {
            if (_isDirty)
            {
                Construct();
                InitializePrimitive();
                _isDirty = false;
            }

            BeginDraw(camera, graphicsDevice);

            // Set BasicEffect parameters.
            Effect.World = Matrix.Identity;
            Effect.View = camera.ViewMatrix;
            Effect.Projection = camera.ProjectionMatrix;
            Effect.DiffuseColor = DiffuseColor.ToVector3();

            // Set our vertex declaration, vertex buffer, and index buffer.
            graphicsDevice.SetVertexBuffer(_vertexBuffer);
            Core.GraphicsDevice.Indices = _indexBuffer;

            Effect.CurrentTechnique.Passes[0].Apply();
            var primitiveCount = _indices.Length;
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, primitiveCount);
        }
    }
}
