using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Apos.Input;
using FontStashSharp;
using SDL2;
using static SDL2.SDL;
using System;

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

            // _callback = new SDL_TimerCallback(PushEvent);
            // _handle = new IntPtr(SDL_AddTimer(1, _callback, IntPtr.Zero));

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
            Console.WriteLine("Hello");
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
            // case SDL_EventType.SDL_USEREVENT:
            //     Console.WriteLine("User");
            //     break;
            }

            return 0;
        }
        // private uint PushEvent(uint interval, IntPtr param) {
        //     SDL_Event e = new SDL_Event();
        //     SDL_UserEvent userEvent = new SDL_UserEvent();

        //     userEvent.type = (uint)SDL_EventType.SDL_USEREVENT;
        //     userEvent.code = 0;

        //     e.type = SDL_EventType.SDL_USEREVENT;
        //     e.user = userEvent;

        //     SDL_PushEvent(ref e);

        //     return 1;
        // }
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
        // IntPtr _handle;
        // SDL_TimerCallback _callback;
    }
}
