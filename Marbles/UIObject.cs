// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Aximo.Engine;
using Aximo.Render;
using OpenToolkit.Mathematics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace Aximo.Marbles
{
    public class UIMarbles : UIComponent
    {
        private DateTime LastStatUpdate;
        private Font DefaultFont = new Font(SystemFonts.Families.First(), 15, FontStyle.Regular);

        public UIMarbles() : base(new Vector2i(RenderContext.Current.ScreenSize.X, RenderContext.Current.ScreenSize.Y))
        {
        }

        public override void UpdateFrame()
        {
            if ((DateTime.UtcNow - LastStatUpdate).TotalSeconds < 0.5)
                return;
            LastStatUpdate = DateTime.UtcNow;
            Image.Mutate(ctx => ctx.Fill(Color.Transparent));
            var app = RenderApplication.Current as MarblesApplication;
            var board = app.Board;
            if (board == null)
                return;
            var txt = board.TotalScore.ToString();
            if (board.LastMoveScore > 0)
                txt += "\n+" + board.LastMoveScore.ToString();
            Image.Mutate(ctx => ctx.DrawText(txt, DefaultFont, Color.White, new PointF(620, 20)));
            UpdateTexture();
        }

    }

}
