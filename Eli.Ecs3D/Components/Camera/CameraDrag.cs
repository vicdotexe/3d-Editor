using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Eli.Ecs3D.Components.Camera
{
    public class CameraDrag : Component, IUpdatable
    {
        public float DragSpeed = 100;
        private Vector2 dragOrigin;
        private Vector3 cameraOrigin;
        private Camera3D _camera;
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            _camera = Entity.GetComponent<Camera3D>();
        }

        public void Update()
        {
            if (Input.MiddleMouseButtonDown)
            {
                var change = dragOrigin - Input.MousePosition;
                var dragSpeed = this.DragSpeed;
                if (Input.IsKeyDown(Keys.LeftShift))
                    dragSpeed *= 0.1f;
                _camera.Position += (_camera.Right * Input.MousePositionDelta.X * dragSpeed * Time.DeltaTime) + (_camera.TransformMatrix.Up * Input.MousePositionDelta.Y * dragSpeed * Time.DeltaTime);
            }
        }
    }
}
