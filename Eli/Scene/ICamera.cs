using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Eli.ECS
{
    public partial class Camera
    {
        public enum Dimensions
        {
            Two,
            Three
        }
    }
    public interface ICamera
    {
        Camera.Dimensions Type { get; }
        Matrix TransformMatrix { get;}
        Matrix InverseTransformMatrix { get; }
        Matrix ViewMatrix { get;  }
        Matrix ProjectionMatrix { get; }
        void OnSceneRenderTargetSizeChanged(int width, int height);
        void ForceMatrixUpdate();
    }
}
