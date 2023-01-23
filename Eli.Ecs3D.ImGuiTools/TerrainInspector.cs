using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eli.Ecs3D.ImGuiTools.ObjectInspectors;
using Eli.Ecs3D.Terrain;
using ImGuiNET;
using Microsoft.Xna.Framework;
using ImVec2 = System.Numerics.Vector2;

namespace Eli.Ecs3D.ImGuiTools.TypeInspectors
{
    public class TerrainInspector : AbstractTypeInspector
    {
        private HeightMap _heightMap;
        private TerrainEditor _editor;

        public override void Initialize()
        {
            base.Initialize();
            _heightMap = GetValue<HeightMap>();
        }

        public override void DrawMutable()
        {
            if (ImGui.Button("Edit"))
            {
                _editor = new TerrainEditor(_heightMap);
            }

            _editor?.DrawImgui();
        }
    }

    public class TerrainComponentInspector : ComponentInspector
    {
        private TerrainComponent _component;
        public TerrainComponentInspector(Component component) : base(component)
        {
            _component = component as TerrainComponent;
            sliderId = Utils.RandomString();
        }
        public int Buttons;
        public int Texture;
        private float testY;
        public override void Draw()
        {
            base.Draw();

            var xz = new Vector2(_component.LightDirection.X,_component.LightDirection.Z);
            test = Mathf.Atan2(xz.Y, xz.X);
            SliderFunctionCycle(sliderId, ref test, (2 * (float)Math.PI));
            _component.LightDirection = new Vector3(Mathf.Cos(test), _component.LightDirection.Y, Mathf.Sin(test));
            ImGui.SliderAngle("Testttt", ref test);
            _component.LightDirection = new Vector3(Mathf.Cos(test), _component.LightDirection.Y, Mathf.Sin(test));
            
            _component.LightDirection = new Vector3(_component.LightDirection.X, testY,_component.LightDirection.Z);
            ImGui.VSliderFloat("Yaxis", new ImVec2(10, 50), ref testY, -1, 1);
            _component.LightDirection = new Vector3(_component.LightDirection.X, testY, _component.LightDirection.Z);

            if (!_component.EditMode)
                return;
            ImGui.PushID(_scopeId);
            ImGui.RadioButton("Paint", ref Buttons, 0);
            if (Buttons == 0)
            {
                ImGui.RadioButton("Grass", ref Texture, 0);
                ImGui.RadioButton("Dirt", ref Texture, 1);
                ImGui.RadioButton("Rock", ref Texture, 2);
                ImGui.RadioButton("Snow", ref Texture, 3);

            }
            ImGui.RadioButton("Smooth", ref Buttons, 1);
            ImGui.RadioButton("Raise", ref Buttons, 2);
            ImGui.RadioButton("Lower", ref Buttons, 3);
            ImGui.DragInt("CursorSize", ref _component.HeightMap.CursorSize, 1);
            ImGui.DragInt("CursorStrength", ref _component.HeightMap.CursorStrength, 1);
            ImGui.PopID();
            _component.EditType = (TerrainActions) Buttons;
            _component.PaintTextureId = Texture;
        }

        private float test = 0;
        private string sliderId;

        
        private bool SliderFunctionLimited(string id, ref float sValue, float vMin, float vMax)
        {
            var style = ImGui.GetStyle();

            float radiusOuter = 20.0f;
            var pos = ImGui.GetCursorScreenPos();
            var center = new ImVec2(pos.X + radiusOuter, pos.Y + radiusOuter);
            float lineHeight = ImGui.GetTextLineHeight();
            var drawList = ImGui.GetWindowDrawList();

            float ANGLE_MIN = 3.141592f * 0.75f;
            float ANGLE_MAX = 3.141592f * 2.25f;

            ImGui.InvisibleButton(id, new ImVec2(radiusOuter * 2, radiusOuter * 2 + lineHeight + style.ItemInnerSpacing.Y));
            bool value_changed = false;
            bool is_active = ImGui.IsItemActive();
            bool is_hovered = ImGui.IsItemHovered();

            if (is_active && ImGui.GetMouseDragDelta().X != 0.0f)
            {
                float step = (vMax - vMin) / 200.0f;
                sValue = ImGui.GetMouseDragDelta().X * step;
                if (sValue < vMin) sValue = vMin;
                if (sValue > vMax) sValue = vMax;
                value_changed = true;
            }

            float t = (sValue - vMin) / (vMax - vMin);
            float angle = ANGLE_MIN + (ANGLE_MAX - ANGLE_MIN) * t;
            float angle_cos = Mathf.Cos(angle), angle_sin = Mathf.Sin(angle);
            float radius_inner = radiusOuter * 0.40f;
            drawList.AddCircleFilled(center, radiusOuter, ImGui.GetColorU32(ImGuiCol.FrameBg), 16);
            drawList.AddLine(new ImVec2(center.X + angle_cos * radius_inner, center.Y + angle_sin * radius_inner), new ImVec2(center.X + angle_cos * (radiusOuter - 2), center.Y + angle_sin * (radiusOuter - 2)), ImGui.GetColorU32(ImGuiCol.SliderGrabActive), 2.0f);
            drawList.AddCircleFilled(center, radius_inner, ImGui.GetColorU32(is_active ? ImGuiCol.FrameBgActive : is_hovered ? ImGuiCol.FrameBgHovered : ImGuiCol.FrameBg), 16);
            drawList.AddText(new ImVec2(pos.X, pos.Y + radiusOuter * 2 + style.ItemInnerSpacing.Y), ImGui.GetColorU32(ImGuiCol.Text), id);

            if (is_active || is_hovered)
            {
                ImGui.SetNextWindowPos(new ImVec2(pos.X - style.WindowPadding.X, pos.Y - lineHeight - style.ItemInnerSpacing.Y - style.WindowPadding.Y));
                ImGui.BeginTooltip();
                ImGui.Text(sValue.ToString());
                ImGui.EndTooltip();
            }

            return value_changed;
        }

        private bool SliderFunctionCycle(string id, ref float sValue, float vMax)
        {
            var style = ImGui.GetStyle();

            float radiusOuter = 20.0f;
            var pos = ImGui.GetCursorScreenPos();
            var center = new ImVec2(pos.X + radiusOuter, pos.Y + radiusOuter);
            float lineHeight = ImGui.GetTextLineHeight();
            var drawList = ImGui.GetWindowDrawList();

            float ANGLE_MIN = Mathf.Deg2Rad * 90;
            float ANGLE_MAX = Mathf.Deg2Rad * 450;

            ImGui.InvisibleButton(id, new ImVec2(radiusOuter * 2, radiusOuter * 2 + lineHeight + style.ItemInnerSpacing.Y));
            bool value_changed = false;
            bool is_active = ImGui.IsItemActive();
            bool is_hovered = ImGui.IsItemHovered();

            if (is_active && ImGui.GetIO().MouseDelta.X != 0.0f)
            {
                float step = (vMax - -vMax) / 200.0f;
                sValue += ImGui.GetIO().MouseDelta.X * step;
                if (sValue < -vMax) sValue = vMax;
                if (sValue > vMax) sValue = -vMax;
                value_changed = true;
            }

            float t = (sValue - 0) / (vMax - 0);
            float angle = ANGLE_MIN + (ANGLE_MAX - ANGLE_MIN) * t;
            float angle_cos = Mathf.Cos(angle), angle_sin = Mathf.Sin(angle);
            float radius_inner = radiusOuter * 0.40f;
            drawList.AddCircleFilled(center, radiusOuter, ImGui.GetColorU32(ImGuiCol.FrameBg), 16);
            drawList.AddLine(new ImVec2(center.X + angle_cos * radius_inner, center.Y + angle_sin * radius_inner), new ImVec2(center.X + angle_cos * (radiusOuter - 2), center.Y + angle_sin * (radiusOuter - 2)), ImGui.GetColorU32(ImGuiCol.SliderGrabActive), 2.0f);
            drawList.AddCircleFilled(center, radius_inner, ImGui.GetColorU32(is_active ? ImGuiCol.FrameBgActive : is_hovered ? ImGuiCol.FrameBgHovered : ImGuiCol.FrameBg), 16);
            drawList.AddText(new ImVec2(pos.X, pos.Y + radiusOuter * 2 + style.ItemInnerSpacing.Y), ImGui.GetColorU32(ImGuiCol.Text), id);

            if (is_active || is_hovered)
            {
                ImGui.SetNextWindowPos(new ImVec2(pos.X - style.WindowPadding.X, pos.Y - lineHeight - style.ItemInnerSpacing.Y - style.WindowPadding.Y));
                ImGui.BeginTooltip();
                ImGui.Text(sValue.ToString());
                ImGui.EndTooltip();
            }

            return value_changed;
        }
    }
}
