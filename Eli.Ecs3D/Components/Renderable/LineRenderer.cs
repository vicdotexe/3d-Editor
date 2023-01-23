using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eli.ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli.Ecs3D.Components.Renderable
{
    public class LineRenderer : RenderableComponent
    {
        private int _vertexCount;
        private int _vertexCapacity;

        private VertexPositionColor[] vertices;

        private BasicEffect effect;

        public BasicEffect Effect
        {
            get { return effect; }
        }

        public int PrimitiveCount
        {
            get { return (_vertexCount / 2); }
        }

        public LineRenderer(int vertexCapacity)
        {
            _vertexCapacity = vertexCapacity;

            vertices = new VertexPositionColor[vertexCapacity];

            effect = new BasicEffect(Core.GraphicsDevice);

            effect.LightingEnabled = false;
            effect.VertexColorEnabled = true;
        }

        public void Clear()
        {
            _vertexCount = 0;
        }

        protected void SubmitVertex(Vector3 position, Color color)
        {
            vertices[_vertexCount].Position = position;
            vertices[_vertexCount++].Color = color;
        }

        public void Submit(Vector3 start, Vector3 end, Color color)
        {
            SubmitVertex(start, color);
            SubmitVertex(end, color);
        }

        public override void Render(ICamera camera)
        {
            if (_vertexCount >= 2)
            {
                effect.View = camera.ViewMatrix;
                effect.Projection = camera.ProjectionMatrix;

                effect.CurrentTechnique.Passes[0].Apply();

                Core.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                    PrimitiveType.LineList, vertices, 0, _vertexCount / 2);
            }
        }

        protected override void CalculateBoundingSphere()
        {
            throw new NotImplementedException();
        }

        protected override void CalculateBoundingBox()
        {
            throw new NotImplementedException();
        }

        public static LineRenderer CreateGrid(float gridSize, int lineCount, Color zColor, Color xColor)
        {
            /// <summary>

            LineRenderer lineRenderer = new LineRenderer(lineCount * 4);

            lineRenderer.Clear();

            for (int i = 0; i < lineCount; i++)
            {
                lineRenderer.Submit(
                    Vector3.Right * gridSize * i, 
                    (Vector3.Right * gridSize * i) + Vector3.Backward * (gridSize * (lineCount - 1)), 
                    xColor);

                lineRenderer.Submit(
                    Vector3.Backward * gridSize * i, 
                    (Vector3.Backward * gridSize * i) + Vector3.Right * (gridSize * (lineCount - 1)),
                    zColor);
            }

            return lineRenderer;
        }

        public static LineRenderer CreateGrid(float gridSize, int lineCount, Color color)
        {
            return CreateGrid(gridSize, lineCount, color, color);
        }
    }
}
