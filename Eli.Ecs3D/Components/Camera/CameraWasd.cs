using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace Eli.Ecs3D.Components.Camera
{
    public class CameraWasd : Component, IUpdatable
    {
        private Camera3D _camera;
        public float Strength = 400;
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            _camera = Entity.GetComponent<Camera3D>();
        }

        public void Update()
        {
            float forward = 0;
            if (Input.IsKeyDown(Keys.W))
                forward += 1;
            if (Input.IsKeyDown(Keys.S))
                forward -= 1;

            float strafe = 0;
            if (Input.IsKeyDown(Keys.A))
                strafe -= 1;
            if (Input.IsKeyDown(Keys.D))
                strafe += 1;

            if (Input.IsKeyDown(Keys.LeftShift))
            {
                strafe *= 0.1f;
                forward *= 0.1f;
            }

            _camera.MoveForward(forward * Strength * Time.DeltaTime);
            _camera.Strafe(-strafe * Strength * Time.DeltaTime);
        }
    }
}
