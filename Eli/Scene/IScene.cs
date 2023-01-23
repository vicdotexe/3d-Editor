using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Eli.ECS
{
    /*
    public abstract class Camera : Component, ICamera
    {
        public abstract Matrix TransformMatrix { get; }
        public abstract Matrix InverseTransformMatrix { get;  }
        public abstract Matrix ViewMatrix { get;  }
        public abstract Matrix ProjectionMatrix { get; }
        public abstract void OnSceneRenderTargetSizeChanged(int width, int height);

        public abstract void ForceMatrixUpdate();
    }
    */
    public static class SceneExt
    {
        public static T As<T>(this IScene scene) where T : IScene
        {
            return (T) scene;
        }
    }
    public interface IScene
    {
        /// <summary>
        /// The Active Camera in this scene.
        /// </summary>
        ICamera Camera { get; set; }

        /// <summary>
        /// The size you want to design your scene in.
        /// </summary>
        Point SceneRenderTargetSize { get;}

        /// <summary>
        /// The rectangle you want to render your final target at.
        /// </summary>
        Rectangle RenderDestination { get; }

        /// <summary>
        /// Basically the backbuffer of the scene.
        /// </summary>
        RenderTarget2D SceneRenderTarget { get; }

        /// <summary>
        /// Local content manager for scene specific assets.
        /// </summary>
        NezContentManager Content { get; }

        void Begin();
        void End();
        void Update();
        void Render(); 

        /// <summary>
        /// Called after Render, and returns the final rendertarget of the scene
        /// to Core for handling, which will either render it to the screen at
        /// at the final render destination, or pass it to a final render delegate to process.
        /// </summary>
        /// <returns>The final </returns>
        RenderTarget2D PostRender();
    }
}
