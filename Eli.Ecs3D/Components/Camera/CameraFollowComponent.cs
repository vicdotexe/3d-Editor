using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Eli.Ecs3D
{
    public class CameraFollowComponent : Component, IUpdatable
    {
        private Camera3D _camera;
        private Transform3 Target;

        public CameraFollowComponent(Transform3 target)
        {
            Target = target;
        }
        public void Update()
        {
            _camera.Forward = _camera.Position - Target.Position;
            _camera.Forward.Normalize();
        }

        public void RotateZAroundTarget(float ammount)
        {
            var target = Target.Position * new Vector3(1, 1, 1);
            _camera.Position = Vector3.Transform(_camera.Position - target, Matrix.CreateFromAxisAngle(_camera.Up, ammount)) + target;
        }

        public void RotateYAroundTarget(float ammount)
        {
            var target = Target.Position * new Vector3(1, 1, 1);
            var dir = Vector3.Normalize(_camera.Position - target);
            var axis = Vector3.Cross(dir, _camera.Up);
            _camera.Position = Vector3.Transform(_camera.Position - target, Matrix.CreateFromAxisAngle(axis, ammount)) + target;
        }

        public void ZoomOutToTarget(float multiplier = 10)
        {
            var target = Target.Position * new Vector3(1, 1, 1);
            var dir = Vector3.Normalize(_camera.Position - target);
            _camera.Position += dir * multiplier;
        }
        public void ZoomInToTarget(float multiplier = 10)
        {
            var target = Target.Position * new Vector3(1, 1, 1);
            var dir = Vector3.Normalize(_camera.Position - target);
            _camera.Position -= dir * multiplier;
        }
    }
}
