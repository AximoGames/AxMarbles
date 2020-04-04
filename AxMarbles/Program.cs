﻿// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using Aximo.Engine;
using OpenToolkit;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;

namespace Aximo.Marbles
{
    internal class Program
    {

        public static void Main(string[] args)
        {
            var config = new RenderApplicationConfig
            {
                WindowTitle = "Marbles",
                WindowSize = new Vector2i(800, 600),
                WindowBorder = WindowBorder.Fixed,
            };

            new GameStartup<MarblesApplication, GtkUI>(config).Start();
        }

    }
}
