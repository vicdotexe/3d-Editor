using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Eli.Ecs3D.Components.Renderable.Materials
{
    /// <summary>
    /// Create some extensions to built-in objects.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Get all materials of a mesh.
        /// </summary>
        static public MaterialAPI[] GetMaterials(this ModelMesh mesh)
        {
            FastList<MaterialAPI> ret = new FastList<MaterialAPI>();
            foreach (Effect effect in mesh.Effects)
            {
                ret.Add(effect.Tag as MaterialAPI);
            }
            return ret.Buffer;
        }

        /// <summary>
        /// Get a material from a mesh effect.
        /// Note: this will only work on an effect that are loaded as part of a model.
        /// </summary>
        static public MaterialAPI GetMaterial(this Effect effect)
        {
            return effect.Tag as MaterialAPI;
        }

        /// <summary>
        /// Get a material from a mesh part.
        /// </summary>
        static public MaterialAPI GetMaterial(this ModelMeshPart meshpart)
        {
            return meshpart.Effect.GetMaterial();
        }
    }
}
