using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Eli.ECS;
using Eli.Textures;
using Microsoft.Xna.Framework;

namespace Eli
{
    /// <summary>
    /// Renderers are added to a Scene and handle all of the actual calls to RenderableComponent.render and Entity.debugRender.
    /// A simple Renderer could just start the Batcher.instanceGraphics.batcher or it could create its own local Batcher instance
    /// if it needs it for some kind of custom rendering.
    ///
    /// Note that it is a best practice to ensure all Renderers that render to a RenderTarget have lower renderOrders to avoid issues
    /// with clearing the back buffer (http://gamedev.stackexchange.com/questions/90396/monogame-setrendertarget-is-wiping-the-backbuffer).
    /// Giving them a negative renderOrder is a good strategy to deal with this.
    /// </summary>
    public abstract class BaseRenderer : IComparable<BaseRenderer>
    {
        public ICamera Camera;
        /// <summary>
        /// specifies the order in which the Renderers will be called by the scene
        /// </summary>
        public readonly int RenderOrder = 0;

        /// <summary>
        /// if renderTarget is not null this renderer will render into the RenderTarget instead of to the screen
        /// </summary>
        public RenderTexture RenderTexture;

        /// <summary>
        /// if renderTarget is not null this Color will be used to clear the screen
        /// </summary>
        public Color RenderTargetClearColor = Color.Transparent;

        /// <summary>
        /// flag for this renderer that decides if it should debug render or not. The render method receives a bool (debugRenderEnabled)
        /// letting the renderer know if the global debug rendering is on/off. The renderer then uses the local bool to decide if it
        /// should debug render or not.
        /// </summary>
        public bool ShouldDebugRender = true;

        /// <summary>
        /// if true, the Scene will call SetRenderTarget with the scene RenderTarget. The default implementaiton returns true if the Renderer
        /// has a renderTexture
        /// </summary>
        /// <value><c>true</c> if wants to render to scene render target; otherwise, <c>false</c>.</value>
        public virtual bool WantsToRenderToSceneRenderTarget => RenderTexture == null;

        /// <summary>
        /// if true, the Scene will call the render method AFTER all PostProcessors have finished. This must be set to true BEFORE calling
        /// Scene.addRenderer to take effect and the Renderer should NOT have a renderTexture. The main reason for this type of Renderer
        /// is so that you can render your UI without post processing on top of the rest of your Scene. The ScreenSpaceRenderer is an
        /// example Renderer that sets this to true;
        /// </summary>
        public bool WantsToRenderAfterPostProcessors;

        protected BaseRenderer(int renderOrder)
        {
            RenderOrder = renderOrder;
        }



        /// <summary>
        /// called when a scene is ended or this Renderer is removed from the Scene. use this for cleanup.
        /// </summary>
        public virtual void Unload() => RenderTexture?.Dispose();

        internal abstract void onAddedToScene(IScene scene);

        internal abstract void render(IScene scene);
        
        /// <summary>
        /// called when the default scene RenderTarget is resized and when adding a Renderer if the scene has already began. default implementation
        /// calls through to RenderTexture.onSceneBackBufferSizeChanged
        /// so that it can size itself appropriately if necessary.
        /// </summary>
        /// <param name="newWidth">New width.</param>
        /// <param name="newHeight">New height.</param>
        public virtual void OnSceneBackBufferSizeChanged(int newWidth, int newHeight) => RenderTexture?.OnSceneBackBufferSizeChanged(newWidth, newHeight);

        public int CompareTo(BaseRenderer other) => RenderOrder.CompareTo(other.RenderOrder);
    }

    public abstract class Renderer<TU> : BaseRenderer where TU : IScene
    {
        protected Renderer(int renderOrder) : base(renderOrder)
        {
        }

        /// <summary>
        /// called when the Renderer is added to the Scene
        /// </summary>
        /// <param name="scene">Scene.</param>
        internal override void onAddedToScene(IScene scene)
        {
           OnAddedToScene((TU)scene);
        }

        public virtual void OnAddedToScene(TU scene)
        {

        }

        internal override void render(IScene scene)
        {
            Render((TU)scene);
        }

        public virtual void Render(TU scene)
        {

        }
    }
}
