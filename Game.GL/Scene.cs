using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eli;
using Eli.ECS;
using Eli.Ecs3D;
using Eli.Ecs3D.Components.Camera;
using Eli.Ecs3D.Components.Renderable;
using Eli.Ecs3D.ImGuiTools;
using Eli.Ecs3D.Terrain;
using Eli.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game.GL
{
    public class Scene : EntityScene3D
    {

        public Scene()
        {
            var imguiManager = new EditorGui(true);
            Core.RegisterGlobalManager(imguiManager);
        }

        public override void OnStart()
        {
            AddSceneComponent<DebugHelper>();
            AddSceneComponent<EntityWorldGizmo>().Gizmo.ObjectPicked += SelectEntity;
            Core.GetGlobalManager<EditorGui>().OnDraggedIntoGame +=
                GetSceneComponent<EntityWorldGizmo>().OnDraggedIntoGame;
            //Core.GetGlobalManager<EditorGui>().AddCustomDrawCall(window.Draw);
            Camera.Position = new Vector3(50) * 10;
            var cameraEntity = Camera.Entity;
            cameraEntity.AddComponent<CameraGamepadMoverComponent>();
            cameraEntity.AddComponent<CameraDrag>();
            cameraEntity.AddComponent<CameraScrollWheelZoom>();
            cameraEntity.AddComponent<CameraLook>();
            cameraEntity.AddComponent<CameraWasd>();
            cameraEntity.AddComponent(LineRenderer.CreateGrid(100, 1000, Color.Red, Color.Blue));
            AddRenderer(new DefaultRenderer(0));

            var testEntity = World.AddEntity(new Entity("test2"));
            //testEntity.AddComponent<TerrainComponent>();
            var model = Content.LoadModel("Eli/Models/Dungeon/Carpet");
            var model2 = Content.LoadModel("Eli/Models/Dungeon/Candle");
            var sun = World.AddEntity(new Entity("Sun")).AddComponent<LightSource>().Entity
                .AddComponent(new ModelComponent());
                sun.Model = Content.LoadBasicModel(BasicModel.Sphere);
            sun.Entity.Rotation =
                Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationX(Mathf.Deg2Rad * 90) *
                                                    Matrix.CreateRotationY(Mathf.Deg2Rad * 180));
            sun.Entity.Scale = new Vector3(0.25f);
            atlas = Content.LoadSpriteAtlas("Content/Eli/Textures/Zerosuit/zerosuit.atlas");
            //var modelComp = testEntity.AddComponent(new ModelComponent());
            //modelComp.Model = model;


            DebugHelper.AddOnKeyPress(Keys.P, delegate
            {
                World.Save("save1");
            });

            DebugHelper.AddOnKeyPress(Keys.O, delegate
            {
                World.Load("save1");
            });
        }

        public SpriteAtlas atlas;

        private void SelectEntity(object sender, IGizmoObject entity)
        {
            EditorGui.SetActiveEntity ((Entity)entity, false);
        }

        public override RenderTarget2D PostRender()
        {
            var rt = base.PostRender();
            Core.GraphicsDevice.SetRenderTarget(rt);
            Graphics.Instance.Batcher.Begin();
            if (TerrainComponent.StaticHeightMap != null)
            {
                Graphics.Instance.Batcher.Draw(TerrainComponent.StaticHeightMap.ColorMap, new Rectangle(0,0,512,512));
                Graphics.Instance.Batcher.Draw(TerrainComponent.StaticHeightMap.Texture, new Rectangle(0,512,512,512));
            }

            Graphics.Instance.Batcher.End();
            return rt;
        }
    }
}
