using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eli.ImGuiTools;
using Eli.Textures;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Num = System.Numerics;
namespace Eli.Ecs3D.ImGuiTools
{
    public class ModelDrop
    {
        public string path;
    }

    public class ModelAssetsInspector
    {
        private NezContentManager FinalContent;
        private NezContentManager Content;
        private List<Texture2D> _atlasTextures = new List<Texture2D>();
        private Camera3D _camera = new Camera3D();
        private List<string> _names = new List<string>();
        private List<Model> _models = new List<Model>();
        private int iSize = 300;
        public static Dictionary<string, string> _paths = new Dictionary<string, string>();
        public static Dictionary<string, Sprite> _spritesByName = new Dictionary<string, Sprite>();
        private Dictionary<int, BoundingBox> _bounds = new Dictionary<int, BoundingBox>();
        private Viewport vp = new Viewport(0, 0, 300, 300);
        private RasterizerState rasterstate;
        private static Dictionary<IntPtr, Texture2D> _ids = new Dictionary<IntPtr, Texture2D>();
        private static Dictionary<Texture2D, IntPtr> _idsByTexture = new Dictionary<Texture2D, IntPtr>();
        public static string _currentPath;

        public ModelAssetsInspector()
        {
            GatherData();
            DrawToRenderTarget();

        }


        private void LoadModel()
        {
            if (EditorGui.ActiveEntity != null)
            {
                var entity = (Entity) EditorGui.ActiveEntity;
                if (entity.HasComponent<ModelComponent>())
                {
                    //entity.GetComponent<ModelComponent>().SetModel();
                }
            }
        }
        private bool isOpen = true;
        public void DrawGui()
        {
            //ImGui.SetNextWindowSizeConstraints(new Num.Vector2(300, 300), new Num.Vector2(1080, 1920));
            //ImGui.Begin("modelPicker", ref isOpen, ImGuiWindowFlags.HorizontalScrollbar);
            int numWide = (int)ImGui.GetWindowWidth();
            int numHigh = (int)ImGui.GetWindowHeight();

            var num = 0;
            string prevName;
            foreach (var name in _paths.Keys)
            {
                var rect = _spritesByName[name].Uvs;
                ImGui.PushID(name);
                if (ImGui.ImageButton(_idsByTexture[_spritesByName[name].Texture2D], new Num.Vector2(100, 100),
                    new Num.Vector2(rect.Left, rect.Top), new Num.Vector2(rect.Right, rect.Bottom)))
                {
                    _currentPath = _paths[name];
                    //LoadModelFile();

                    ///testDrag


                }
                if (ImGui.IsItemHovered())
                {
                    if (ImGui.IsMouseDown(0))
                    {
                        ImGuiManager.StartDragging(Core.Scene.Content.LoadModel(_paths[name]));
                    }
                    // ImGui.GetDragDropPayload()
                    //ImGui.SetDragDropPayload()
                }

                if (ImGui.GetContentRegionAvail().X - 300 < num * (100 ) )
                {
                    ImGui.NewLine();

                    num = 0;
                }
                else
                {
                    ImGui.SameLine(0);
                    num++;
                }
                prevName = name;
            }

            /*
            // If the mouse is pressed on the item, prepare it for dragging.
            
            if (ui->hover == item_id && ui->left_mouse_pressed)
                ui->prepare_drag = item_id;

            // If the mouse button is lifted, abort dragging.
            if (!ui->left_mouse_is_down)
                ui->prepare_drag = 0;

            // If this item was clicked on (prepared for dragging), but the mouse is now
            // outside the item -- start dragging.
            if (ui->prepare_drag == item_id && ui->hover != item_id)
            {
                tm_drag_api->start_dragging(object_id);
                ui->prepare_drag = 0;
            }
            //ImGui.BeginChildFrame(1500, new System.Numerics.Vector2(200, 200));
            //ImGui.EndChildFrame();
            //ImGui.End();
            */
            
        }
        public void GatherData()
        {
            Content = new NezContentManager();
            FinalContent = new NezContentManager();
            var _model = Content.LoadBasicModel(BasicModel.Cone);
            var directory = Content.GetPathForLoadedAsset(Content.LoadBasicModel(BasicModel.Cone)).Replace("Eli/Models/Cone", "Content/Eli/Models//Dungeon/");
            _camera.Position = new Vector3(16) * 10;
            _camera.Forward = new Vector3(-1);
            //_camera.ForceMatrixUpdate(1080,1920);

            var contentPath = Content.GetPathForLoadedAsset(Content.LoadBasicModel(BasicModel.Cone))
                .Replace("Eli/Models/Cone", "Content/");

            int count = 0;

            foreach (var file in Directory.GetFiles(contentPath, "*.xnb", SearchOption.AllDirectories))
            {
                var filePath = file.Replace("Content/", "").Replace(".xnb", "");
                var asset = Content.Load<object>(filePath);
                if (asset is Model)
                {
                    var name = new FileInfo(file).Name;
                    //_models.Add(asset as Model);
                    _names.Add(name);
                    _paths[name] = filePath;
                }
                else
                {
                    //Content.UnloadAsset<IDisposable>(file.Replace("Content/", "").Replace(".xnb", ""));
                }
                //_objects.Add(asset);
                //_objectNames.Add( (new FileInfo(file).Name));

            }

            Content.Unload();
            Content.Dispose();
            GC.Collect();
            foreach (var name in _names)
            {

                _models.Add(FinalContent.Load<Model>(_paths[name]));
                count++;
            }

            foreach (var file in Directory.GetFiles(directory))
            {
                //_models.Add(Content.LoadModel(file.Replace("Content/", "").Replace(".xnb","")));
                //var name = new FileInfo(file).Name;
                //_names.Add(name);


            }

            var list = new List<Model>();

            for (int f = 0; f < _models.Count; f++)
            {
                if (_models[f].Meshes.Count > 1)
                {
                    list.Add(_models[f]);
                }
                foreach (var mesh in _models[f].Meshes)
                {
                    foreach (var effect in mesh.Effects)
                    {
                        (effect as BasicEffect).EnableDefaultLighting();
                        (effect as BasicEffect).Alpha = 1;
                    }
                }
                _bounds[f] = BoundsHelper.CreateBoundingBox(_models[f], Matrix.Identity);

                var pixelCount = f * 300;

                count = 0;

            }


            //_renderTarget = RenderTarget.Create(iSize, _models.Count * iSize);
            _atlasTextures.Add(new Texture2D(Core.GraphicsDevice, 4096,4096));

            rasterstate = new RasterizerState()
            {
                ScissorTestEnable = true
            };

            int length = 4096 / iSize;

            var i = 0;
            count = 0;
            var renderTargetCount = 0;
            for (int x = 0; x < length; x++)
            {
                if (i >= _models.Count)
                    break;
                for (int y = 0; y < length; y++)
                {
                    if (i >= _models.Count)
                        break;
                    if (count >= ((4096 / iSize) * (4096 / iSize)) -1)
                    {
                        count = 0;
                        renderTargetCount++;
                        x = 0;
                        y = 0;
                        _atlasTextures.Add(new Texture2D(Core.GraphicsDevice, 4096, 4096));
                    }
                    _spritesByName[_names[i]] = new Sprite(_atlasTextures[renderTargetCount], new Rectangle(x * iSize, y * iSize, iSize, iSize));
                    i++;
                    count++;
                }
            }
        }

        private void DrawToRenderTarget()
        {
            var renderTarget = RenderTarget.GetTemporary(4096, 4096);
            int texCount = 0;
            foreach (Texture2D texture in _atlasTextures)
            {

                var id = ImGuiRenderer.Bind(texture);
                _ids[id] = texture;
                _idsByTexture[texture] = id;
                var oldViewPort = Core.GraphicsDevice.Viewport;
                var oldRas = Core.GraphicsDevice.RasterizerState;
                Core.GraphicsDevice.SetRenderTarget(renderTarget);
                Core.GraphicsDevice.BlendState = BlendState.Opaque;
                Core.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                //Core.GraphicsDevice.RasterizerState = rasterstate;
                //Core.GraphicsDevice.SetRenderTarget(tempTarget);

                Core.GraphicsDevice.Clear(Color.CornflowerBlue);
                Core.GraphicsDevice.Viewport = vp;

                int length = 4096 / iSize;

                var i = 0;
                for (int x = 0; x < length; x++)
                {
                    if (i >= _models.Count)
                        break;
                    for (int y = 0; y < length; y++)
                    {
                        if (i >= _models.Count)
                            break;
                        vp.Y = y * iSize;
                        vp.X = x * iSize;
                        Core.GraphicsDevice.Viewport = vp;

                        float cameraDistance = 0.02f; // Constant factor
                        Vector3 objectSizes = _bounds[i].Extents() * 2;
                        float objectSize = Mathf.Max(objectSizes.X, objectSizes.Y, objectSizes.Z);
                        float cameraView =
                            (float) (2.0f * Math.Tan(0.5f * Mathf.Deg2Rad *
                                                     MathHelper.PiOver4)); // Visible height 1 meter in front
                        float distance =
                            cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
                        distance += 0.5f * objectSize; // Estimated offset from the center to the outside of the object
                        _camera.Position = -_bounds[i].Center() - distance * _camera.Forward;
                        _camera.AspectRatio = 1;

                        foreach (var mesh in _models[i].Meshes)
                        foreach (var effect in mesh.Effects)
                        {
                            var be = effect as BasicEffect;
                            be.SpecularColor = be.DiffuseColor;
                        }
                        _models[i].Draw(Matrix.CreateTranslation(-_bounds[i].Center()), _camera.ViewMatrix,
                            _camera.ProjectionMatrix);
                        i++;

                    }
                }
                Color[] texdata = new Color[renderTarget.Width * renderTarget.Height];
                renderTarget.GetData(texdata);
                _atlasTextures[texCount].SetData(texdata);
                texCount++;
            }
        }
    }
}
