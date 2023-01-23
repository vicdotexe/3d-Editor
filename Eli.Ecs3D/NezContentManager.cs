using System;
using System.Collections.Generic;
using System.Text;
using Eli.Ecs3D.Components.Renderable.Materials;
using Microsoft.Xna.Framework.Graphics;

namespace Eli
{
    public static class NezContentManagerExt
    {
        public static Model LoadModel(this NezContentManager contentManager, string path)
        {
            var model = contentManager.Load<Model>(path);
            foreach (var mesh in model.Meshes)
            {
                foreach (var effect in mesh.Effects)
                {
                    LitMaterial bmat = new LitMaterial(effect as BasicEffect);
                    //LitMaterial bmat = new LitMaterial(meshPart.Effect as BasicEffect);
                    effect.Tag = bmat;
                }
                /*
                foreach (var meshPart in mesh.MeshParts)
                {
                    //meshPart.Tag = new BasicMaterial((BasicEffect) meshPart.Effect, true);
                    //BasicMaterial bmat = new BasicMaterial(meshPart.Effect as BasicEffect);

                        LitMaterial bmat = new LitMaterial(meshPart.Effect as BasicEffect);
                    //LitMaterial bmat = new LitMaterial(meshPart.Effect as BasicEffect);
                    meshPart.Effect.Tag = bmat;
                }
                */

            }

            return model;
        }
        /// <summary>
        /// Loads a simple model from eli default content.
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public static Model LoadBasicModel(this NezContentManager contentManager, BasicModel modelType)
        {
            return contentManager.LoadModel("Eli/Models/" + modelType);
        }
    }

    public static class MaterialHelpers
    {

        public static Material3D GetMaterial(this ModelMeshPart meshPart)
        {
            return meshPart.Tag as Material3D;
        }
    }
}
