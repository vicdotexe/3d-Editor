using System;
using System.IO;
using Microsoft.Xna.Framework;


namespace Eli
{
	public static class EffectResource
	{
		// sprite effects
		internal static byte[] SpriteBlinkEffectBytes => GetFileResourceBytes("Content/Eli/Effects/SpriteBlinkEffect.mgfxo");

		internal static byte[] SpriteLinesEffectBytes => GetFileResourceBytes("Content/Eli/Effects/SpriteLines.mgfxo");

		internal static byte[] SpriteAlphaTestBytes => GetFileResourceBytes("Content/Eli/Effects/SpriteAlphaTest.mgfxo");

		internal static byte[] CrosshatchBytes => GetFileResourceBytes("Content/Eli/Effects/Crosshatch.mgfxo");

		internal static byte[] NoiseBytes => GetFileResourceBytes("Content/Eli/Effects/Noise.mgfxo");

		internal static byte[] TwistBytes => GetFileResourceBytes("Content/Eli/Effects/Twist.mgfxo");

		internal static byte[] DotsBytes => GetFileResourceBytes("Content/Eli/Effects/Dots.mgfxo");

		internal static byte[] DissolveBytes => GetFileResourceBytes("Content/Eli/Effects/Dissolve.mgfxo");

		// post processor effects
		internal static byte[] BloomCombineBytes => GetFileResourceBytes("Content/Eli/Effects/BloomCombine.mgfxo");

		internal static byte[] BloomExtractBytes => GetFileResourceBytes("Content/Eli/Effects/BloomExtract.mgfxo");

		internal static byte[] GaussianBlurBytes => GetFileResourceBytes("Content/Eli/Effects/GaussianBlur.mgfxo");

		internal static byte[] VignetteBytes => GetFileResourceBytes("Content/Eli/Effects/Vignette.mgfxo");

		internal static byte[] LetterboxBytes => GetFileResourceBytes("Content/Eli/Effects/Letterbox.mgfxo");

		internal static byte[] HeatDistortionBytes => GetFileResourceBytes("Content/Eli/Effects/HeatDistortion.mgfxo");

		internal static byte[] SpriteLightMultiplyBytes => GetFileResourceBytes("Content/Eli/Effects/SpriteLightMultiply.mgfxo");

		internal static byte[] PixelGlitchBytes => GetFileResourceBytes("Content/Eli/Effects/PixelGlitch.mgfxo");

		internal static byte[] StencilLightBytes => GetFileResourceBytes("Content/Eli/Effects/StencilLight.mgfxo");

		// deferred lighting
		internal static byte[] DeferredSpriteBytes => GetFileResourceBytes("Content/Eli/Effects/DeferredSprite.mgfxo");

		internal static byte[] DeferredLightBytes => GetFileResourceBytes("Content/Eli/Effects/DeferredLighting.mgfxo");

		// forward lighting
		internal static byte[] ForwardLightingBytes => GetFileResourceBytes("Content/Eli/Effects/ForwardLighting.mgfxo");

		internal static byte[] PolygonLightBytes => GetFileResourceBytes("Content/Eli/Effects/PolygonLight.mgfxo");

		// scene transitions
		internal static byte[] SquaresTransitionBytes => GetFileResourceBytes("Content/Eli/Effects/transitions/Squares.mgfxo");

		// sprite or post processor effects
		internal static byte[] SpriteEffectBytes => GetMonoGameEmbeddedResourceBytes("Microsoft.Xna.Framework.Graphics.Effect.Resources.SpriteEffect.ogl.mgfxo");

		internal static byte[] MultiTextureOverlayBytes => GetFileResourceBytes("Content/Eli/Effects/MultiTextureOverlay.mgfxo");

		internal static byte[] ScanlinesBytes => GetFileResourceBytes("Content/Eli/Effects/Scanlines.mgfxo");

		internal static byte[] ReflectionBytes => GetFileResourceBytes("Content/Eli/Effects/Reflection.mgfxo");

		internal static byte[] GrayscaleBytes => GetFileResourceBytes("Content/Eli/Effects/Grayscale.mgfxo");

		internal static byte[] SepiaBytes => GetFileResourceBytes("Content/Eli/Effects/Sepia.mgfxo");

		internal static byte[] PaletteCyclerBytes => GetFileResourceBytes("Content/Eli/Effects/PaletteCycler.mgfxo");


		/// <summary>
		/// gets the raw byte[] from an EmbeddedResource
		/// </summary>
		/// <returns>The embedded resource bytes.</returns>
		/// <param name="name">Name.</param>
		static byte[] GetEmbeddedResourceBytes(string name)
		{
			var assembly = typeof(EffectResource).Assembly;
			using (var stream = assembly.GetManifestResourceStream(name))
			{
				using (var ms = new MemoryStream())
				{
					stream.CopyTo(ms);
					return ms.ToArray();
				}
			}
		}


		internal static byte[] GetMonoGameEmbeddedResourceBytes(string name)
		{
			var assembly = typeof(MathHelper).Assembly;
#if FNA
			name = name.Replace( ".ogl.mgfxo", ".fxb" );
#else
            // MG 3.8 decided to change the location of Effecs...sigh.
			if (!assembly.GetManifestResourceNames().Contains(name))
				name = name.Replace(".Framework", ".Framework.Platform");
#endif

			using (var stream = assembly.GetManifestResourceStream(name))
			{
				using (var ms = new MemoryStream())
				{
					stream.CopyTo(ms);
					return ms.ToArray();
				}
			}
		}


		/// <summary>
		/// fetches the raw byte data of a file from the Content folder. Used to keep the Effect subclass code simple and clean due to the Effect
		/// constructor requiring the byte[].
		/// </summary>
		/// <returns>The file resource bytes.</returns>
		/// <param name="path">Path.</param>
		public static byte[] GetFileResourceBytes(string path)
		{
#if FNA
			path = path.Replace( ".mgfxo", ".fxb" );
#endif

			byte[] bytes;
			try
			{
				using (var stream = TitleContainer.OpenStream(path))
				{
					if (stream.CanSeek)
					{
						bytes = new byte[stream.Length];
						stream.Read(bytes, 0, bytes.Length);
					}
					else
					{
						using (var ms = new MemoryStream())
						{
							stream.CopyTo(ms);
							bytes = ms.ToArray();
						}
					}
				}
			}
			catch (Exception e)
			{
				var txt = string.Format(
					"OpenStream failed to find file at path: {0}. Did you add it to the Content folder and set its properties to copy to output directory?",
					path);
				throw new Exception(txt, e);
			}

			return bytes;
		}
	}
}