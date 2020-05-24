// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

using Aximo.Engine;
using Aximo.Engine.Windows;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;

namespace Aximo.Marbles
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var config = new ApplicationConfig
            {
                WindowTitle = "Marbles",
                WindowSize = new Vector2i(800, 600),
                WindowBorder = WindowBorder.Fixed,
                UseConsole = true,
                IsMultiThreaded = true,
                RenderFrequency = 0,
                UpdateFrequency = 0,
                IdleRenderFrequency = 0,
                IdleUpdateFrequency = 0,
                VSync = VSyncMode.Off,
                //FlushRenderBackend = FlushRenderBackend.Draw,
            };

            new MarblesApplication().Start(config);
        }
    }
}
