﻿using System;
using OpenToolkit.Graphics.OpenGL4;
using System.Threading;
using OpenToolkit.Mathematics;
using System.Runtime.InteropServices;
using OpenToolkit.Windowing.Common;

namespace AxEngine
{
    class MainClass
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
        }

    }
}
