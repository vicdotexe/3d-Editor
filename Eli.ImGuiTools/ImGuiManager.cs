using System;
using System.Collections.Generic;
using System.Text;
using Eli.ECS;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.ImGui;
using Num = System.Numerics;

namespace Eli.ImGuiTools
{
	public partial class ImGuiManager : GlobalManager
	{
        public bool ShowCoreWindow = true;
        public bool ShowSeperateGameWindow = true;
        public bool ShowMenuBar = true;

        public bool UseCustomLayout = false;

		private ImGuiRenderer _renderer;
        List<Type> _sceneSubclasses = new List<Type>();
        Num.Vector2? _gameViewForcedSize;
        WindowPosition? _gameViewForcedPos;
        RenderTarget2D _lastRenderTarget;
        IntPtr _renderTargetId = IntPtr.Zero;
        protected float _mainMenuBarHeight;
        List<Action> _drawCommands = new List<Action>();

		CoreWindow _coreWindow = new CoreWindow();

        Num.Vector2 _gameWindowFirstPosition;
        string _gameWindowTitle;

        public RectangleF GameWindowRect;

        public string GameWindowTitle => _gameWindowTitle;
        ImGuiWindowFlags _gameWindowFlags = 0;

        public void AddCustomDrawCall(Action action)
        {
            _drawCommands.Add(action);
        }

        public virtual void AdjustGameWindow() {}

        public static object DraggedObject;
        public static bool IsDragging => DraggedObject != null;
        public static void StartDragging(object context)
        {
            DraggedObject = context;
        }

        public static void StopDragging()
        {
            DraggedObject = null;
        }

		public ImGuiManager(bool useCustomLayout, ImGuiOptions options = null)
        {
            if (options == null)
                options = new ImGuiOptions();
            _gameWindowFirstPosition = options._gameWindowFirstPosition;
            _gameWindowTitle = options._gameWindowTitle;
            _gameWindowFlags = options._gameWindowFlags;

			_renderer = new ImGuiRenderer(Core.Instance);
            _renderer.RebuildFontAtlas(options);
            Core.Emitter.AddObserver(CoreEvents.SceneChanged, OnSceneChanged);
			// find all Scenes
			_sceneSubclasses = ReflectionUtils.GetAllSubclasses(typeof(IScene), true);
        }

        /// <summary>
        /// this is where we issue any and all ImGui commands to be drawn
        /// </summary>
        void LayoutGui()
        {
			
            if (ShowMenuBar)
                DrawMainMenuBar();



            if (ShowSeperateGameWindow)
                DrawGameWindow();

            //DrawEntityInspectors();

            for (var i = _drawCommands.Count - 1; i >= 0; i--)
                _drawCommands[i]();

            //_sceneGraphWindow.Show(ref ShowSceneGraphWindow);
            _coreWindow.Show(ref ShowCoreWindow);


			/*
            if (_spriteAtlasEditorWindow != null)
            {
                if (!_spriteAtlasEditorWindow.Show())
                    _spriteAtlasEditorWindow = null;
            }

            if (ShowDemoWindow)
                ImGui.ShowDemoWindow(ref ShowDemoWindow);

            if (ShowStyleEditor)
            {
                ImGui.Begin("Style Editor", ref ShowStyleEditor);
                ImGui.ShowStyleEditor();
                ImGui.End();
            }
			*/
			if (!ImGui.IsMouseDown(0) && DraggedObject != null)
				StopDragging();
		}


		/// <summary>
		/// draws the main menu bar
		/// </summary>
		void DrawMainMenuBar()
		{
			if (ImGui.BeginMainMenuBar())
			{
				_mainMenuBarHeight = ImGui.GetWindowHeight();
				if (ImGui.BeginMenu("File"))
				{
					//if (ImGui.MenuItem("Open Sprite Atlas Editor"))
						//_spriteAtlasEditorWindow = _spriteAtlasEditorWindow ?? new SpriteAtlasEditorWindow();

					if (ImGui.MenuItem("Quit ImGui"))
						SetEnabled(false);
					ImGui.EndMenu();
				}

				if (_sceneSubclasses.Count > 0 && ImGui.BeginMenu("Scenes"))
				{
					foreach (var sceneType in _sceneSubclasses)
					{
						if (ImGui.MenuItem(sceneType.Name))
						{
							var scene = (IScene)Activator.CreateInstance(sceneType);
							//Core.StartSceneTransition(new FadeTransition(() => scene));
                            Core.Scene = scene;
                        }
					}

					ImGui.EndMenu();
				}


				if (ImGui.BeginMenu("Game View"))
				{
					var rtSize = Core.Scene.SceneRenderTargetSize;

					if (ImGui.BeginMenu("Resize"))
					{
						if (ImGui.MenuItem("0.25x"))
							_gameViewForcedSize = new Num.Vector2(rtSize.X / 4f, rtSize.Y / 4f);
						if (ImGui.MenuItem("0.5x"))
							_gameViewForcedSize = new Num.Vector2(rtSize.X / 2f, rtSize.Y / 2f);
						if (ImGui.MenuItem("0.75x"))
							_gameViewForcedSize = new Num.Vector2(rtSize.X / 1.33f, rtSize.Y / 1.33f);
						if (ImGui.MenuItem("1x"))
							_gameViewForcedSize = new Num.Vector2(rtSize.X, rtSize.Y);
						if (ImGui.MenuItem("1.5x"))
							_gameViewForcedSize = new Num.Vector2(rtSize.X * 1.5f, rtSize.Y * 1.5f);
						if (ImGui.MenuItem("2x"))
							_gameViewForcedSize = new Num.Vector2(rtSize.X * 2, rtSize.Y * 2);
						if (ImGui.MenuItem("3x"))
							_gameViewForcedSize = new Num.Vector2(rtSize.X * 3, rtSize.Y * 3);
						ImGui.EndMenu();
					}

					if (ImGui.BeginMenu("Reposition"))
					{
						foreach (var pos in Enum.GetNames(typeof(WindowPosition)))
						{
							if (ImGui.MenuItem(pos))
								_gameViewForcedPos = (WindowPosition)Enum.Parse(typeof(WindowPosition), pos);
						}

						ImGui.EndMenu();
					}


					ImGui.EndMenu();
				}

				if (ImGui.BeginMenu("Window"))
				{
					//ImGui.MenuItem("ImGui Demo Window", null, ref ShowDemoWindow);
					/*
					ImGui.MenuItem("Style Editor", null, ref ShowStyleEditor);
					if (ImGui.MenuItem("Open imgui_demo.cpp on GitHub"))
					{
						System.Diagnostics.Process.Start("https://github.com/ocornut/imgui/blob/master/imgui_demo.cpp");
					}
					*/
					ImGui.Separator();
					ImGui.MenuItem("Core Window", null, ref ShowCoreWindow);
					//ImGui.MenuItem("Scene Graph Window", null, ref ShowSceneGraphWindow);
					//ImGui.MenuItem("Separate Game Window", null, ref ShowSeperateGameWindow);
					ImGui.EndMenu();
				}

				ImGui.EndMainMenuBar();
			}
		}
	}
}
