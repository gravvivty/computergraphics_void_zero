using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;
using VoidZero.Game;
using VoidZero.Graphics;
using VoidZero.States;

namespace VoidZero.Core
{
    public class Window : GameWindow
    {
        private GameManager _game;

        public Window()
            : base(GameWindowSettings.Default,
                new NativeWindowSettings
                {
                    ClientSize = new Vector2i(1600, 900),
                    APIVersion = new Version(4, 1)
                })
        { }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ClearColor(0.05f, 0.1f, 0.15f, 1f);

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
            _game.Draw();
            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
            _game.OnResize(Size.X, Size.Y);
        }
    }
}
