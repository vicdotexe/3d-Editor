using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli.Ecs3D.Components.Renderable
{
    public abstract class BaseMaterial
    {
        public abstract Effect Effect { get; }

        public void SetWorldMatrix(Matrix world)
        {

        }
    }
}
