// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

using System;
using System.Drawing;
using Aximo.Engine;
using Aximo.Render;

namespace Aximo.Marbles
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
