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

        private static Thread th;

        public static void Main(string[] args)
        {
            UIThreadMain();
            return;

            th = new Thread(UIThreadMain);
            th.Start();

            ConsoleLoop();

            demo.Close();
            demo.Dispose();
            th.Abort();
            Environment.Exit(0);
        }

        private static void ConsoleLoop()
        {
            while (true)
            {
                var cmd = Console.ReadLine();
                var args = cmd.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (args.Length == 0)
                    continue;
                switch (cmd)
                {
                    case "q":
                        return;
                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
            }
        }

        private static MarblesApplication demo;

        private static void UIThreadMain()
        {
            demo = new MarblesApplication(new RenderApplicationStartup
            {
                WindowTitle = "Marbles",
                WindowSize = new Vector2i(800, 600),
                WindowBorder = WindowBorder.Fixed,
            });
            demo.Run();
            Environment.Exit(0);
        }

    }
}
