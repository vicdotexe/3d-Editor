using System;
using System.Collections.Generic;
using System.Text;
using Eli.ECS;
using Eli.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli.Ecs3D.Components.Renderable
{
    public struct ConstructionData
    {
        public int Width;
        public int Height;
        public Vector2 TopLeft;
        public Vector2 TopRight;
        public Vector2 BottomLeft;
        public Vector2 BottomRight;

        public ConstructionData(int width, int height, Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight)
        {
            Width = width;
            Height = height;
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
        }
    }

    public class Texture2DComponent : RenderableComponent
    {
        public Vector3 Offset;
        public bool FaceCamera = true;
        public Vector3? LockedAxis = new Vector3?(new Vector3(0,1,0));

        private BasicEffect effect;

        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private VertexPositionColorTexture[] _vertices = new VertexPositionColorTexture[4];
        private ushort[] _indices = new ushort[6];
        private bool _needsInitialized = false;
        public string TexturePath { get; set; }
        public ConstructionData ConstructionData;
        public Texture2DComponent()
        {
            _needsInitialized = true;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            if (_needsInitialized)
            {
                var texture = Entity.World.Scene.Content.Load<Texture2D>(TexturePath);
                effect = new BasicEffect(Core.GraphicsDevice);
                effect.TextureEnabled = true;
                effect.Texture = texture;
                effect.Alpha = 1;
                Construct(ConstructionData.Width,ConstructionData.Height,ConstructionData.TopLeft,ConstructionData.TopRight,ConstructionData.BottomLeft,ConstructionData.BottomRight);
            }
        }

        public Texture2DComponent(Texture2D texture)
        {
            TexturePath = Core.Scene.Content.GetPathForLoadedAsset(texture);
            effect = new BasicEffect(Core.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.Texture = texture;
            effect.Alpha = 1;
            ConstructionData = new ConstructionData(texture.Width, texture.Height, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1));
            Construct(texture.Width,texture.Height, new Vector2(0,0),new Vector2(1,0),new Vector2(0,1),new Vector2(1,1) );
        }

        public Texture2DComponent(Sprite sprite)
        {
            TexturePath = Core.Scene.Content.GetPathForLoadedAsset(sprite.Texture2D);
            effect = new BasicEffect(Core.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.Texture = sprite.Texture2D;
            effect.Alpha = 1;

            var left = Mathf.Map01(sprite.SourceRect.Left, 0, sprite.Texture2D.Width);
            var right = Mathf.Map01(sprite.SourceRect.Right, 0, sprite.Texture2D.Width);
            var top = Mathf.Map01(sprite.SourceRect.Top, 0, sprite.Texture2D.Height);
            var bottom = Mathf.Map01(sprite.SourceRect.Bottom, 0, sprite.Texture2D.Height);

            var topLeft = new Vector2(left, top);
            var topRight = new Vector2(right, top);
            var bottomLeft = new Vector2(left, bottom);
            var bottomRight = new Vector2(right, bottom);

            ConstructionData = new ConstructionData(sprite.SourceRect.Width, sprite.SourceRect.Height, topLeft, topRight, bottomLeft, bottomRight);
            Construct(sprite.SourceRect.Width,sprite.SourceRect.Height, topLeft, topRight, bottomLeft, bottomRight);
        }


        protected void Construct(float width, float height, Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight)
        {
            var color = Color.White;
            vertexBuffer = new VertexBuffer(Core.GraphicsDevice, typeof(VertexPositionColorTexture), 4, BufferUsage.None);
            indexBuffer = new IndexBuffer(Core.GraphicsDevice, typeof(ushort), 6, BufferUsage.None);
            var halfwidth = width * 0.5f;
            var halfheight = height * 0.5f;
            //left bottom corner
            _vertices[0] = new VertexPositionColorTexture(new Vector3(-halfwidth, -halfheight, 0), color, bottomLeft);
            //left upper corner
            _vertices[1] = new VertexPositionColorTexture(new Vector3(-halfwidth, halfheight, 0), color, topLeft);
            //right upper corner
            _vertices[2] = new VertexPositionColorTexture(new Vector3(halfwidth, halfheight, 0), color, topRight);
            //lower right corner
            _vertices[3] = new VertexPositionColorTexture(new Vector3(halfwidth, -halfheight, 0), color, bottomRight);
            //Front face
            //bottom right triangle
            _indices[0] = 0;
            _indices[1] = 3;
            _indices[2] = 2;
            //top left triangle
            _indices[3] = 2;
            _indices[4] = 1;
            _indices[5] = 0;

            vertexBuffer.SetData(_vertices);
            indexBuffer.SetData(_indices);
        }

        public override void Render(ICamera camera)
        {
            CalculateBoundingBox();
            CalculateBoundingSphere();
            // decompose transformations
            Vector3 position; Quaternion rotation; Vector3 scale;
            Entity.WorldMatrix.Decompose(out scale, out rotation, out position);

            // add position offset
            position += Offset;

            // create a new world matrix for the billboard
            Matrix newWorld;

            // if facing camera, create billboard world matrix
            if (FaceCamera)
            {
                // set rotation based on camera with locked axis
                if (LockedAxis != null)
                {
                    newWorld = Matrix.CreateScale(scale) *
                               Matrix.CreateConstrainedBillboard(position, camera.TransformMatrix.Translation,
                                   LockedAxis.Value, null, null);
                }
                // set rotation based on camera without any locked axis
                else
                {
                    newWorld = Matrix.CreateScale(scale) *
                               Matrix.CreateBillboard(position, camera.TransformMatrix.Translation,
                                   Vector3.Up, null);
                }
            }
            // if not facing camera, just use world transformations
            else
            {
                newWorld = Entity.WorldMatrix;
            }

            Core.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Core.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            effect.World = newWorld;
            effect.View = camera.ViewMatrix;
            effect.Projection = camera.ProjectionMatrix;

            Core.GraphicsDevice.SetVertexBuffer(vertexBuffer);
            Core.GraphicsDevice.Indices = indexBuffer;

            effect.CurrentTechnique.Passes[0].Apply();
            var primitiveCount = _indices.Length / 3;
            Core.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
        }

        protected override void CalculateBoundingSphere()
        {
            BoundingSphere = new BoundingSphere(new Vector3(0), 5).Transform(Entity.WorldMatrix);
        }

        protected override void CalculateBoundingBox()
        {
            BoundingBox = new BoundingBox(Entity.WorldMatrix.Translation - new Vector3(5),
                Entity.WorldMatrix.Translation + new Vector3(5));
        }
    }
}
