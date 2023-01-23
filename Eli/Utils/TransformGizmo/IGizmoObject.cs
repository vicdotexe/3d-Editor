using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Eli
{
    public interface IGizmoObject : ITransformable3D
    {
        BoundingBox EditorBoundingBox { get; }
        float? Select(Ray selectionRay);
    }
}
