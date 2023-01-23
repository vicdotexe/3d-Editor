using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;


namespace Eli
{
	public static class Vector3Ext
	{
		/// <summary>
		/// returns a Vector2 ignoring the z component
		/// </summary>
		/// <returns>The vector2.</returns>
		/// <param name="vec">Vec.</param>
		public static Vector2 ToVector2(this Vector3 vec)
		{
			return new Vector2(vec.X, vec.Y);
		}

        /// <summary>
        /// rounds the x and y values
        /// </summary>
        /// <param name="vec">Vec.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Round(this Vector3 vec)
        {
            return new Vector3(Mathf.Round(vec.X), Mathf.Round(vec.Y), Mathf.Round(vec.Z));
        }


	}
}