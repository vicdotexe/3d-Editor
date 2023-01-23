using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eli.ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Eli.Ecs3D.Terrain
{
    public enum TerrainActions
    {
        Paint,
        Smooth,
        Raise,
        Lower
    }

    public class TerrainComponent : RenderableComponent, IUpdatable
    {
        public HeightMap HeightMap;
        public static QuadTree QuadTree => StaticHeightMap.QuadTree;
        public static HeightMap StaticHeightMap;
        public bool EditMode = false;
        public TerrainActions EditType;
        public int PaintTextureId = 1;

        public Vector3 LightDirection
        {
            //get { return HeightMap.Effect.LightDirection; }
            //set { HeightMap.Effect.LightDirection = value; }
            get { return new Vector3(); }
            set {  }
        }

        public Color AmbientLight
        {
            get
            {
                var al = HeightMap.Effect.AmbientLight;

                return new Color(al);
            }
            set { HeightMap.Effect.AmbientLight = value.ToVector3(); }
        }

        public float LightPower
        {
            get { return HeightMap.Effect.LightPower; }
            set { HeightMap.Effect.LightPower = value; }
        }
        public TerrainComponent()
        {
            HeightMap = new HeightMap(100);

            //HeightMap.RandomizeHeight(GenerationType.Random, true);
            //HeightMap.SmoothHeightMap();
            StaticHeightMap = HeightMap;
        }

        public override void Initialize()
        {
            base.Initialize();
            HeightMap.CreateNewHeightMap(MapSize.Small);
            //HeightMap.RandomizeHeight(GenerationType.Random, true);
            //HeightMap.SmoothHeightMap();
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

        }

        public override void Render(ICamera camera)
        {
            base.Render(camera);
            HeightMap.Draw(Entity.WorldMatrix,camera.ViewMatrix,camera.ProjectionMatrix);
        }

        protected override void CalculateBoundingSphere()
        {
            throw new NotImplementedException();
        }

        protected override void CalculateBoundingBox()
        {
            throw new NotImplementedException();
        }

        private int layerId = 0;

        private void EditLogic()
        {
            if (!EditMode)
                return;
            var mouseRay = Camera3D.Main.PickingRay(Input.MousePosition, Core.GraphicsDevice.Viewport);

            var rayLength = QuadTree.Intersects(ref mouseRay, out var triangle, Entity.WorldMatrix);

            if (rayLength.HasValue)
            {
                HeightMap.Effect.DrawCursor = true;
                Vector3 rayTarget = mouseRay.Position + mouseRay.Direction * rayLength.Value;

                HeightMap.groundCursorPosition.X = ((int)(rayTarget.X / HeightMap.CellSize) *
                                                    (int)HeightMap.CellSize) / (HeightMap.Size * HeightMap.CellSize);

                HeightMap.groundCursorPosition.Y = rayTarget.Y;

                HeightMap.groundCursorPosition.Z = ((int)(rayTarget.Z / HeightMap.CellSize) *
                                                    (int)HeightMap.CellSize) / (HeightMap.Size * HeightMap.CellSize);
                Point pixel = new Point((int)(rayTarget.X / HeightMap.CellSize),
                    (int)(rayTarget.Z / HeightMap.CellSize));

                if (Input.LeftMouseButtonDown)
                {
                    switch (EditType)
                    {
                        case TerrainActions.Paint:
                            HeightMap.Paint(pixel.X,pixel.Y, PaintTextureId);
                            break;
                        case TerrainActions.Smooth:
                            HeightMap.Smooth();
                            break;
                        case TerrainActions.Raise:
                            HeightMap.RaiseHeight();
                            break;
                        case TerrainActions.Lower:
                            HeightMap.LowerHeight();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

            }
            else
            {
                HeightMap.Effect.DrawCursor = false;
            }

        }


        public void Update()
        {

            EditLogic();
            if (Input.IsKeyPressed( (Keys.N)))
                layerId++;
            if (layerId > 3)
                layerId = 0;

            HeightMap.Update();
            HeightMap.UpdateHeightTexture();
        }
    }
}
