using System;
using System.Collections.Generic;
using System.Text;
using Eli.ECS;
using Eli.Ecs3D.Components.Renderable.Materials;
using Microsoft.Xna.Framework;

namespace Eli.Ecs3D
{
    public interface IRenderable
    {
        BoundingBox BoundingBox { get; }
        int RenderLayer { get; set; }

        float LayerDepth { get; set; }

        Material Material { get; set; }
        bool IsVisibleFromCamera(Camera3D camera);

        void Render(ICamera camera);
    }

    /// <summary>
    /// Comparer for sorting IRenderables. Sorts first by RenderLayer, then LayerDepth. If there is a tie Materials
    /// are used for the tie-breaker to avoid render state changes.
    /// </summary>
    public class RenderableComparer : IComparer<IRenderable>
    {
        public int Compare(IRenderable self, IRenderable other)
        {
            var res = other.RenderLayer.CompareTo(self.RenderLayer);
            if (res == 0)
            {
                res = other.LayerDepth.CompareTo(self.LayerDepth);
                if (res == 0)
                {
                    // both null or equal
                    if (ReferenceEquals(self.Material, other.Material))
                        return 0;

                    if (other.Material == null)
                        return -1;

                    return 1;
                }
            }

            return res;
        }
    }
}
