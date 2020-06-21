// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Aximo.Engine;
using Aximo.Engine.Components.UI;
using Aximo.Render;
using OpenToolkit.Mathematics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace Aximo.Marbles
{
    public class UIMarbles : UIComponent
    {
        private DateTime LastStatUpdate;

        public UIMarbles() : base(new Vector2(RenderContext.Current.ScreenPixelSize.X, RenderContext.Current.ScreenPixelSize.Y))
        {
        }

        public override void UpdateFrame()
        {
            if ((DateTime.UtcNow - LastStatUpdate).TotalSeconds < 0.5)
                return;
            LastStatUpdate = DateTime.UtcNow;
            ImageContext.Clear(Color.Transparent);
            var app = Application.Current as MarblesApplication;
            var board = app.Board;
            if (board == null)
                return;
            var txt = board.TotalScore.ToString();
            if (board.LastMoveScore > 0)
                txt += "\n+" + board.LastMoveScore.ToString();
            ImageContext.DrawText(txt, Color.White, new Vector2(620, 20));
            UpdateTexture();
        }
    }
}
