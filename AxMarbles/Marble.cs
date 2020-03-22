// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

using System;
using Aximo.Engine;
using Aximo.Render;
using OpenTK;

namespace Aximo.Marbles
{

    public class Marble : IDisposable
    {

        public Marble(MarbleColor color)
        {
            Color = color;
            var colors = color.GetRegularColors();
            if (colors.Count == 2)
            {
                Color1 = colors[0];
                Color2 = colors[1];
            }
            else
            {
                Color1 = color;
                Color2 = color;
            }
        }

        public Marble(MarbleColor color1, MarbleColor color2)
        {
            Color1 = color1;
            Color2 = color2;
            Color = color1 | Color2;
        }

        public MarbleColor Color { get; private set; }
        public MarbleColor Color1 { get; private set; }
        public MarbleColor Color2 { get; private set; }

        public Vector2i Position;

        public StaticMeshComponent RenderObject;
        public MarbleState State;

        public void Dispose()
        {
            RenderObject.Dispose();
            RenderObject = null;
        }

        public override string ToString()
        {
            return $"[Pos: {Position}, Color: {Color}]";
        }
    }

}