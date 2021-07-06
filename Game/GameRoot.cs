using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Apos.Input;
using FontStashSharp;
using SDL2;
using static SDL2.SDL;
using System;
using System.Runtime.InteropServices;

namespace GameProject {
    public class GameRoot : Game {
        public GameRoot() {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.SynchronizeWithVerticalRetrace = false;
            IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            IsFixedTimeStep = false;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += WindowSizeChanged;

            SDL_SetWindowMinimumSize(Window.Handle, 480, 270);

            _filter = new SDL_EventFilter(HandleSDLEvent);
            SDL_AddEventWatch(_filter, IntPtr.Zero);
            // SDL_SetEventFilter(_filter, IntPtr.Zero);

            _timerProc = BackupTick;
            _handle = SetTimer(IntPtr.Zero, IntPtr.Zero, 1, _timerProc);

            base.Initialize();
        }

        protected override void LoadContent() {
            _s = new SpriteBatch(GraphicsDevice);

            InputHelper.Setup(this);

            _fontSystem = new FontSystem();
            _fontSystem.AddFont(TitleContainer.OpenStream($"{Content.RootDirectory}/source-code-pro-medium.ttf"));
        }

        protected override void Update(GameTime gameTime) {
            _started = true;
            if (!_manualTick) {
                _manualTickCount = 0;
            }

            InputHelper.UpdateSetup();

            if (_quit.Pressed()) Exit();

            if (_toggleMaximize.Pressed()) {
                if (_isMaximized) {
                    SDL_RestoreWindow(Window.Handle);
                } else {
                    SDL_MaximizeWindow(Window.Handle);
                }
            }

            InputHelper.UpdateCleanup();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            var font = _fontSystem.GetFont(30);
            _s.Begin();
            _s.DrawString(font, $"{(int)gameTime.TotalGameTime.TotalMilliseconds}", new Vector2(10, 10), Color.White);
            _s.DrawString(font, $"{_graphics.PreferredBackBufferWidth}, {_graphics.PreferredBackBufferHeight}", new Vector2(10, 100), Color.White);
            _s.End();

            base.Draw(gameTime);
        }

        private void WindowSizeChanged(object? sender, EventArgs e) {
            Console.WriteLine("Size change.");
        }
        private unsafe int HandleSDLEvent(IntPtr userdata, IntPtr ptr) {
            SDL_Event* e = (SDL_Event*) ptr;

            switch (e->type) {
            case SDL_EventType.SDL_WINDOWEVENT:
                switch (e->window.windowEvent) {
                case SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                    int width = e->window.data1;
                    int height = e->window.data2;
                    var p = Window.Position;
                    _graphics.PreferredBackBufferWidth = width;
                    _graphics.PreferredBackBufferHeight = height;
                    _graphics.ApplyChanges();
                    Window.Position = p;
                    WindowSizeChanged(this, EventArgs.Empty);
                    if (_started) this.Tick();
                    break;
                case SDL_WindowEventID.SDL_WINDOWEVENT_MOVED:
                    if (_started) this.Tick();
                    break;
                case SDL_WindowEventID.SDL_WINDOWEVENT_MAXIMIZED:
                    _isMaximized = true;
                    break;
                case SDL_WindowEventID.SDL_WINDOWEVENT_RESTORED:
                    _isMaximized = false;
                    break;
                }
                break;
            }

            return 0;
        }
        private void BackupTick(IntPtr hWnd, uint uMsg, IntPtr nIDEvent, uint dwTime) {
            if (_started) {
                if (_manualTickCount > 2) {
                    _manualTick = true;
                    this.Tick();
                    _manualTick = false;
                }
                _manualTickCount++;
            }
        }
        bool _started = false;
        bool _isMaximized = false;

        GraphicsDeviceManager _graphics;
        SpriteBatch _s;

        ICondition _quit =
            new AnyCondition(
                new KeyboardCondition(Keys.Escape),
                new GamePadCondition(GamePadButton.Back, 0)
            );
        ICondition _toggleMaximize = new KeyboardCondition(Keys.M);

        FontSystem _fontSystem = null!;

        SDL_EventFilter _filter;

        [DllImport("user32.dll", ExactSpelling=true)]
        static extern IntPtr SetTimer(IntPtr hWnd, IntPtr nIDEvent, uint uElapse, TimerProc lpTimerFunc);
        delegate void TimerProc(IntPtr hWnd, uint uMsg, IntPtr nIDEvent, uint dwTime);

        IntPtr _handle;
        TimerProc _timerProc;

        int _manualTickCount = 0;
        bool _manualTick;
    }
}
