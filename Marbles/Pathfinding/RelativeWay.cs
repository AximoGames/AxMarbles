// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

using OpenToolkit.Mathematics;

#pragma warning disable CA1010 // Collections should implement generic interface

namespace Aximo.Marbles.PathFinding
{
    public struct RelativeWay
    {
        public int Range;
        public RelativeWayDirection Way;
        public Vector2i JumpTo;

        public RelativeWay(int range, RelativeWayDirection way)
            : this()
        {
            Range = range;
            Way = way;
        }

        public RelativeWay(Vector2i jumpTo)
            : this()
        {
            JumpTo = jumpTo;
            Way = RelativeWayDirection.None;
            Range = 0;
        }

        public static RelativeWayDirection GetWay(Vector2i c1, Vector2i c2)
        {
            if (c1.Equals(c2))
                return RelativeWayDirection.None;
            if (c1.X == c2.X)
            {
                if (c1.Y < c2.Y)
                    return RelativeWayDirection.Down;
                else
                    return RelativeWayDirection.Up;
            }
            else
            {
                if (c1.Y == c2.Y)
                {
                    if (c1.X < c2.X)
                        return RelativeWayDirection.Right;
                    else
                        return RelativeWayDirection.Left;
                }
                else
                {
                    return RelativeWayDirection.None;
                }
            }
        }

        public int RelX
        {
            get
            {
                if (Way == RelativeWayDirection.Right)
                {
                    return Range;
                }
                else
                {
                    if (Way == RelativeWayDirection.Left)
                        return -Range;
                    else
                        return 0;
                }
            }
        }

        public int RelY
        {
            get
            {
                if (Way == RelativeWayDirection.Down)
                {
                    return Range;
                }
                else
                {
                    if (Way == RelativeWayDirection.Up)
                        return -Range;
                    else
                        return 0;
                }
            }
        }

    }

}
