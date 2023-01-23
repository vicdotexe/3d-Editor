using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli.Ecs3D.Components.Renderable
{
    public class BasicModelComponent : ModelComponent
    {
        public Color Color;

        public BasicModelComponent(BasicModel modelType, Color color)
        {
            Color = color;
        }

    }
}
