using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using Eli.ECS;
using Eli.Ecs3D.Components.Renderable.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli.Ecs3D
{
    public class ModelData
    {
        public List<Vector3[]> meshPartVertices = new List<Vector3[]>();

        public void SetVertexData(Model model, Vector3 localOffset)
        {
            meshPartVertices = new List<Vector3[]>();
            foreach (var mesh in model.Meshes)
            foreach (var meshPart in mesh.MeshParts)
            {
                VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[meshPart.VertexBuffer.VertexCount];
                meshPart.VertexBuffer.GetData<VertexPositionNormalTexture>(vertices, meshPart.StartIndex,10);
                var verts = new Vector3[vertices.Length];
                var index = 0;
                foreach (var vpnt in vertices)
                {
                    verts[index] = Vector3.Transform(vpnt.Position,mesh.ParentBone.Transform * Matrix.CreateTranslation(-localOffset));
                    index++;
                }
                meshPartVertices.Add(verts);
            }
        }
    }
    public class ModelComponent: RenderableComponent
    {

        public Model Model
        {
            get
            {
                return _model;
            }
            set
            {
                if (value == _model || value == null)
                    return;
                SetModel(value);
                ModelData.SetVertexData(_model, _localOffset);
            }
        }

        public string ModelPath { get; set; }
        private Model _model;
        private Vector3 _localOffset;
        private Dictionary<string, Material3D> _materials = new Dictionary<string, Material3D>();
        public ModelData ModelData { get; } = new ModelData();
        private Material3D GetMaterial(string meshName, int meshPartIndex = 0)
        {
            if (_materials.TryGetValue(meshName, out var material))
            {
                return material;
            }

            return Model.Meshes[meshName].MeshParts[meshPartIndex].Effect.Tag as Material3D;
        }

        private Material3D GetMaterial(ModelMeshPart meshPart)
        {
            return meshPart.Effect.Tag as Material3D;
        }
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            if (Model == null)
            {
                if (ModelPath == null)
                    Model = Entity.World.Scene.Content.LoadBasicModel(BasicModel.Cube);
                else
                    Model = Entity.World.Scene.Content.LoadModel(ModelPath);
            }


        }

        public override void OnEntityTransformChanged()
        {
            base.OnEntityTransformChanged(); 
            CalculateBoundingBox();
            CalculateBoundingSphere();
        }

        public void SetModel(Model model)
        {
            _model = model;
            _localOffset = GetLocalOffset(model);
            if (Entity != null)
            {
                CalculateBoundingBox();
                CalculateBoundingSphere();
            }
            if (Entity != null)
            {
                ModelPath = Entity.World.Scene.Content.GetPathForLoadedAsset(_model);
            }
            else if (Core.Scene != null)
            {

                ModelPath = Core.Scene.Content.GetPathForLoadedAsset(_model);
            }
            else
            {
                ModelPath = Core.Content.GetPathForLoadedAsset(_model);
            }

            UpdateMaterials();
        }

        private void UpdateMaterials()
        {

        }

        private Vector3 GetLocalOffset(Model model)
        {
            return BoundsHelper.CreateBoundingBox(Model, Matrix.Identity).Center();
        }


        public override void Render(ICamera camera)
        {

            basicWorkingRender(camera);
        }

        private void basicWorkingRender(ICamera camera)
        {
            Core.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Core.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Core.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            //Core.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;

            var world = Matrix.CreateTranslation(-_localOffset) *Entity.WorldMatrix;
            var view = Entity.World.Scene.Camera.ViewMatrix;
            var projection = Entity.World.Scene.Camera.ProjectionMatrix;

            foreach (ModelMesh mesh in Model.Meshes)
            {

                var index = 0;
                foreach (var meshpart in mesh.MeshParts)
                {
                    
                   // var material = GetMaterial(mesh.Name, index);
                   var material = GetMaterial(meshpart);
                    material.Apply(mesh.ParentBone.Transform * world, view, projection, BoundingSphere);
                    meshpart.Tag = meshpart.Effect;
                    meshpart.Effect = material.Effect;
                    //var ef = (meshpart.Effect as BasicEffect);
                    //ef.World = mesh.ParentBone.Transform * world;
                    //ef.View = view;
                    //ef.Projection = projection;
                    //ef.Alpha = 1;
                    //ef.EnableDefaultLighting();
                    index++;

                }

                mesh.Draw();

                foreach (var meshpart in mesh.MeshParts)
                {
                    meshpart.Effect = meshpart.Tag as Effect;
                    meshpart.Tag = null;
                }
            }

        }

        #region Bounds
        private void DrawBoundingSphere(Matrix view, Matrix projection)
        {
            //don't fuck with this, it just draws the sphere
            bool hit = Entity.Hovering;
            bool selected = Entity.Selected;
            Graphics.DrawWireSphere(Matrix.CreateScale(BoundingSphere.Radius * 2) * Matrix.CreateTranslation(BoundingSphere.Center), view, projection, selected ? Color.Gold : hit ? Color.Pink : Color.Transparent);
        }

        protected override void CalculateBoundingSphere()
        {
            BoundingSphere = BoundsHelper.GetBoundingSphere(Model);
            BoundingSphere = BoundingSphere.Transform(Entity.WorldMatrix);
        }

        protected override void CalculateBoundingBox()
        {
            BoundingBox = BoundsHelper.CreateBoundingBox(Model,Matrix.CreateTranslation(-_localOffset) * Entity.WorldMatrix);
        }
        #endregion

    }

    

}
