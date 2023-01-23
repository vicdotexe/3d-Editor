#region Using Statements
using System;
using System.Collections;
using System.Diagnostics;
using Eli.Analysis;
using Eli.BitmapFonts;
using Eli.Console;
using Eli.ECS;
using Eli.Systems;
using Eli.Textures;
using Eli.Timers;
using Eli.Tweens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Eli
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Core : Game
    {
        
        /// <summary>
        /// core emitter. emits only Core level events.
        /// </summary>
        public static Emitter<CoreEvents> Emitter;

        /// <summary>
        /// enables/disables if we should quit the app when escape is pressed
        /// </summary>
        public static bool ExitOnEscapeKeypress = true;

        /// <summary>
        /// enables/disables pausing when focus is lost. No update or render methods will be called if true when not in focus.
        /// </summary>
        public static bool PauseOnFocusLost = true;

        /// <summary>
        /// enables/disables debug rendering
        /// </summary>
        public static bool DebugRenderEnabled = false;
#if DEBUG
        internal static long drawCalls;
        TimeSpan _frameCounterElapsedTime = TimeSpan.Zero;
        int _frameCounter = 0;
        string _windowTitle;
#endif

        public static Color BackBufferClearColor = Color.Black;
        internal static Core _instance;
        public static Core Instance => _instance;
        public new static NezGlobalContentManager Content;
        public new static GraphicsDevice GraphicsDevice;
        ITimer _graphicsDeviceChangeTimer;
        public static SamplerState DefaultSamplerState = SamplerState.AnisotropicClamp;

        public static Color LetterBoxColor;

        // globally accessible systems
        FastList<GlobalManager> _globalManagers = new FastList<GlobalManager>();
        CoroutineManager _coroutineManager = new CoroutineManager();
        TimerManager _timerManager = new TimerManager();
        
        private IScene _scene;
        private IScene _nextScene;

        public static IFinalRenderDelegate FinalRenderDelegate
        {
            set
            {
                if (_finalRenderDelegate != null)
                    _finalRenderDelegate.Unload();

                _finalRenderDelegate = value;

                //if (_finalRenderDelegate != null)
                   // _finalRenderDelegate.OnAddedToScene(this);
            }
            get => _finalRenderDelegate;
        }

        static IFinalRenderDelegate _finalRenderDelegate;

        /// <summary>
        /// The currently active Scene. Note that if set, the Scene will not actually change until the end of the Update
        /// </summary>
        public static IScene Scene
        {
            get => _instance._scene;
            set
            {
                Insist.IsNotNull(value, "Scene cannot be null!");

                // handle our initial Scene. If we have no Scene and one is assigned directly wire it up
                if (_instance._scene == null)
                {
                    _instance._scene = value;
                    _instance._scene.Begin();
                    _instance.OnSceneChanged();
                }
                else
                {
                    _instance._nextScene = value;
                }
            }
        }


        public Core(int width = 1280, int height = 720, bool isFullScreen = false, bool enableEntitySystems = true,
            string windowTitle = "Nez", string contentDirectory = "Content")
        {
#if DEBUG
            _windowTitle = windowTitle;
#endif
            LetterBoxColor = Color.Black;
            _instance = this;
            Emitter = new Emitter<CoreEvents>();

            var graphicsManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = width,
                PreferredBackBufferHeight = height,
                IsFullScreen = isFullScreen,
                SynchronizeWithVerticalRetrace = true
            };
            graphicsManager.DeviceReset += OnGraphicsDeviceReset;
            graphicsManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;

            Screen.Initialize(graphicsManager);
            Window.ClientSizeChanged += OnGraphicsDeviceReset;
            Window.OrientationChanged += OnOrientationChanged;

            base.Content.RootDirectory = contentDirectory;
            Content = new NezGlobalContentManager(Services, base.Content.RootDirectory);
            IsMouseVisible = true;
            IsFixedTimeStep = false;

            // setup systems
            RegisterGlobalManager(_coroutineManager);
            RegisterGlobalManager(new TweenManager());
            RegisterGlobalManager(_timerManager);
            RegisterGlobalManager(new RenderTarget());
        }

        #region CoreEvents
        void OnOrientationChanged(object sender, EventArgs e)
        {
            Emitter.Emit(CoreEvents.OrientationChanged);
        }

        /// <summary>
        /// this gets called whenever the screen size changes
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        protected void OnGraphicsDeviceReset(object sender, EventArgs e)
        {
            // we coalese these to avoid spamming events
            if (_graphicsDeviceChangeTimer != null)
            {
                _graphicsDeviceChangeTimer.Reset();
            }
            else
            {
                _graphicsDeviceChangeTimer = Schedule(0.05f, false, this, t =>
                {
                    (t.Context as Core)._graphicsDeviceChangeTimer = null;
                    Emitter.Emit(CoreEvents.GraphicsDeviceReset);
                });
            }
        }
        #endregion

        #region Passthroughs to Game

        public new static void Exit()
        {
            ((Game)_instance).Exit();
        }

        #endregion
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //base.Initialize();
            GraphicsDevice = base.GraphicsDevice;
            var font = Content.LoadBitmapFont("DefaultContent/Eli/Fonts/NezDefaultBMFont.fnt");
            Graphics.Instance = new Graphics(font);
            base.Initialize();
            //test = Content.Load<Model>("Eli/Models/Cone");
        }

        private Model test;
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            //TODO: use this.Content to load your game content here 
        }


        protected override void Update(GameTime gameTime)
        {
            // For Mobile devices, this logic will close the Game when the Back button is pressed
            if (PauseOnFocusLost && !IsActive)
            {
                SuppressDraw();
                return;
            }

           
            StartDebugUpdate();

            // update all our systems and global managers
            Time.Update((float)gameTime.ElapsedGameTime.TotalSeconds, gameTime);
            Input.Update();

            if (ExitOnEscapeKeypress &&
                (Input.IsKeyDown(Keys.Escape) || Input.GamePads[0].IsButtonReleased(Buttons.Back)))
            {
                base.Exit();
                return;
            }
           
            if (_scene != null)
            {
                for (var i = _globalManagers.Length - 1; i >= 0; i--)
                {
                    if (_globalManagers.Buffer[i].Enabled)
                        _globalManagers.Buffer[i].Update();
                }

                //ToDo: Implement SceneTransition
                // we set the RenderTarget here so that the Viewport will match the RenderTarget properly
                Core.GraphicsDevice.SetRenderTarget(_scene.SceneRenderTarget);
                _scene.Update();

                if (_nextScene != null)
                {
                    _scene.End();

                    _scene = _nextScene;
                    _nextScene = null;
                    OnSceneChanged();

                    _scene.Begin();
                }
            }
            EndDebugUpdate();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(BackBufferClearColor);
            if (PauseOnFocusLost && !IsActive)
                return;

            StartDebugDraw(gameTime.ElapsedGameTime);

            if (_scene != null)
            {
                _scene.Render();
                // render as usual if we dont have an active SceneTransition
                FinalRenderToBuffer(_scene.PostRender(), null);
            }
#if DEBUG
            if (DebugRenderEnabled)
                Debug.Render();
#endif


            EndDebugDraw();
        }

        private void FinalRenderToBuffer(RenderTarget2D currentRenderTarget, RenderTarget2D finalRenderTarget)
        {
            // render our final result to the backbuffer or let our delegate do so
            if (_finalRenderDelegate != null)
            {
                _finalRenderDelegate.HandleFinalRender(finalRenderTarget, LetterBoxColor, currentRenderTarget, _scene.RenderDestination, DefaultSamplerState);
            }
            else
            {
                Core.GraphicsDevice.SetRenderTarget(finalRenderTarget);
                Core.GraphicsDevice.Clear(LetterBoxColor);

                Graphics.Instance.Batcher.Begin(BlendState.Opaque, DefaultSamplerState, null, null);
                Graphics.Instance.Batcher.Draw(currentRenderTarget, _scene.RenderDestination, Color.White);
                Graphics.Instance.Batcher.End();
            }
        }

        /// <summary>
        /// Called after a Scene ends, before the next Scene begins
        /// </summary>
        void OnSceneChanged()
        {
            Emitter.Emit(CoreEvents.SceneChanged);
            Time.SceneChanged();
            GC.Collect();
        }

        #region Debug Injection

        [Conditional("DEBUG")]
        void StartDebugUpdate()
        {
#if DEBUG
            TimeRuler.Instance.StartFrame();
            TimeRuler.Instance.BeginMark("update", Color.Green);
#endif
        }

        [Conditional("DEBUG")]
        void EndDebugUpdate()
        {
#if DEBUG
            TimeRuler.Instance.EndMark("update");
            DebugConsole.Instance.Update();
            drawCalls = 0;
#endif
        }

        [Conditional("DEBUG")]
        void StartDebugDraw(TimeSpan elapsedGameTime)
        {
#if DEBUG
            TimeRuler.Instance.BeginMark("draw", Color.Gold);

            // fps counter
            _frameCounter++;
            _frameCounterElapsedTime += elapsedGameTime;
            if (_frameCounterElapsedTime >= TimeSpan.FromSeconds(1))
            {
                var totalMemory = (GC.GetTotalMemory(false) / 1048576f).ToString("F");
                Window.Title = string.Format("{0} {1} fps - {2} MB", _windowTitle, _frameCounter, totalMemory);
                _frameCounter = 0;
                _frameCounterElapsedTime -= TimeSpan.FromSeconds(1);
            }
#endif
        }

        [Conditional("DEBUG")]
        void EndDebugDraw()
        {
#if DEBUG
            TimeRuler.Instance.EndMark("draw");
            DebugConsole.Instance.Render();

            // the TimeRuler only needs to render when the DebugConsole is not open
            if (!DebugConsole.Instance.IsOpen)
                TimeRuler.Instance.Render();

#if !FNA
            drawCalls = GraphicsDevice.Metrics.DrawCount;
#endif
#endif
        }

        #endregion

        #region Global Managers

        /// <summary>
        /// adds a global manager object that will have its update method called each frame before Scene.update is called
        /// </summary>
        /// <returns>The global manager.</returns>
        /// <param name="manager">Manager.</param>
        public static void RegisterGlobalManager(GlobalManager manager)
        {
            _instance._globalManagers.Add(manager);
            manager.Enabled = true;
        }

        /// <summary>
        /// removes the global manager object
        /// </summary>
        /// <returns>The global manager.</returns>
        /// <param name="manager">Manager.</param>
        public static void UnregisterGlobalManager(GlobalManager manager)
        {
            _instance._globalManagers.Remove(manager);
            manager.Enabled = false;
        }

        /// <summary>
        /// gets the global manager of type T
        /// </summary>
        /// <returns>The global manager.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T GetGlobalManager<T>() where T : GlobalManager
        {
            for (var i = 0; i < _instance._globalManagers.Length; i++)
            {
                if (_instance._globalManagers.Buffer[i] is T)
                    return _instance._globalManagers.Buffer[i] as T;
            }

            return null;
        }

        #endregion


        #region Systems access

        /// <summary>
        /// starts a coroutine. Coroutines can yeild ints/floats to delay for seconds or yeild to other calls to startCoroutine.
        /// Yielding null will make the coroutine get ticked the next frame.
        /// </summary>
        /// <returns>The coroutine.</returns>
        /// <param name="enumerator">Enumerator.</param>
        public static ICoroutine StartCoroutine(IEnumerator enumerator)
        {
            return _instance._coroutineManager.StartCoroutine(enumerator);
        }

        /// <summary>
        /// schedules a one-time or repeating timer that will call the passed in Action
        /// </summary>
        /// <param name="timeInSeconds">Time in seconds.</param>
        /// <param name="repeats">If set to <c>true</c> repeats.</param>
        /// <param name="context">Context.</param>
        /// <param name="onTime">On time.</param>
        public static ITimer Schedule(float timeInSeconds, bool repeats, object context, Action<ITimer> onTime)
        {
            return _instance._timerManager.Schedule(timeInSeconds, repeats, context, onTime);
        }

        /// <summary>
        /// schedules a one-time timer that will call the passed in Action after timeInSeconds
        /// </summary>
        /// <param name="timeInSeconds">Time in seconds.</param>
        /// <param name="context">Context.</param>
        /// <param name="onTime">On time.</param>
        public static ITimer Schedule(float timeInSeconds, object context, Action<ITimer> onTime)
        {
            return _instance._timerManager.Schedule(timeInSeconds, false, context, onTime);
        }

        /// <summary>
        /// schedules a one-time or repeating timer that will call the passed in Action
        /// </summary>
        /// <param name="timeInSeconds">Time in seconds.</param>
        /// <param name="repeats">If set to <c>true</c> repeats.</param>
        /// <param name="onTime">On time.</param>
        public static ITimer Schedule(float timeInSeconds, bool repeats, Action<ITimer> onTime)
        {
            return _instance._timerManager.Schedule(timeInSeconds, repeats, null, onTime);
        }

        /// <summary>
        /// schedules a one-time timer that will call the passed in Action after timeInSeconds
        /// </summary>
        /// <param name="timeInSeconds">Time in seconds.</param>
        /// <param name="onTime">On time.</param>
        public static ITimer Schedule(float timeInSeconds, Action<ITimer> onTime)
        {
            return _instance._timerManager.Schedule(timeInSeconds, false, null, onTime);
        }

        #endregion
    }
}
