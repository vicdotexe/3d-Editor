using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eli.Ecs3D.ImGuiTools.TypeInspectors;
using Eli.Ecs3D.Terrain;
using ImGuiNET;

namespace Eli.Ecs3D.ImGuiTools
{
    public class TerrainEditor
    {
        public HeightMap HeightMap;

        public TerrainEditor(HeightMap heightMap)
        {
            HeightMap = heightMap;
        }

        public int Buttons;
        public int Texture;
        public void DrawImgui()
        {
            ImGui.Begin("Terrain Editor");
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
            ImGui.DragInt("CursorSize", ref TerrainComponent.StaticHeightMap.CursorSize, 1);
            ImGui.DragInt("CursorStrength", ref TerrainComponent.StaticHeightMap.CursorStrength, 1);
            ImGui.End();
            //TerrainComponent.EditType = (TerrainActions) Buttons;
            //TerrainComponent.PaintTextureId = Texture;
        }
    }
}
