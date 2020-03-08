using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace AxEngine
{
    public class UIObject : ScreenTextureObject, IUpdateFrame
    {
        private GraphicsTexture GfxTexture;
        private DateTime LastStatUpdate;
        private Font DefaultFont = new Font(FontFamily.GenericSansSerif, 15, GraphicsUnit.Point);

        public UIObject()
        {
        }

        public override void Init()
        {
            GfxTexture = new GraphicsTexture(Context.ScreenSize);
            GfxTexture.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            SourceTexture = GfxTexture.Texture;
            base.Init();
        }

        public void OnUpdateFrame()
        {
            if ((DateTime.UtcNow - LastStatUpdate).TotalSeconds < 0.5)
                return;
            LastStatUpdate = DateTime.UtcNow;
            GfxTexture.Graphics.Clear(Color.Transparent);
            var app = RenderApplication.Current as MarblesApplication;
            var board = app.Board;
            if (board == null)
                return;
            var txt = board.TotalScore.ToString();
            if (board.LastMoveScore > 0)
                txt += "\n+" + board.LastMoveScore.ToString();
            GfxTexture.Graphics.DrawString(txt, DefaultFont, Brushes.White, new PointF(620f, 20f));
            GfxTexture.UpdateTexture();
        }
    }

}
