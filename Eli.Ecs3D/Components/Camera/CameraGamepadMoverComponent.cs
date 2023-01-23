using System;
using System.Collections.Generic;
using System.Text;

namespace Eli.Ecs3D
{
    public class CameraGamepadMoverComponent : Component, IUpdatable
    {
        private Camera3D _camera;
        private Ps4Input _input;
        private bool inverse = false;
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            _camera = Entity.GetComponent<Camera3D>();
            _input = new Ps4Input();
        }

        public void Update()
        {
            if (Ps4Input.Main.Triangle.IsPressed)
                inverse = !inverse;

            _camera.MoveForward(120 * Ps4Input.Main.LeftStick.Value.Y * Time.DeltaTime);
            _camera.Strafe(-120 * Ps4Input.Main.LeftStick.Value.X * Time.DeltaTime);

            _camera.Pitch((inverse ? 1 : -1) * -90*Ps4Input.Main.RightStick.Value.Y * Time.DeltaTime);
            _camera.Yaw(-90*Ps4Input.Main.RightStick.Value.X * Time.DeltaTime);
        }

        public override string Serialize()
        {
            return $"{GetType()}";
        }
    }
}
