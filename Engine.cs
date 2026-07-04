#nullable enable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Reflection;
using System.Runtime;

namespace Monocle {
    /// <summary>
    /// Core game engine class that manages the game loop, graphics, input, and scene management.
    /// Inherits from MonoGame's Game class and provides the foundation for all Monocle-based games.
    /// Modernized for .NET 9 with nullable reference types and improved patterns.
    /// </summary>
    public class Engine : Game {

        /// <summary>
        /// The title of the game window and application.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The version of the game application.
        /// </summary>
        public Version? Version { get; set; }

        // references
        /// <summary>
        /// The singleton instance of the Engine. Provides global access to the engine throughout the application.
        /// </summary>
        public static Engine? Instance { get; private set; }

        /// <summary>
        /// The MonoGame graphics device manager for handling graphics settings and device management.
        /// </summary>
        public static GraphicsDeviceManager? Graphics { get; private set; }

        /// <summary>
        /// The command system for handling debug commands and console functionality.
        /// </summary>
        public static Commands? Commands { get; private set; }

        /// <summary>
        /// The object pooler for efficient memory management and object reuse.
        /// </summary>
        public static Pooler? Pooler { get; private set; }

        /// <summary>
        /// Optional action to override the default game loop behavior.
        /// </summary>
        public static Action? OverloadGameLoop { get; set; }

        // screen size
        /// <summary>
        /// The logical width of the game screen in pixels.
        /// </summary>
        public static int Width { get; private set; }

        /// <summary>
        /// The logical height of the game screen in pixels.
        /// </summary>
        public static int Height { get; private set; }

        /// <summary>
        /// The actual viewport width after scaling and padding.
        /// </summary>
        public static int ViewWidth { get; private set; }

        /// <summary>
        /// The actual viewport height after scaling and padding.
        /// </summary>
        public static int ViewHeight { get; private set; }

        /// <summary>
        /// The padding around the viewport for maintaining aspect ratio.
        /// Setting this value will update the view immediately.
        /// </summary>
        public static int ViewPadding {
            get => viewPadding;
            set {
                viewPadding = value;
                Instance?.UpdateView();
            }
        }
        private static int viewPadding = 0;
        private static bool resizing;

        // time
        /// <summary>
        /// The time in seconds since the last frame, affected by TimeRate and FreezeTimer.
        /// </summary>
        public static float DeltaTime { get; private set; }

        /// <summary>
        /// The raw time in seconds since the last frame, unaffected by TimeRate or FreezeTimer.
        /// </summary>
        public static float RawDeltaTime { get; private set; }

        /// <summary>
        /// Multiplier for time scaling. 1.0 is normal speed, 0.5 is half speed, 2.0 is double speed.
        /// </summary>
        public static float TimeRate { get; set; } = 1f;

        /// <summary>
        /// Timer for freezing game time. While greater than 0, DeltaTime will be 0.
        /// </summary>
        public static float FreezeTimer { get; set; }

        /// <summary>
        /// Current frames per second, updated approximately once per second.
        /// </summary>
        public static int FPS { get; private set; }

        private TimeSpan counterElapsed = TimeSpan.Zero;
        private int fpsCounter = 0;

        // content directory
#if !CONSOLE
        private static readonly string AssemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
#endif

        /// <summary>
        /// Gets the full path to the content directory, handling platform-specific paths.
        /// </summary>
        public static string ContentDirectory {
            get {
#if PS4
                return Path.Combine("/app0/", Instance?.Content.RootDirectory ?? string.Empty);
#elif NSWITCH
                return Path.Combine("rom:/", Instance?.Content.RootDirectory ?? string.Empty);
#elif XBOXONE
                return Instance?.Content.RootDirectory ?? string.Empty;
#else
                return Path.Combine(AssemblyDirectory, Instance?.Content.RootDirectory ?? string.Empty);
#endif
            }
        }

        // util
        /// <summary>
        /// The background color used when clearing the screen each frame.
        /// </summary>
        public static Color ClearColor { get; set; }

        /// <summary>
        /// Whether the game should exit when the Escape key is pressed.
        /// </summary>
        public static bool ExitOnEscapeKeypress { get; set; }

        // scene
        /// <summary>
        /// The currently active scene being updated and rendered.
        /// </summary>
        private Scene? scene;

        /// <summary>
        /// The scene that will become active at the start of the next frame.
        /// </summary>
        private Scene? nextScene;

        /// <summary>
        /// Initializes a new instance of the Engine with the specified display settings.
        /// </summary>
        /// <param name="width">The logical width of the game screen.</param>
        /// <param name="height">The logical height of the game screen.</param>
        /// <param name="windowWidth">The initial window width in pixels.</param>
        /// <param name="windowHeight">The initial window height in pixels.</param>
        /// <param name="windowTitle">The title to display in the window title bar.</param>
        /// <param name="fullscreen">Whether to start in fullscreen mode.</param>
        /// <exception cref="ArgumentException">Thrown when width or height are not positive.</exception>
        /// <exception cref="ArgumentNullException">Thrown when windowTitle is null.</exception>
        public Engine(int width, int height, int windowWidth, int windowHeight, string windowTitle, bool fullscreen) {
            ArgumentNullException.ThrowIfNull(windowTitle);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(windowWidth);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(windowHeight);

            Instance = this;

            Title = Window.Title = windowTitle;
            Width = width;
            Height = height;
            ClearColor = Color.Black;

            Graphics = new GraphicsDeviceManager(this);
            Graphics.DeviceReset += OnGraphicsReset;
            Graphics.DeviceCreated += OnGraphicsCreate;
            Graphics.SynchronizeWithVerticalRetrace = true;
            Graphics.PreferMultiSampling = false;
            Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Graphics.PreferredBackBufferFormat = SurfaceFormat.Color;
            Graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;


#if PS4 || XBOXONE
            Graphics.PreferredBackBufferWidth = 1920;
            Graphics.PreferredBackBufferHeight = 1080;
#elif NSWITCH
            Graphics.PreferredBackBufferWidth = 1280;
            Graphics.PreferredBackBufferHeight = 720;
#else
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnClientSizeChanged;

            if (fullscreen) {
                Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                Graphics.IsFullScreen = true;
            } else {
                Graphics.PreferredBackBufferWidth = windowWidth;
                Graphics.PreferredBackBufferHeight = windowHeight;
                Graphics.IsFullScreen = false;
            }
#endif

            Content.RootDirectory = @"Content";

            IsMouseVisible = false;
            IsFixedTimeStep = false;
            ExitOnEscapeKeypress = true;

            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            Graphics.ApplyChanges();
        }

#if !CONSOLE
        /// <summary>
        /// Handles window resize events to update the graphics buffer and view settings.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnClientSizeChanged(object? sender, EventArgs e) {
            if (Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0 && !resizing) {
                resizing = true;

                if (Graphics != null) {
                    Graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                    Graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                }
                UpdateView();

                resizing = false;
            }
        }
#endif

        /// <summary>
        /// Handles graphics device reset events to restore graphics resources.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnGraphicsReset(object? sender, EventArgs e) {
            UpdateView();

            scene?.HandleGraphicsReset();
            if (nextScene != null && nextScene != scene)
                nextScene.HandleGraphicsReset();
        }

        /// <summary>
        /// Handles graphics device creation events to initialize graphics resources.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnGraphicsCreate(object? sender, EventArgs e) {
            UpdateView();

            scene?.HandleGraphicsCreate();
            if (nextScene != null && nextScene != scene)
                nextScene.HandleGraphicsCreate();
        }

        /// <summary>
        /// Called when the game window gains focus.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        protected override void OnActivated(object? sender, EventArgs args) {
            base.OnActivated(sender, args);

            if (scene != null)
                scene.GainFocus();
        }

        /// <summary>
        /// Called when the game window loses focus.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        protected override void OnDeactivated(object? sender, EventArgs args) {
            base.OnDeactivated(sender, args);

            scene?.LoseFocus();
        }

        protected override void Initialize() {
            base.Initialize();

            MInput.Initialize();
            Tracker.Initialize();
            Pooler = new Pooler();
            Commands = new Commands();
        }

        protected override void LoadContent() {
            base.LoadContent();

            Monocle.Draw.Initialize(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime) {
            RawDeltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
            DeltaTime = RawDeltaTime * TimeRate;

            //Update input
            MInput.Update();

#if !CONSOLE
            if (ExitOnEscapeKeypress && MInput.Keyboard.Pressed(Microsoft.Xna.Framework.Input.Keys.Escape)) {
                Exit();
                return;
            }
#endif

            if (OverloadGameLoop != null) {
                OverloadGameLoop();
                base.Update(gameTime);
                return;
            }

            //Update current scene
            if (FreezeTimer > 0)
                FreezeTimer = Math.Max(FreezeTimer - RawDeltaTime, 0);
            else if (scene != null) {
                scene.BeforeUpdate();
                scene.Update();
                scene.AfterUpdate();
            }

            //Debug Console
            if (Commands?.Open ?? false)
                Commands.UpdateOpen();
            else if (Commands?.Enabled ?? false)
                Commands.UpdateClosed();

            //Changing scenes
            if (scene != nextScene) {
                var lastScene = scene;
                if (scene != null)
                    scene.End();
                scene = nextScene;
                OnSceneTransition(lastScene, nextScene);
                if (scene != null)
                    scene.Begin();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            RenderCore();

            base.Draw(gameTime);
            if (Commands?.Open ?? false)
                Commands.Render();

            //Frame counter
            fpsCounter++;
            counterElapsed += gameTime.ElapsedGameTime;
            if (counterElapsed >= TimeSpan.FromSeconds(1)) {
#if DEBUG
                Window.Title = Title + " " + fpsCounter.ToString() + " fps - " + (GC.GetTotalMemory(false) / 1048576f).ToString("F") + " MB";
#endif
                FPS = fpsCounter;
                fpsCounter = 0;
                counterElapsed -= TimeSpan.FromSeconds(1);
            }
        }

        /// <summary>
        /// Override if you want to change the core rendering functionality of Monocle Engine.
        /// By default, this simply sets the render target to null, clears the screen, and renders the current Scene
        /// </summary>
        protected virtual void RenderCore() {
            if (scene != null)
                scene.BeforeRender();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Viewport = Viewport;
            GraphicsDevice.Clear(ClearColor);

            if (scene != null) {
                scene.Render();
                scene.AfterRender();
            }
        }

        /// <summary>
        /// Called when the game is exiting. Handles cleanup of input and other resources.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The exit event arguments.</param>
        protected override void OnExiting(object sender, EventArgs args) {
            base.OnExiting(sender, args);
            MInput.Shutdown();
        }

        public void RunWithLogging() {
            try {
                Run();
            } catch (Exception e) {
                ErrorLog.Write(e);
                ErrorLog.Open();
            }
        }

        #region Scene

        /// <summary>
        /// Called after a Scene ends, before the next Scene begins.
        /// </summary>
        /// <param name="from">The scene that is ending (can be null).</param>
        /// <param name="to">The scene that is beginning (can be null).</param>
        protected virtual void OnSceneTransition(Scene? from, Scene? to) {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            TimeRate = 1f;
        }

        /// <summary>
        /// The currently active Scene. Note that if set, the Scene will not actually change until the end of the Update.
        /// </summary>
        public static Scene? Scene {
            get => Instance?.scene;
            set { if (Instance != null) Instance.nextScene = value; }
        }

        #endregion

        #region Screen

        public static Viewport Viewport { get; private set; }
        public static Matrix ScreenMatrix;

        /// <summary>
        /// Sets the game to windowed mode with the specified dimensions.
        /// </summary>
        /// <param name="width">The window width in pixels.</param>
        /// <param name="height">The window height in pixels.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when width or height are not positive.</exception>
        public static void SetWindowed(int width, int height) {
#if !CONSOLE
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

            if (Graphics != null) {
                resizing = true;
                Graphics.PreferredBackBufferWidth = width;
                Graphics.PreferredBackBufferHeight = height;
                Graphics.IsFullScreen = false;
                Graphics.ApplyChanges();
                Console.WriteLine($"WINDOW-{width}x{height}");
                resizing = false;
            }
#endif
        }

        /// <summary>
        /// Sets the game to fullscreen mode using the default adapter's current display mode.
        /// </summary>
        public static void SetFullscreen() {
#if !CONSOLE
            if (Graphics != null) {
                resizing = true;
                Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                Graphics.IsFullScreen = true;
                Graphics.ApplyChanges();
                Console.WriteLine("FULLSCREEN");
                resizing = false;
            }
#endif
        }

        /// <summary>
        /// Updates the viewport and screen matrix based on current screen dimensions and view padding.
        /// </summary>
        private void UpdateView() {
            float screenWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            float screenHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

            // get View Size
            if (screenWidth / Width > screenHeight / Height) {
                ViewWidth = (int) (screenHeight / Height * Width);
                ViewHeight = (int) screenHeight;
            } else {
                ViewWidth = (int) screenWidth;
                ViewHeight = (int) (screenWidth / Width * Height);
            }

            // apply View Padding
            var aspect = ViewHeight / (float) ViewWidth;
            ViewWidth -= ViewPadding * 2;
            ViewHeight -= (int) (aspect * ViewPadding * 2);

            // update screen matrix
            ScreenMatrix = Matrix.CreateScale(ViewWidth / (float) Width);

            // update viewport
            Viewport = new Viewport {
                X = (int) (screenWidth / 2 - ViewWidth / 2),
                Y = (int) (screenHeight / 2 - ViewHeight / 2),
                Width = ViewWidth,
                Height = ViewHeight,
                MinDepth = 0,
                MaxDepth = 1
            };

            //Debug Log
            //Calc.Log("Update View - " + screenWidth + "x" + screenHeight + " - " + viewport.Width + "x" + viewport.GuiHeight + " - " + viewport.X + "," + viewport.Y);
        }

        #endregion
    }
}
