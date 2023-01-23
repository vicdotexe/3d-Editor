using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli
{
    public static class BoundsHelper
    {
        private static Vector3[] GetVertexElement(ModelMeshPart meshPart, VertexElementUsage usage)
        {
            VertexDeclaration vd = meshPart.VertexBuffer.VertexDeclaration;
            VertexElement[] elements = vd.GetVertexElements();

            Func<VertexElement, bool> elementPredicate = ve => ve.VertexElementUsage == usage && ve.VertexElementFormat == VertexElementFormat.Vector3;
            if (!elements.Any(elementPredicate))
                return null;

            VertexElement element = elements.First(elementPredicate);

            Vector3[] vertexData = new Vector3[meshPart.NumVertices];
            meshPart.VertexBuffer.GetData((meshPart.VertexOffset * vd.VertexStride) + element.Offset,
                vertexData, 0, vertexData.Length, vd.VertexStride);

            return vertexData;
        }

        public static BoundingBox? GetBoundingBox(ModelMeshPart meshPart, Matrix transform)
        {
            if (meshPart.VertexBuffer == null)
                return null;

            Vector3[] positions = BoundsHelper.GetVertexElement(meshPart, VertexElementUsage.Position);
            if (positions == null)
                return null;

            Vector3[] transformedPositions = new Vector3[positions.Length];
            Vector3.Transform(positions, ref transform, transformedPositions);

            return BoundingBox.CreateFromPoints(transformedPositions);
        }

        public static BoundingBox CreateBoundingBox(Model model)
        {
            return CreateBoundingBox(model, Matrix.Identity);
        }

        public static BoundingBox CreateBoundingBox(Model model, Matrix transform)
        {
            Matrix[] boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            BoundingBox? result = null;
            foreach (ModelMesh mesh in model.Meshes)
            foreach (ModelMeshPart meshPart in mesh.MeshParts)
            {
                BoundingBox? meshPartBoundingBox = GetBoundingBox(meshPart, boneTransforms[mesh.ParentBone.Index] * transform);
                if (meshPartBoundingBox != null)
                {
                    if (result == null)
                        result = meshPartBoundingBox;
                    else
                        result = BoundingBox.CreateMerged(result.Value, meshPartBoundingBox.Value);
                }
            }
            return result.Value;
        }

        static Dictionary<Model, BoundingSphere> _calculatedBoundingSpheres = new Dictionary<Model, BoundingSphere>();

        /// <summary>
        /// Return bounding sphere for a model instance (calculate if needed, else return from cache).
        /// </summary>
        /// <param name="model">Model to get bounding sphere for.</param>
        /// <returns>BoundingSphere instance, in local space.</returns>
        public static BoundingSphere GetBoundingSphere(Model model)
        {
            // try to get value from cache, and if got bounding box in cache return it.
            BoundingSphere ret;
            if (_calculatedBoundingSpheres.TryGetValue(model, out ret))
            {
                return ret;
            }


            // got here? it means we need to calculate bounding sphere.
            ret = new BoundingSphere();
            foreach (var mesh in model.Meshes)
            {
                var sphere = mesh.BoundingSphere.Transform(mesh.ParentBone.Transform);
                //var sphere = mesh.BoundingSphere;
                ret = BoundingSphere.CreateMerged(ret, sphere);
            }

            // add to cache and return
            _calculatedBoundingSpheres[model] = ret;
            return ret;
        }

        public static Vector3 Center(this BoundingBox box)
        {
            return box.Min + box.Extents();
        }

        public static Vector3 Extents(this BoundingBox box)
        {
            return (box.Max - box.Min) * 0.5f;
        }
    }

    public class BoundingBoxBuffers
    {
        public VertexBuffer Vertices;
        public int VertexCount;
        public IndexBuffer Indices;
        public int PrimitiveCount;

        protected static BasicEffect effect;

        public void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
        {
            if (effect == null)
                effect = Core.Content.LoadMonoGameEffect<BasicEffect>();

            graphicsDevice.SetVertexBuffer(Vertices);
            graphicsDevice.Indices = Indices;

            effect.World = Matrix.Identity;
            effect.View = view;
            effect.Projection = projection;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0,
                    VertexCount, 0, PrimitiveCount);
            }
        }

		public static BoundingBoxBuffers CreateBoundingBoxBuffers(BoundingBox boundingBox, GraphicsDevice graphicsDevice)
		{
			BoundingBoxBuffers boundingBoxBuffers = new BoundingBoxBuffers();

			boundingBoxBuffers.PrimitiveCount = 24;
			boundingBoxBuffers.VertexCount = 48;

			VertexBuffer vertexBuffer = new VertexBuffer(graphicsDevice,
				typeof(VertexPositionColor), boundingBoxBuffers.VertexCount,
				BufferUsage.WriteOnly);
			List<VertexPositionColor> vertices = new List<VertexPositionColor>();

			const float ratio = 5.0f;

			Vector3 xOffset = new Vector3((boundingBox.Max.X - boundingBox.Min.X) / ratio, 0, 0);
			Vector3 yOffset = new Vector3(0, (boundingBox.Max.Y - boundingBox.Min.Y) / ratio, 0);
			Vector3 zOffset = new Vector3(0, 0, (boundingBox.Max.Z - boundingBox.Min.Z) / ratio);
			Vector3[] corners = boundingBox.GetCorners();

			// Corner 1.
			AddVertex(vertices, corners[0]);
			AddVertex(vertices, corners[0] + xOffset);
			AddVertex(vertices, corners[0]);
			AddVertex(vertices, corners[0] - yOffset);
			AddVertex(vertices, corners[0]);
			AddVertex(vertices, corners[0] - zOffset);

			// Corner 2.
			AddVertex(vertices, corners[1]);
			AddVertex(vertices, corners[1] - xOffset);
			AddVertex(vertices, corners[1]);
			AddVertex(vertices, corners[1] - yOffset);
			AddVertex(vertices, corners[1]);
			AddVertex(vertices, corners[1] - zOffset);

			// Corner 3.
			AddVertex(vertices, corners[2]);
			AddVertex(vertices, corners[2] - xOffset);
			AddVertex(vertices, corners[2]);
			AddVertex(vertices, corners[2] + yOffset);
			AddVertex(vertices, corners[2]);
			AddVertex(vertices, corners[2] - zOffset);

			// Corner 4.
			AddVertex(vertices, corners[3]);
			AddVertex(vertices, corners[3] + xOffset);
			AddVertex(vertices, corners[3]);
			AddVertex(vertices, corners[3] + yOffset);
			AddVertex(vertices, corners[3]);
			AddVertex(vertices, corners[3] - zOffset);

			// Corner 5.
			AddVertex(vertices, corners[4]);
			AddVertex(vertices, corners[4] + xOffset);
			AddVertex(vertices, corners[4]);
			AddVertex(vertices, corners[4] - yOffset);
			AddVertex(vertices, corners[4]);
			AddVertex(vertices, corners[4] + zOffset);

			// Corner 6.
			AddVertex(vertices, corners[5]);
			AddVertex(vertices, corners[5] - xOffset);
			AddVertex(vertices, corners[5]);
			AddVertex(vertices, corners[5] - yOffset);
			AddVertex(vertices, corners[5]);
			AddVertex(vertices, corners[5] + zOffset);

			// Corner 7.
			AddVertex(vertices, corners[6]);
			AddVertex(vertices, corners[6] - xOffset);
			AddVertex(vertices, corners[6]);
			AddVertex(vertices, corners[6] + yOffset);
			AddVertex(vertices, corners[6]);
			AddVertex(vertices, corners[6] + zOffset);

			// Corner 8.
			AddVertex(vertices, corners[7]);
			AddVertex(vertices, corners[7] + xOffset);
			AddVertex(vertices, corners[7]);
			AddVertex(vertices, corners[7] + yOffset);
			AddVertex(vertices, corners[7]);
			AddVertex(vertices, corners[7] + zOffset);

			vertexBuffer.SetData(vertices.ToArray());
			boundingBoxBuffers.Vertices = vertexBuffer;

			IndexBuffer indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, boundingBoxBuffers.VertexCount,
				BufferUsage.WriteOnly);
			indexBuffer.SetData(Enumerable.Range(0, boundingBoxBuffers.VertexCount).Select(i => (short)i).ToArray());
			boundingBoxBuffers.Indices = indexBuffer;

			return boundingBoxBuffers;
		}

		private static void AddVertex(List<VertexPositionColor> vertices, Vector3 position)
		{
			vertices.Add(new VertexPositionColor(position, Color.White));
		}

        // cache of bounding spheres we calculated for loaded models.

    }
}

