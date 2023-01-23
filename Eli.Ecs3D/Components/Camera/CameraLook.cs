using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Eli.Ecs3D.Components.Camera
{
    public class CameraLook : Component, IUpdatable
    {
        private Camera3D _camera;
        public float Strength =5;

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            _camera = Entity.GetComponent<Camera3D>();
        }

        public void Update()
        {
            if (Input.RightMouseButtonDown)
            {
                var change = Input.MousePositionDelta.ToVector2() * Time.DeltaTime * Strength;
                _camera.Yaw(-change.X);
                _camera.Pitch(change.Y);
            }
        }
    }
}
