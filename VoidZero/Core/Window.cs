using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace VoidZero.Core
{
    // Startup Window and top level class
    public class Window : GameWindow
    {
        private GameManager _game;

        public Window()
            : base(GameWindowSettings.Default,
                new NativeWindowSettings
                {
                    ClientSize = new Vector2i(1600, 900),
                    APIVersion = new Version(4, 1),
                    Title = "Void Zero",
                    WindowBorder = WindowBorder.Fixed
                })
        { }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ClearColor(0f, 0f, 0f, 1f);

            VSync = VSyncMode.On;

            _game = new GameManager(this);
            _game.Initialize();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            _game.Update((float)e.Time);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            _game.ApplyViewport(FramebufferSize.X, FramebufferSize.Y);
            _game.Draw((float)e.Time);

            GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);
            _game.DrawMenu();
            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            _game.OnResize(Size.X, Size.Y, FramebufferSize.X, FramebufferSize.Y);
        }
    }
}
