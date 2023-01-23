using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli.Ecs3D.Components.Renderable.Materials
{
    /// <summary>
    /// Track which material parameters need to be recomputed during the next OnApply.
    /// </summary>
    public enum MaterialDirtyFlags
    {
        /// <summary>
        /// Change in world matrix.
        /// </summary>
        World = 1 << 0,

        /// <summary>
        /// Change in light sources, not including ambient or emissive.
        /// </summary>
        LightSources = 1 << 1,

        /// <summary>
        /// Change in material color params (can be diffuse, specular, etc. This includes specular power as well.)
        /// </summary>
        MaterialColors = 1 << 2,

        /// <summary>
        /// Change in material alpha.
        /// </summary>
        Alpha = 1 << 3,

        /// <summary>
        /// Change in texture, texture enabled, or other texture-related params.
        /// </summary>
        TextureParams = 1 << 4,

        /// <summary>
        /// Lighting params changed (enabled disabled / smooth lighting mode).
        /// </summary>
        LightingParams = 1 << 5,

        /// <summary>
        /// Change in fog-related params.
        /// </summary>
        Fog = 1 << 6,

        /// <summary>
        /// Chage in alpha-test related params.
        /// </summary>
        AlphaTest = 1 << 7,

        /// <summary>
        /// Change in skinned mesh bone transformations.
        /// </summary>
        Bones = 1 << 8,

        /// <summary>
        /// Change in ambient light color.
        /// </summary>
        AmbientLight = 1 << 9,

        /// <summary>
        /// Change in emissive light color.
        /// </summary>
        EmissiveLight = 1 << 10,

        /// <summary>
        /// All dirty flags.
        /// </summary>
        All = int.MaxValue
    }

    public abstract class Material3D
    {   
        /// <summary>
        /// BlendState used by the Batcher for the current RenderableComponent
        /// </summary>
        public BlendState BlendState = BlendState.AlphaBlend;

        /// <summary>
        /// DepthStencilState used by the Batcher for the current RenderableComponent
        /// </summary>
        public DepthStencilState DepthStencilState = DepthStencilState.None;

        /// <summary>
        /// SamplerState used by the Batcher for the current RenderableComponent
        /// </summary>
        public SamplerState SamplerState = Core.DefaultSamplerState;

        /// <summary>
        /// Effect used by the Batcher for the current RenderableComponent
        /// </summary>
        public abstract Effect Effect { get; }


        /// <summary>
        /// Current world transformations.
        /// </summary>
        virtual public Matrix World
        {
            get { return _world; }
            set { _world = value; }
        }
        Matrix _world;


        public void Apply(Matrix world, Matrix view, Matrix projection, BoundingSphere boundingSphere)
        {
            
            ApplyLights(LightManager.GetLights(world, boundingSphere).ToArray());
            MaterialSpecificApply(world, view, projection);
        }

        protected virtual void ApplyLights(LightSource[] lights){}
        protected abstract void MaterialSpecificApply(Matrix world, Matrix view, Matrix projection);
    }
}
