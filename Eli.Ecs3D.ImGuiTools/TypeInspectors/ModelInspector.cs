using System.Collections.Generic;
using System.IO;
using Eli.Ecs3D.Components.Renderable.Materials;
using Eli.Ecs3D.ImGuiTools.ObjectInspectors;
using Eli.ImGuiTools;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using Num = System.Numerics;
namespace Eli.Ecs3D.ImGuiTools.TypeInspectors
{
    public class ModelInspector : AbstractTypeInspector
    {
        private string _currentPath;
        private int _currentModel;
        private Model _model;
        private Dictionary<int,List<AbstractTypeInspector>> _effectsInspector = new Dictionary<int,List<AbstractTypeInspector>>();

        public override void Initialize()
        {
            base.Initialize();
            var model = GetValue<Model>();
            _model = model;
            _currentPath = Core.Scene.Content.GetPathForLoadedAsset(model);

            var effects = ListPool<Material3D>.Obtain();

            foreach (var mesh in model.Meshes)
            {
                foreach (var effect in mesh.Effects)
                {
                    if (!effects.Contains(effect.Tag as Material3D))
                    {
                        effects.Add(effect.Tag as Material3D);
                        _effectsInspector.Add(NezImGui.GetScopeId(),TypeInspectorUtils.GetInspectableProperties(effect.Tag));
                    }

                }

            }

            effects.Clear();
            ListPool<Material3D>.Free(effects);
        }

        public override void DrawReadOnly()
        {
            base.DrawReadOnly();
            

        }

        public override void DrawMutable()
        {
            // throw new NotImplementedException();
            var openFile = false;
            if (ImGui.Button("File: "+ Path.GetFileName(_currentPath)))
            {
                //openFile = true;
                LoadModelFile();
            }

            if (openFile)
                OpenFilePopup();
            foreach (var inspector in _effectsInspector)
            {
                foreach (var ins in inspector.Value)
                {
                    ImGui.PushID(inspector.Key);
                    ins.DrawMutable();
                    ImGui.PopID();
                }

            }

            //OpenFilePopup();
            //ShowList();
        }

        void ShowList()
        {


        }
        void OpenFilePopup()
        {
            var isOpen = true;
            ImGui.SetNextWindowSizeConstraints(new Num.Vector2(300,300), new Num.Vector2(1080,1920));
            ImGui.Begin("modelPicker", ref isOpen,ImGuiWindowFlags.HorizontalScrollbar);
            int numWide = (int) ImGui.GetWindowWidth();
            int numHigh = (int) ImGui.GetWindowHeight();

            var num = 0;
                foreach (var name in ModelViewerWindow._paths.Keys)
                {
                    var rect = ModelViewerWindow._spritesByName[name].Uvs;
                    ImGui.PushID(name);
                    if (ImGui.ImageButton(ModelViewerWindow._id, new Num.Vector2(100, 100),
                        new Num.Vector2(rect.Left, rect.Top), new Num.Vector2(rect.Right, rect.Bottom)))
                    {
                        
                        LoadModelFile();
                        
                    }

                    if (ImGui.GetContentRegionAvail().X < num * (100+(num * ImGui.GetStyle().FramePadding.X) + 50))
                    {
                        ImGui.NewLine();
                        
                        num = 0;
                    }
                    else
                    {
                        ImGui.SameLine(0);
                        num++;
                }

                }
                //ImGui.BeginChildFrame(1500, new System.Numerics.Vector2(200, 200));
                //ImGui.EndChildFrame();
            ImGui.End();
                /*
                if (ImGui.BeginPopupModal("open-file", ref isOpen, ImGuiWindowFlags.NoTitleBar))
                {
                    var picker = FilePicker.GetFilePicker(this, Path.Combine(Environment.CurrentDirectory, "Content"), ".xnb");
                    picker.DontAllowTraverselBeyondRootFolder = true;
                    if (picker.Draw())
                    {
                        var file = picker.SelectedFile;
                        _currentPath = file.Replace(".xnb", "");
    
                        LoadModelFile();
                        FilePicker.RemoveFilePicker(this);
                    }
                    ImGui.EndPopup();
                }
                */
        }

        void LoadModelFile()
        {
            SetValue(Core.Scene.Content.LoadModel(ModelAssetsInspector._currentPath));
        }
    }

    
}
