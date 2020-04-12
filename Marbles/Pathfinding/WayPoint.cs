// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

using OpenToolkit.Mathematics;

#pragma warning disable CA1010 // Collections should implement generic interface

namespace Aximo.Marbles.PathFinding
{
    public class WayPoint
    {
        private WayPointMap Map;
        public WayPoint(WayPointMap map)
        {
            Map = map;
        }

        public WayPoint Parent;
        public Vector2i Position;
        //Public GCost As Byte
        public int GWCost;
        public int HCost;
        public MarbleRegion Region;
        public int OpenIndex;
        public bool IsClosed;

        public ushort GCost;
    }
}
