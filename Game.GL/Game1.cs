using Eli;
using Eli.Ecs3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game.GL
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Core
    {
        public Game1() : base(1920, 1080)
        {

        }

        protected override void Initialize()
        {
            base.Initialize();
            Core.DefaultSamplerState = SamplerState.AnisotropicClamp;
            Scene = new Scene();
        }
    }
}
