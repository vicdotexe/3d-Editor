using System;
using System.Collections.Generic;
using System.Text;
using Eli.ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Eli;

namespace Eli
{
    /// <summary>
    /// Inherit from this class and make sure to set Gizmo.SetSelectionPool
    /// to whatever transformable objects you want to be interacting with the gizmo.
    /// Also a good place to hook onto the Gizmo.ObjectPicked and Object.Released events.
    /// Make sure to set the camera if you don't want it to use the scene's camera.
    /// </summary>
    public abstract class Gizmo3DSceneComponent : SceneComponent
    {

        public TransformGizmo Gizmo { get; private set; }
        public ICamera Camera;
        public bool UseDefaultKeyPresses = true;

        protected bool _dragging = false;
        public float ScaleDeltaModifier = 0.01f;
        public float RotationDeltaModifer = 1;
        public float TransformDeltaModifier = 1;

        public void SetDragging()
        {
            _dragging = true;
        }

        protected Gizmo3DSceneComponent():this(null){}
        protected Gizmo3DSceneComponent(ICamera camera) 
        {
            Gizmo = new TransformGizmo(Core.GraphicsDevice, Graphics.Instance.Batcher, null);
            Gizmo.TranslateEvent += GizmoTranslateEvent;
            Gizmo.RotateEvent += GizmoRotateEvent;
            Gizmo.ScaleEvent += GizmoScaleEvent;
            Camera = camera;
        }

        public override void OnAddedToScene(BaseScene scene)
        {
            base.OnAddedToScene(scene);
            if (Camera == null)
                Camera = scene.Camera;
        }

        public override void Update()
        {
            GizmoUpdate(Camera.ViewMatrix, Camera.ProjectionMatrix, Camera.TransformMatrix.Translation);
        }

        public override void Render()
        {
            Gizmo.Draw();
        }

        #region Gizmo Event Hooks


        private KeyboardState _previousKeys;
        private MouseState _previousMouse;
        private MouseState _currentMouse;
        private KeyboardState _currentKeys;


        private bool IsNewButtonPress(Keys key)
        {
            return _currentKeys.IsKeyDown(key) && _previousKeys.IsKeyUp(key);
        }

        private void GizmoUpdate(Matrix viewMatrix, Matrix projectionMatrix, Vector3 cameraPosition)
        {
            _currentMouse = Input.CurrentMouseState;
            _currentKeys = Keyboard.GetState();

            // update camera properties for rendering and ray-casting.
            Gizmo.UpdateCameraProperties(viewMatrix, projectionMatrix, cameraPosition);

            if (UseDefaultKeyPresses)
                DefaultKeypresses();

            CustomKeyPresses();
            UpdateSelectionPool();
            Gizmo.Update(Time.GameTime);

            _previousKeys = _currentKeys;
            _previousMouse = Input.CurrentMouseState;
            _dragging = false;
        }

        private void DefaultKeypresses()
        {
            var rect = new Rectangle(0,0, Screen.Width, Screen.Height);

            // select entities with your cursor (add the desired keys for add-to / remove-from -selection)
            if (_currentMouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released && !_dragging && rect.Contains(Input.MousePosition))
                Gizmo.SelectEntities(Input.MousePosition,
                    _currentKeys.IsKeyDown(Keys.LeftControl) || _currentKeys.IsKeyDown(Keys.RightControl),
                    _currentKeys.IsKeyDown(Keys.LeftAlt) || _currentKeys.IsKeyDown(Keys.RightAlt));

            // set the active mode like translate or rotate
            if (IsNewButtonPress(Keys.Z))
                Gizmo.ActiveMode = GizmoMode.Translate;
            if (IsNewButtonPress(Keys.X))
                Gizmo.ActiveMode = GizmoMode.Rotate;
            if (IsNewButtonPress(Keys.V))
                Gizmo.ActiveMode = GizmoMode.NonUniformScale;
            if (IsNewButtonPress(Keys.C))
                Gizmo.ActiveMode = GizmoMode.UniformScale;

            // toggle precision mode
            if (_currentKeys.IsKeyDown(Keys.LeftShift) || _currentKeys.IsKeyDown(Keys.RightShift))
                Gizmo.PrecisionModeEnabled = true;
            else
                Gizmo.PrecisionModeEnabled = false;

            // toggle active space
            if (IsNewButtonPress(Keys.Enter))
                Gizmo.ToggleActiveSpace();

            // toggle snapping
            if (IsNewButtonPress(Keys.G))
                Gizmo.SnapEnabled = !Gizmo.SnapEnabled;

            // select pivot types
            if (IsNewButtonPress(Keys.Tab))
                Gizmo.NextPivotType();

            // clear selection
            if (IsNewButtonPress(Keys.Escape))
                Gizmo.Clear();
        }
        protected abstract void CustomKeyPresses();

        /// <summary>
        /// 
        /// </summary>
        protected abstract void UpdateSelectionPool(
            );
        private void GizmoTranslateEvent(IGizmoObject transformable, TransformationEventArgs e)
        {
            transformable.Position += (Vector3)e.Value * TransformDeltaModifier;
        }

        private void GizmoRotateEvent(IGizmoObject transformable, TransformationEventArgs e)
        {
            e.Value = (Matrix)e.Value * Matrix.CreateScale(RotationDeltaModifer);
            Gizmo.RotationHelper(transformable, e);
        }

        private void GizmoScaleEvent(IGizmoObject transformable, TransformationEventArgs e)
        {
            Vector3 delta = (Vector3)e.Value * ScaleDeltaModifier;
            if (Gizmo.ActiveMode == GizmoMode.UniformScale)
                transformable.Scale *= 1 + ((delta.X + delta.Y + delta.Z) / 3);
            else
                transformable.Scale += delta;
            transformable.Scale = Vector3.Clamp(transformable.Scale, Vector3.Zero, transformable.Scale);
        }
        #endregion
    }
}
