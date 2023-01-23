using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eli.ImGuiTools;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace Eli.Ecs3D.ImGuiTools
{
    public class DraggedObject
    {
        public object Context;
    }

    public class EditorGui : ImGuiManager
    {
        static float w = 200.0f;
        private static float w2 = 1350;
        static float h = 760;

        public EditorGui(bool useCustomLayout, ImGuiOptions options = null) : base(useCustomLayout, options)
        {
            AddCustomDrawCall(Layout);
            _mInspector = new ModelAssetsInspector();
        }

        public EventHandler<object> OnDraggedIntoGame;

        public static Entity ActiveEntity
        {
            get => _activeEntity;
        }

        public static void SetActiveEntity(Entity entity, bool focusWindow)
        {
            if (_activeEntity != entity)
            {
                _activeEntity = entity;
                _eInspector = new EntityInspectorFrame(entity);
                _eInspector.SetWindowFocus(focusWindow);
            }
        }
        private static EntityInspectorFrame _eInspector;
        private static ModelAssetsInspector _mInspector;

        public static System.Numerics.Vector2 CenterSize;

        private static Entity _activeEntity;
        public static System.Numerics.Vector2 CenterLocation;


        private void Layout()
        {
            var mainFlags =
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoBackground |
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoBringToFrontOnFocus |
                ImGuiWindowFlags.NoResize;
            var areaSize = new System.Numerics.Vector2(Screen.Width, Screen.Height - _mainMenuBarHeight - 25);
            //w = areaSize.X;
            var pos = new System.Numerics.Vector2(0, _mainMenuBarHeight);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new System.Numerics.Vector2(0, 0));
            ImGui.SetNextWindowPos(pos);
            ImGui.SetNextWindowSize(areaSize);
            if (ImGui.Begin("MainWindow", mainFlags))
            {

                ImGui.BeginChild("left", new System.Numerics.Vector2(w, h), true);
            ImGui.EndChild();
            ImGui.SameLine();

            ImGui.InvisibleButton("vsplitter1", new System.Numerics.Vector2(4.0f, h));
            if (ImGui.IsItemActive())
            {
                w += ImGui.GetIO().MouseDelta.X;
                w2 -= ImGui.GetIO().MouseDelta.X;
            }

            ImGui.SameLine();
            ImGui.BeginChild("center", new System.Numerics.Vector2(w2, h), true);
            ImGui.EndChild();
            ImGui.SameLine();

            ImGui.InvisibleButton("vsplitter2", new System.Numerics.Vector2(4.0f, h));
            if (ImGui.IsItemActive())
                w2 += ImGui.GetIO().MouseDelta.X;

            ImGui.SameLine();
            ImGui.BeginChild("right", new System.Numerics.Vector2(0, h), true);
            _eInspector?.Draw();
            ImGui.EndChild();

            ImGui.InvisibleButton("hsplitter", new System.Numerics.Vector2(-1, 4.0f));
            if (ImGui.IsItemActive())
                h += ImGui.GetIO().MouseDelta.Y;

            ImGui.BeginChild("bottom", new System.Numerics.Vector2(0, 0), true);
            _mInspector?.DrawGui();
            ImGui.EndChild();

            CenterSize = new System.Numerics.Vector2(w2, h);
            CenterLocation = new System.Numerics.Vector2(w + 10 + 4, _mainMenuBarHeight);
            Camera3D.Main.AspectRatio = w2 / h;
            }

            ImGui.End();
            ImGui.PopStyleVar();

            if (fireDrag)
            {
                OnDraggedIntoGame?.Invoke(this, DraggedObject);
                DraggedObject = null;
                fireDrag = false;
            }

            if (IsDragging && GameWindowRect.Contains(ImGui.GetMousePos().ToXNA()))
            {
                ImGui.SetWindowFocus(GameWindowTitle);
                fireDrag = true;
            }

            
        }

        private bool fireDrag = false;

        private bool wasDragging;


        public override void AdjustGameWindow()
        {
            ImGui.SetNextWindowPos(CenterLocation);
            ImGui.SetNextWindowSize(CenterSize);
            GameWindowRect = new RectangleF(CenterLocation.ToXNA(), CenterSize.ToXNA());
        }


    }
}
