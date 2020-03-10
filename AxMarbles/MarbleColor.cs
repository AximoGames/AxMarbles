// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

namespace Aximo.Marbles
{
    public enum MarbleColor
    {
        None = 0,
        Red = 1,
        Green = 2,
        Blue = 4,
        Yellow = 8,
        Orange = 16,
        White = 32,
        Black = 64,
        BombColor = 128,
        ColorJoker = Red | Green | Blue | Yellow | Orange | White | Black,
        BombJoker = ColorJoker | BombColor,
    }

}