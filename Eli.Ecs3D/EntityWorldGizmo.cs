using System;
using System.Collections.Generic;
using System.Text;
using Eli.ECS;
using Microsoft.Xna.Framework.Input;
using Eli;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eli.Ecs3D
{
    public class EntityWorldGizmo : Gizmo3DSceneComponent
    {
        private EntityScene3D _scene;

        public override void OnAddedToScene(BaseScene scene)
        {
            base.OnAddedToScene(scene);
            _scene = scene.As<EntityScene3D>();
        }

        protected override void CustomKeyPresses()
        {
            var _currentMouse = Input.CurrentMouseState;
            var _previousMouse = Input.PreviousMouseState;
            var _currentKeys = Input.CurrentKeyboardState;


            if (Input.IsKeyPressed(Keys.Delete))
            {
                foreach (var entity in Gizmo.Selection)
                    (entity as Entity).Destroy();
                Gizmo.Clear();
            }
        }

        protected override void UpdateSelectionPool()
        {
            Gizmo.SetSelectionPool(_scene.World.Entities.ActiveBuffer);
        }

        public void OnDraggedIntoGame(object sender, object context)
        {
            if (context is Model)
            {
                var entity = Scene.As<EntityScene3D>().World.AddEntity(new Entity());
                entity.AddComponent(new ModelComponent()).SetModel(context as Model);
                //entity.Position = Camera3D.Main.Position +
                //                  (Camera3D.Main.Forward * Camera3D.Main.FarPlane * 0.05f);

                var forward = Camera3D.Main.Forward;
                var up = Camera3D.Main.Up;
                var right = Camera3D.Main.Right;

                var relativeUp = -forward - up;
                relativeUp.Normalize();
                var yDis = (Camera3D.Main.Position + (Camera3D.Main.Forward * 500)).Y;
                Plane plane = new Plane(Vector3.Up, -yDis);
                var ray = Camera3D.Main.PickingRay(Input.MousePosition, Core.GraphicsDevice.Viewport);
                float? distance = ray.Intersects(plane);
                if (distance == null)
                    return;
                var mousePos = ray.Position + ray.Direction * distance.Value;

                //var desiredCenter = Camera3D.Main.Position + (Camera3D.Main.Forward * 500);
                //var diff = desiredCenter - mousePos;
                //var newPos = new Vector3(mousePos.X,desiredCenter.Y,mousePos.Z);
                //newPos += new Vector3(diff.X, 0, diff.Z);
                entity.Position = mousePos;
                Gizmo.ForcePick(entity);
                Gizmo.ActiveMode = GizmoMode.Translate;
                Gizmo.ActivePivot = PivotType.ObjectCenter;
                Gizmo.ActiveAxis = GizmoAxis.ZX;
                Gizmo.Enabled = true;
                SetDragging();
            }

        }
    }
}
