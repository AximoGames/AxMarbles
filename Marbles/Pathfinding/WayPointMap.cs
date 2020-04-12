// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

using OpenToolkit.Mathematics;

#pragma warning disable CA1010 // Collections should implement generic interface

namespace Aximo.Marbles.PathFinding
{
    public class WayPointMap
    {

        public int JumpCost = 10;
        public int MinSektorCost = 10;
        private int LenX;
        private int LenY;
        public WayPoint[,] Items;
        public JumpHolePointToPointList JumpHoles = new JumpHolePointToPointList();
        public MarbleBoard Board;

        public WayPointMap(MarbleBoard board)
        {
            Board = board;
            LenX = Board.Width;
            LenY = Board.Height;
        }

        public WayPoint this[int x, int y]
        {
            get
            {
                if ((x < 0) || (x > LenX - 1))
                    return null;
                if ((y < 0) || (y > LenY - 1))
                    return null;

                if (Items[x, y] == null)
                {
                    var itm = new WayPoint(this)
                    {
                        Position = new Vector2i(x, y),
                        //itm.GCost = CByte(SektorMapArray(x, y).DefaultPlanet.EinflugKosten)
                        GCost = (ushort)Board.GetPathFindingCosts(x, y),
                        IsClosed = false,
                        Region = MarbleRegion.Default,
                    };
                    Items[x, y] = itm;
                }
                return Items[x, y];
            }
        }

        public WayPoint this[Vector2i pos]
        {
            get
            {
                return this[pos.X, pos.Y];
            }
        }

        public WayPoint ItemByOffset(Vector2i pos, int offsetX, int offsetY)
        {
            return this[pos.X + offsetX, pos.Y + offsetY];
        }

        public void Reset()
        {
            if (Items == null)
            {
                Items = new WayPoint[LenX, LenY];
                return;
            }
            var x = 0;
            var y = 0;
            for (y = 0; y < LenY; y++)
                for (x = 0; x < LenX; x++)
                {
                    if (!(Items[x, y] == null))
                    {
                        Items[x, y].Parent = null;
                        Items[x, y].GWCost = 0;
                        Items[x, y].HCost = 0;
                        Items[x, y].OpenIndex = 0;
                        Items[x, y].IsClosed = false;
                        //---
                        Items[x, y].GCost = Board.GetPathFindingCosts(x, y);
                    }
                }
        }

        public void Clear()
        {
            Items = null;
        }

    }

}
