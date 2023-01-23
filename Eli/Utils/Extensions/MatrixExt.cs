using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Eli
{
    public static class MatrixExt
    {
        /// <summary>
        /// Extract the correct scale from matrix.
        /// </summary>
        /// <param name="mat">Matrix to get scale from.</param>
        /// <returns>Matrix scale.</returns>
        public static Vector3 GetScale(this Matrix mat)
        {
            mat.Decompose(out Vector3 scale, out Quaternion rot, out Vector3 pos);
            return scale;
        }
    }
}
