using System;
using System.Collections.Generic;
using System.Text;
using Eli.ECS;
using Eli.Ecs3D.Components.Renderable.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Eli.Ecs3D
{
    public class DefaultRenderer : Renderer<EntityScene3D>
    {
        public DefaultRenderer(int renderOrder) : base(renderOrder)
        {
        }



        public override void OnAddedToScene(EntityScene3D scene)
        {
            base.OnAddedToScene(scene);
            DebugHelper.AddOnKeyPress(Keys.Up, Increase);
            DebugHelper.AddOnKeyPress(Keys.Down, Decrease);
            #region Hotkey Explanation
            _helpTextBuilder = new StringBuilder();
            _helpTextBuilder.AppendLine("Hotkeys:");

            _helpTextBuilder.AppendLine("1,2,3,4 to switch Transformation Modes");
            _helpTextBuilder.AppendLine("O = Switch space (Local/World)");
            _helpTextBuilder.AppendLine("I = Toggle Snapping");
            _helpTextBuilder.AppendLine("P = Switch PivotTypes");
            _helpTextBuilder.AppendLine("Hold Control = Add to selection");
            _helpTextBuilder.AppendLine("Hold Shift = Precision Mode");
            _helpTextBuilder.AppendLine("Hold Alt = Remove from selection");

            _helpText = _helpTextBuilder.ToString();

            #endregion
        }

        private string _helpText;
        private StringBuilder _helpTextBuilder;

        private int modelNumber = 2;

        private void Increase()
        {
            modelNumber+=1;
        }

        private void Decrease()
        {
            modelNumber-=1;
        }
        public override void Render(EntityScene3D scene)
        {
            modelNumber = Mathf.Clamp(modelNumber, 0, scene.World.RenderableComponents.Count - 1);
            Core.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Core.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Core.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            Core.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
            MaterialAPI.SetViewProjection(scene.Camera.ViewMatrix, scene.Camera.ProjectionMatrix);
            for (int i = 0; i < scene.World.RenderableComponents.Count; i++)
            {
                scene.World.RenderableComponents[i].Render(scene.Camera);

            }

            /*
            //Graphics.DrawWireSphere(Matrix.Identity, scene.Camera.ViewMatrix, scene.Camera.ProjectionMatrix, Color.Pink);
            _lineX.Draw(scene.Camera, Core.GraphicsDevice);
            _lineY.Draw(scene.Camera, Core.GraphicsDevice);
            _lineZ.Draw(scene.Camera, Core.GraphicsDevice);

            foreach (var e in scene.World.Entities.ActiveBuffer)
            {
                if (e == null)
                    continue;
                if (e.GetComponent<Camera3D>() == scene.Camera)
                    continue;

                e.EditorRender(scene.Camera);
            }
            */
            /*
            Graphics.Instance.Batcher.Begin();
            foreach (var e in scene.World.Entities.ActiveBuffer)
            {
                if (e == null)
                    continue;
                if (e.GetComponent<Camera3D>() == scene.Camera)
                    continue;
                e.ScreenSpaceDebugRender(scene.Camera);
            }
            Graphics.Instance.Batcher.DrawString(Graphics.Instance.BitmapFont, _helpText, new Vector2(5), Color.White);
            Graphics.Instance.Batcher.End();
            */
        }
    }
}
