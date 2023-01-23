using System;
using System.Collections.Generic;
using System.Text;
using Eli.ECS;
using Eli.Ecs3D.Components.Renderable.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli.Ecs3D
{
    public abstract class RenderableComponent : Component, IRenderable, IComparable<RenderableComponent>
    {
        public BoundingBox BoundingBox { get; protected set; }
 
        public BoundingSphere BoundingSphere { get; protected set; }

        
        /// <summary>
        /// standard Batcher layerdepth. 0 is in front and 1 is in back. Changing this value will trigger a sort of the renderableComponents
        /// list on the scene.
        /// </summary>
        [Range(0, 1)]
        public float LayerDepth
        {
            get => _layerDepth;
            set => SetLayerDepth(value);
        }

        private float _layerDepth;
        /// <summary>
        /// lower renderLayers are in the front and higher are in the back, just like layerDepth but not clamped to 0-1. Note that this means
        /// higher renderLayers are sent to the Batcher first. An important fact when using the stencil buffer.
        /// </summary>
        /// <value>The render layer.</value>
        public int RenderLayer
        {
            get => _renderLayer;
            set => SetRenderLayer(value);
        }

        private int _renderLayer;

        public  Material Material { get; set; } 
        public Effect Effect { get; set; }
        public bool IsVisibleFromCamera(Camera3D camera)
        {
            throw new NotImplementedException();
        }

        public virtual void Render(ICamera camera)
        {

        }

        protected abstract void CalculateBoundingSphere();
        protected abstract void CalculateBoundingBox();
        #region FluentSetters

        /// <summary>
        /// standard Batcher layerdepth. 0 is in front and 1 is in back. Changing this value will trigger a sort of the renderableComponents
        /// </summary>
        /// <returns>The layer depth.</returns>
        /// <param name="layerDepth">Value.</param>
        public RenderableComponent SetLayerDepth(float layerDepth)
        {
            _layerDepth = Mathf.Clamp01(layerDepth);

            if (Entity != null && Entity.World != null)
                Entity.World.RenderableComponents.SetRenderLayerNeedsComponentSort(RenderLayer);
            return this;
        }

        /// <summary>
        /// lower renderLayers are in the front and higher are in the back, just like layerDepth but not clamped to 0-1. Note that this means
        /// higher renderLayers are sent to the Batcher first. An important fact when using the stencil buffer.
        /// </summary>
        /// <returns>The render layer.</returns>
        /// <param name="renderLayer">Render layer.</param>
        public RenderableComponent SetRenderLayer(int renderLayer)
        {
            if (renderLayer != _renderLayer)
            {
                var oldRenderLayer = _renderLayer;
                _renderLayer = renderLayer;

                // if we have an entity then we are being managed by a ComponentList so we need to let it know that we changed renderLayers
                if (Entity != null && Entity.World != null)
                    Entity.World.RenderableComponents.UpdateRenderableRenderLayer(this, oldRenderLayer, _renderLayer);
            }

            return this;
        }
        #endregion

        /// <Docs>To be added.</Docs>
        /// <para>Returns the sort order of the current instance compared to the specified object.</para>
        /// <summary>
        /// sorted first by renderLayer, then layerDepth and finally material
        /// </summary>
        /// <returns>The to.</returns>
        /// <param name="other">Other.</param>
        public int CompareTo(RenderableComponent other)
        {
            var res = other.RenderLayer.CompareTo(RenderLayer);
            if (res == 0)
            {
                res = other.LayerDepth.CompareTo(LayerDepth);
                if (res == 0)
                {
                    // both null or equal
                    if (ReferenceEquals(Material, other.Material))
                        return 0;

                    if (other.Material == null)
                        return -1;

                    return 1;
                }
            }

            return res;
        }
    }
}
