using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eli.ECS;
using Eli.Ecs3D.Terrain;
using Eli.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli.Ecs3D.Components
{
    public class ShaderTestComponent : RenderableComponent
    {
        private TerrainEffect _effect;
        private Model _model;
        public ShaderTestComponent()
        {
            _effect = new TerrainEffect();
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            _model = Entity.World.Scene.Content.LoadBasicModel(BasicModel.Cube);
        }

        public override void Render(ICamera camera)
        {
            base.Render(camera);
            //_effect.World = camera.ViewMatrix;
            //_effect.World = Matrix.Identity;
            _effect.WorldViewProj = camera.ViewMatrix * camera.ProjectionMatrix;
            foreach (var mesh in _model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = _effect;
                }
            }
            foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                foreach (ModelMesh mesh in _model.Meshes)
                {
                    mesh.Draw();
                }
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
    }
}
