// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

using OpenToolkit.Mathematics;

#pragma warning disable CA1010 // Collections should implement generic interface

namespace Aximo.Marbles.PathFinding
{
    public class JumpHolePoint
    {
        public Vector2i Position;
        public bool Used = false;

        public JumpHolePoint(Vector2i pos)
        {
            Position = pos;
        }
    }
}
