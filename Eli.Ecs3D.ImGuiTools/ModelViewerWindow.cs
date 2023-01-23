using System;
using System.Collections.Generic;
using System.IO;
using Eli.ImGuiTools;
using Eli.Textures;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Num = System.Numerics;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Eli.Ecs3D.ImGuiTools
{
    public class ModelViewerWindow
    {
        private NezContentManager FinalContent;
        private NezContentManager Content;
        private RenderTarget2D _renderTarget;
        private Model _model;
        public bool ShowWindow = true;
        public static IntPtr _id;
        private bool _bound = false;
        private Camera3D _camera = new Camera3D();
        private int _width = 300;
        private int _height = 300;
        private Num.Vector2 size = new Num.Vector2(300,300);
        private List<Model> _models = new List<Model>();
        private List<string> _names = new List<string>();
        private List<RenderTarget2D> _textures = new List<RenderTarget2D>();
        private List<object> _objects = new List<object>();
        private List<string> _objectNames = new List<string>();
        private int iSize = 300;
        public static Dictionary<string, string> _paths = new Dictionary<string, string>();
        public static Dictionary<string, Sprite> _spritesByName = new Dictionary<string, Sprite>();

        public ModelViewerWindow()
        {
            Content = new NezContentManager();
            FinalContent = new NezContentManager();
            _model = Content.LoadBasicModel(BasicModel.Cone);
            var directory = Content.GetPathForLoadedAsset(Content.LoadBasicModel(BasicModel.Cone)).Replace("Eli/Models/Cone", "Content/Eli/Models//Dungeon/");
            _camera.Position = new Vector3(16) * 10;
            _camera.Forward = new Vector3(-1);
            //_camera.ForceMatrixUpdate(1080,1920);

            var contentPath = Content.GetPathForLoadedAsset(Content.LoadBasicModel(BasicModel.Cone))
                .Replace("Eli/Models/Cone", "Content/");

            int count = 0;

            foreach (var file in Directory.GetFiles(contentPath, "*.xnb",SearchOption.AllDirectories))
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
            _renderTarget = RenderTarget.Create(4096, 4096);

            rasterstate = new RasterizerState()
            {
                ScissorTestEnable = true
            };

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

                    _spritesByName[_names[i]] = new Sprite(_renderTarget, new Rectangle(x*iSize, y*iSize, iSize,iSize));
                    i++;

                }
            }
        }

        private Dictionary<int, BoundingBox> _bounds = new Dictionary<int, BoundingBox>();
        private Viewport vp = new Viewport(0,0,300,300);
        private RasterizerState rasterstate;
        private void DrawToRenderTarget()
        {

            var oldViewPort = Core.GraphicsDevice.Viewport;
            var oldRas = Core.GraphicsDevice.RasterizerState;
            Core.GraphicsDevice.SetRenderTarget(_renderTarget);
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
                    float cameraView = (float)(2.0f * Math.Tan(0.5f * Mathf.Deg2Rad * MathHelper.PiOver4)); // Visible height 1 meter in front
                    float distance = cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
                    distance += 0.5f * objectSize; // Estimated offset from the center to the outside of the object
                    _camera.Position = -_bounds[i].Center() - distance * _camera.Forward;
                    _camera.AspectRatio = 1;

                    _models[i].Draw(Matrix.CreateTranslation(-_bounds[i].Center()), _camera.ViewMatrix, _camera.ProjectionMatrix);
                    i++;
  
                }
            }
            /*
            for (int i = 0; i < _models.Count; i++)
            {

                vp.Y = i * iSize;
                //Core.GraphicsDevice.ScissorRectangle = new Rectangle(0,i * iSize, 300,300);
                Core.GraphicsDevice.Viewport = vp;
                //Core.GraphicsDevice.Clear(Color.Purple);

                float cameraDistance = 0.02f; // Constant factor
                Vector3 objectSizes = _bounds[i].Extents() * 2;
                float objectSize = Mathf.Max(objectSizes.X, objectSizes.Y, objectSizes.Z);
                float cameraView = (float)(2.0f * Math.Tan(0.5f * Mathf.Deg2Rad * MathHelper.PiOver4)); // Visible height 1 meter in front
                float distance = cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
                distance += 0.5f * objectSize; // Estimated offset from the center to the outside of the object
                _camera.Position  = -_bounds[i].Center() - distance * _camera.CameraForward;
                _camera.ForceMatrixUpdate(300,300);

                _models[i].Draw(Matrix.CreateTranslation(-_bounds[i].Center()), _camera.ViewMatrix, _camera.ProjectionMatrix);
            }
            */
            /*
            //Core.GraphicsDevice.Clear(Color.Purple);
            Core.GraphicsDevice.BlendState = BlendState.Opaque;


            
            for (int i = 0; i < _models.Count; i++)
            {
                vp.Y = i * iSize;
                Graphics.Instance.Batcher.Begin();
                //Graphics.Instance.Batcher.Draw(_textures[i], new Vector2(0, i * iSize));
                Graphics.Instance.Batcher.DrawString(Graphics.Instance.BitmapFont,_names[i], new Vector2(0, iSize * 0.08f + (i * iSize)), Color.Yellow, 0,Vector2.Zero, new Vector2(3),SpriteEffects.None, 1 );
                Graphics.Instance.Batcher.End();
            }

            
            Core.GraphicsDevice.Viewport = oldViewPort;
            */
            Core.GraphicsDevice.Viewport = oldViewPort;
            //Core.GraphicsDevice.RasterizerState = oldRas;
        }

        public void Draw()
        {
            
            Show(ref ShowWindow);
        }
        private void Show(ref bool isOpen)
        {
            if (!isOpen)
                return;
            if (!_bound)
            {
                _id = ImGuiRenderer.Bind(_renderTarget);
                _bound = true;
                DebugHelper.AddOnUpdate(delegate
                {
                    _camera.MoveForward(60 * Ps4Input.Main.LeftStick.Value.Y * Time.DeltaTime);
                    _camera.Strafe(-60 * Ps4Input.Main.LeftStick.Value.X * Time.DeltaTime);

                    _camera.Pitch(-90 * Ps4Input.Main.RightStick.Value.Y * Time.DeltaTime);
                    _camera.Yaw(-90 * Ps4Input.Main.RightStick.Value.X * Time.DeltaTime);
                });
            }
            DrawToRenderTarget();
            ImGui.SetNextWindowPos(new Num.Vector2(100, 100), ImGuiCond.Appearing);
            ImGui.SetNextWindowSize(new Num.Vector2(500, 500), ImGuiCond.Appearing);
            ImGui.SetNextWindowContentSize(new Num.Vector2(2048, 2048));
            ImGui.Begin("Model Viewer", ref isOpen);
            ImGui.ImageButton(_id, ImGui.GetContentRegionAvail());
            ImGui.End();
        }
    }
}
