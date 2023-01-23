using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eli.Ecs3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli
{
	public abstract class Primitive3DBase<T> where T : struct, Microsoft.Xna.Framework.Graphics.IVertexType
	{
		public int NumberOfVertices;
		private VertexDeclaration _declaration;

		protected T[] _vertices;
        protected ushort[] _indices;

		protected VertexBuffer _vertexBuffer;
		protected IndexBuffer _indexBuffer;

        public BlendState BlendState = BlendState.AlphaBlend;
        public RasterizerState RasterizerState = RasterizerState.CullCounterClockwise;
		public DepthStencilState DepthStencilState = DepthStencilState.Default;

		public Vector3 Position
		{
			get { return _position; }
			set
			{
				_position = value;
				_isDirty = true;
			}
		}
		private Vector3 _position = Vector3.Zero;

		public Vector3 Size
		{
			get { return _size; }
			set
			{
				_size = value;
				_isDirty = true;
			}
		}

		private Vector3 _size;

		public BasicEffect Effect;
		protected bool _isDirty = true;

		public Color DiffuseColor = Microsoft.Xna.Framework.Color.White;

		public Primitive3DBase(int vertCount, int indexCount, BasicEffect effect = null)
        {
            _indices = new ushort[indexCount];
			_vertices = new T[vertCount];
			NumberOfVertices = vertCount;
			var test = new T();
			_declaration = ((IVertexType)test).VertexDeclaration;
            if (effect == null)
                LoadEffect();
            else
                Effect = effect;

			Construct();
			InitializePrimitive();
			InitializeEffect();
		}

		public virtual void LoadEffect()
		{
			Effect = Core.Content.LoadMonoGameEffect<BasicEffect>();
		}

		public abstract void Construct();

		protected void InitializePrimitive()
		{
			if (_vertexBuffer != null)
				_vertexBuffer.Dispose();

			if (_indexBuffer != null)
				_indexBuffer.Dispose();

			// create a vertex buffer, and copy our vertex data into it.
			_vertexBuffer = new VertexBuffer(Core.GraphicsDevice, typeof(T), _vertices.Length,
				BufferUsage.None);
			_vertexBuffer.SetData(_vertices.ToArray());

			// create an index buffer, and copy our index data into it.
			_indexBuffer = new IndexBuffer(Core.GraphicsDevice, typeof(ushort), _indices.Length, BufferUsage.None);
			_indexBuffer.SetData(_indices.ToArray());
		}

		protected abstract void InitializeEffect();

        protected virtual void BeginDraw(Camera3D camera, GraphicsDevice graphicsDevice)
        {
            Core.GraphicsDevice.RasterizerState = RasterizerState;
            graphicsDevice.BlendState = BlendState;
            graphicsDevice.DepthStencilState = DepthStencilState;
        }

		public virtual void Draw(Camera3D camera, GraphicsDevice graphicsDevice)
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
			var primitiveCount = _indices.Length / 3;
			graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
		}

	}
}
