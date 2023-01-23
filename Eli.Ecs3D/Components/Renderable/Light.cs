using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eli.Ecs3D.Components.Renderable.Materials;
using Microsoft.Xna.Framework;

namespace Eli.Ecs3D.Components.Renderable
{
    public static class LightManager
    {
        static BoundingBox Region = new BoundingBox(new Vector3(-10000), new Vector3(10000));
        public static List<LightSource> GetLights(Matrix world, BoundingSphere boundingSpehere)
        {
            var list = ListPool<LightSource>.Obtain();
            FindLights(ref list, world, boundingSpehere);
            return list;
        }

        public static void AddLight(LightSource light)
        {
            _lights.Add(light);
        }

        public static void RemoveLight(LightSource light)
        {
            if (_lights.Contains(light))
                _lights.Remove(light);
        }
        private static List<LightSource> _lights = new List<LightSource>();

        static void FindLights(ref List<LightSource> list, Matrix world, BoundingSphere boundingSphere)
        {
            foreach (var light in _lights)
            {
                if (Region.Contains(boundingSphere) != ContainmentType.Disjoint)
                {
                    list.Add(light);
                }
            }
        }


    }

    public class LightSource : Component, IUpdatable
    {
        public Color Color = Color.White;
        public float Intensity = 1;
        public float Specular = 1;
        public Vector3 Direction;
        public bool IsDirectional = true;
        public float Range = 1000;
        public bool IsInfinite => Range == 0;
        public Vector3 Position;

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            LightManager.AddLight(this);
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();
            LightManager.RemoveLight(this);
        }

        public void Update()
        {
            Position = Entity.Transform.Position;
            Direction = Vector3.Transform(Vector3.Forward, Entity.WorldRotation);
        }
    }
}
