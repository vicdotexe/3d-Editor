using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Eli.Textures;
using Eli.BitmapFonts;


namespace Eli
{
	/// <summary>
	/// wrapper class that holds in instance of a Batcher and helpers so that it can be passed around and draw anything.
	/// </summary>
	public class Graphics
    {

        private static SpherePrimitive _sphere;
        private static RasterizerState _prevRasterizerState;
		private static RasterizerState _wireState = new RasterizerState(){FillMode = FillMode.WireFrame};
        public static void DrawSphere(Matrix world, Matrix view, Matrix projection, Color color)
        {
            _sphere.Draw(world, view, projection, color);
        }

        public static void DrawWireSphere(Matrix world, Matrix view, Matrix projection, Color color)
        {
            _prevRasterizerState = Core.GraphicsDevice.RasterizerState;
            Core.GraphicsDevice.RasterizerState = _wireState;
            DrawSphere(world, view, projection, color);
            Core.GraphicsDevice.RasterizerState = _prevRasterizerState;
        }

		public static Graphics Instance;

		/// <summary>
		/// All 2D rendering is done through this Batcher instance
		/// </summary>
		public Batcher Batcher;

		/// <summary>
		/// default font is loaded up and stored here for easy access. Nez uses it for the DebugConsole
		/// </summary>
		public BitmapFont BitmapFont;

		/// <summary>
		/// A sprite used to draw rectangles, lines, circles, etc. 
		/// Will be generated at startup, but you can replace this with a sprite from your atlas to reduce texture swaps.
		/// Should be a 1x1 white pixel
		/// </summary>
		public Sprite PixelTexture;


		public Graphics()
		{
		}


		public Graphics(BitmapFont font)
		{
			Batcher = new Batcher(Core.GraphicsDevice);
            _sphere = new SpherePrimitive(Core.GraphicsDevice);
			BitmapFont = font;

			// the bottom/right pixel is white on the default font so we'll use that for the pixelTexture
			var fontTex =
				BitmapFont.Textures[
					BitmapFont.DefaultCharacter.TexturePage]; // bitmapFont.defaultCharacterRegion.sprite.texture2D;
			PixelTexture = new Sprite(fontTex, fontTex.Width - 1, fontTex.Height - 1, 1, 1);
		}


		/// <summary>
		/// helper method that generates a single color texture of the given dimensions
		/// </summary>
		/// <returns>The single color texture.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="color">Color.</param>
		public static Texture2D CreateSingleColorTexture(int width, int height, Color color)
		{
			var texture = new Texture2D(Core.GraphicsDevice, width, height);
			var data = new Color[width * height];
			for (var i = 0; i < data.Length; i++)
				data[i] = color;

			texture.SetData<Color>(data);
			return texture;
		}


		public void Unload()
		{
			if (PixelTexture != null)
				PixelTexture.Texture2D.Dispose();
			PixelTexture = null;

			Batcher.Dispose();
			Batcher = null;
		}
	}

}