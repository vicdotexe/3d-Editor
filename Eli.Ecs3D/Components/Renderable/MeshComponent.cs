#define DEACTIVE
using System;
using System.Collections.Generic;
using System.Text;
using Eli.ECS;
using Eli.Ecs3D;
using Eli.Ecs3D.Components.Renderable.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#if !DEACTIVE
public class MeshComponent : RenderableComponent
{
    private Model _model;

    private Vector3 _modelOffset;

    public override void Render(ICamera camera)
    {
        basicWorkingRender(camera);
    }

    protected override void CalculateBoundingSphere()
    {
        throw new NotImplementedException();
    }

    protected override void CalculateBoundingBox()
    {
        throw new NotImplementedException();
    }

    private void basicWorkingRender(ICamera camera)
    {

    }
}



namespace Eli.Ecs3D.Components.Renderable
{
    public class MeshComponent : RenderableComponent
    {
        public ModelMesh Mesh { get; set; }
        public MeshComponent(ModelMesh mesh)
        {
            Mesh = mesh;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();


        }

        public override void Render(ICamera camera)
        {
            base.Render(camera);


            Material.World = Entity.WorldMatrix * Mesh.ParentBone.ModelTransform;


            foreach (BasicEffect effect in Mesh.Effects)
            {
                effect.EnableDefaultLighting();
                effect.LightingEnabled = true;
                effect.View = camera.ViewMatrix;
                effect.Projection = camera.ProjectionMatrix;
                effect.World = Matrix.CreateRotationX(Mathf.Deg2Rad * 180);
                effect.TextureEnabled = true;
                effect.Alpha = 1;
                //effect.DirectionalLight0.Enabled = false;
                //effect.DirectionalLight1.Enabled = false;
                //effect.DirectionalLight2.Enabled = false;
                //effect.EnableDefaultLighting();
                //_textures.AddIfNotPresent(effect.Texture);
            }

            Mesh.Draw();
            
            CalculateBoundingSphere();
            Graphics.DrawWireSphere(Matrix.CreateScale(_lastBoundingSphere.Radius/2), Material.View, Material.Projection, Entity.Hovering ? Color.Gold : Color.Gray);
            /*
            Material.Alpha = 1;
            Material.TextureEnabled = true;
            foreach (var meshPart in Mesh.MeshParts)
            {
                meshPart.Tag = meshPart.Effect;
                meshPart.Effect = Material.Effect;
            }

            Mesh.Draw();
            foreach (var meshPart in Mesh.MeshParts)
            {
                meshPart.Effect = meshPart.Tag as Effect;
                meshPart.Tag = null;
            }
            */
        }

        private BoundingSphere _lastBoundingSphere;
        private float _lastScale;

        protected override void CalculateBoundingSphere()
        {
            _lastBoundingSphere = Mesh.BoundingSphere;
            BoundingSphere = _lastBoundingSphere;
        }

        protected override void CalculateBoundingBox()
        {
            throw new NotImplementedException();
        }
    }

}
#endif