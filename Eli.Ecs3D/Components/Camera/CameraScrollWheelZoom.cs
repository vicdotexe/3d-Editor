using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eli.Tweens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Eli.Ecs3D.Components.Camera
{
    public class CameraScrollWheelZoom : Component, IUpdatable
    {
        public float Strength = 5;
        private int _currentDelta;
        private int _previousDelta;
        private float _timer;
        private Camera3D _camera;
        private int _lastDelta;
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            _camera = Entity.GetComponent<Camera3D>();
        }

        public void Update()
        {
            _currentDelta = Mouse.GetState().ScrollWheelValue;

            if (_currentDelta != _previousDelta)
            {
                _timer = 0.5f;
                _lastDelta = _currentDelta - _previousDelta;
            }

            if (_timer > 0)
            {
                
                var strength = Strength;
                if (Input.IsKeyDown(Keys.LeftShift))
                    strength *= 0.1f;
                _camera.Position = Lerps.Ease(EaseType.Linear, _camera.Position,
                    _camera.Position + (_camera.Forward * _lastDelta * strength * Time.DeltaTime), _timer, 0.5f);
            }

            _timer -= Time.DeltaTime;
            _previousDelta = _currentDelta;
        }
    }
}
