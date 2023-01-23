using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Eli.ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Eli.Ecs3D
{
    public enum CameraMode
    {
        Target,
        Free
    }
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Camera3D : Component, ICamera
    {

        
        public static Camera3D Main
        {
            get => _mainCamera;
            set => _mainCamera = value;
        }
        private static Camera3D _mainCamera;

        #region Public Properties
        public Vector3 Position
        {
            get
            {
                if (Entity != null)
                    return Entity.Position;
                return _position;
            }
            set
            {
                if (Entity != null)
                {
                    if (value != Entity.Position)
                    {
                        Entity.Position = value;
                        _viewDirty = true;
                    }
                }
                else
                {
                    if (value != _position)
                    {
                        _position = value;
                        _viewDirty = true;
                    }
                }
            }
        }

        public Vector3 Up
        {
            get => _up;
            set
            {
                if (value != _up)
                {
                    _up = value;
                    _viewDirty = true;
                }
            }
        }

        public Vector3 Forward
        {
            get => _forward;
            set
            {
                if (value != _forward)
                {
                    _forward = value;
                    _viewDirty = true;
                }
            }
        }
        public Vector3 Right => -Vector3.Normalize(Vector3.Cross(Forward, Up));
        public Vector3 Left => -Right;

        public float FOV
        {
            get
            {
                return _fov;
            }
            set
            {
                if (value != _fov)
                {
                    _fov = value;
                    _projectionDirty = true;
                }
            }
        }

        public float AspectRatio
        {
            get => _aspectRatio;
            set
            {
                if (value != _aspectRatio)
                {
                    _aspectRatio = value;
                    _projectionDirty = true;
                }
            }
        }

        public float NearPlane
        {
            get => _nearPlane;
            set
            {
                if (value != _nearPlane)
                {
                    _nearPlane = value;
                    _projectionDirty = true;
                }
            }
        }

        public float FarPlane
        {
            get => _farPlane;
            set
            {
                if (value != _farPlane)
                {
                    _farPlane = value;
                    _projectionDirty = true;
                }
            }
        }
        public  Matrix ViewMatrix
        {
            get
            {
                if (_viewDirty)
                {
                    _viewMatrix = Matrix.CreateLookAt(Position, Position + Forward, Up);
                    _viewDirty = false;
                }

                return _viewMatrix;
            }
        }


        public  Matrix ProjectionMatrix
        {
            get
            {
                if (_projectionDirty)
                {
                    _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(FOV, AspectRatio, NearPlane, FarPlane);
                    _projectionDirty = false;
                }

                return _projectionMatrix;
            }

        }

        public  Matrix TransformMatrix
        {
            get
            {
                if (Entity != null)
                    return Entity.WorldMatrix;
                return _transformMatrix;
            }
        }
        public  Matrix InverseTransformMatrix => Matrix.Invert(TransformMatrix);

        public Camera.Dimensions Type => throw new NotImplementedException();
        #endregion

        #region Private Variables
        private Vector3 _position;
        private Vector3 _up = new Vector3(0, 1, 0);
        private Vector3 _forward = new Vector3(0, 0, -1);
        private float _fov = MathHelper.PiOver4;
        private float _aspectRatio;
        private float _nearPlane = 1;
        private float _farPlane = 10000;
        private bool _projectionDirty;
        private bool _viewDirty;
        private Matrix _viewMatrix;
        private Matrix _projectionMatrix;
        private Matrix _transformMatrix = Matrix.Identity;
        #endregion

        public Camera3D()
        {
            // TODO: Construct any child components here
            //if (_mainCamera == null) 
                //_mainCamera = this;
            AspectRatio = (float)Screen.Width / (float)Screen.Height;
        }

        #region Component Overrides
        public override void OnEntityTransformChanged()
        {
            base.OnEntityTransformChanged();
            _viewDirty = true;
        }

        public void OnSceneRenderTargetSizeChanged(int width, int height)
        {
            AspectRatio = (float)width / (float)height;
        }
        #endregion

        #region Movement Methods
        public void MoveForward(float amount)
        {
            Position += Vector3.Normalize(Forward) * amount;
        }

        public void Strafe(float amount)
        {
            var change = Right * amount;
            Position += new Vector3(change.X, 0, change.Z);
        }

        public void Pitch(float amount)
        {
            Forward = Vector3.Transform(Forward, Matrix.CreateFromAxisAngle(Right, MathHelper.ToRadians(amount)));
        }

        public void Yaw(float amount)
        {
            Forward = Vector3.Transform(Forward, Matrix.CreateFromAxisAngle(Up, MathHelper.ToRadians(amount)));
        }
        #endregion

        public Ray PickingRay(Vector2 screenLocation, Viewport viewport)
        {
            Vector3 nearPoint = viewport.Unproject(new Vector3(screenLocation.X,
                    screenLocation.Y, 0.0f),
                ProjectionMatrix,
                ViewMatrix,
                Matrix.Identity);

            Vector3 farPoint = viewport.Unproject(new Vector3(screenLocation.X,
                    screenLocation.Y, 1.0f),
                ProjectionMatrix,
                ViewMatrix,
                Matrix.Identity);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }

        public void ForceMatrixUpdate()
        {
            _projectionDirty = true;
            _viewDirty = true;
        }
        
        public override string Serialize()
        {
            return $"{GetType()},{Forward},{Up}";
        }

        public void Deserialize(string[] info)
        {
            foreach (var i in info)
            {
                
            }
        }

    }
}
