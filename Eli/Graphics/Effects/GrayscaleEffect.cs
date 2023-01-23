using Microsoft.Xna.Framework.Graphics;


namespace Eli
{
	public class GrayscaleEffect : Effect
	{
		public GrayscaleEffect() : base(Core.GraphicsDevice, EffectResource.GrayscaleBytes)
		{
		}
	}
}