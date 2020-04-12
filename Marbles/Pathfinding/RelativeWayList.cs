// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using OpenToolkit.Mathematics;

#pragma warning disable CA1010 // Collections should implement generic interface

namespace Aximo.Marbles.PathFinding
{
    public class RelativeWayList : List<RelativeWay>
    {
        public Vector2iList ToVektorList()
        {
            var list = new Vector2iList();
            var vek = new Vector2i();

            //          SRelativeWay Way = new SRelativeWay();
            foreach (var way in this)
            {
                vek.X += way.RelX;
                vek.Y += way.RelY;
                if (vek.X != 0 && vek.Y != 0)
                {
                    list.Add(vek);
                    vek.X = 0;
                    vek.Y = 0;
                }
            }
            if (vek.X != 0 || vek.Y != 0)
                list.Add(vek);
            return list;
        }

        public static RelativeWayList FromVektorList(Vector2iList vektorList)
        {
            var list = new RelativeWayList();

            //          SPoint Vek = new SPoint();
            foreach (var vek in vektorList)
            {
                if (vek.X != 0)
                {
                    if (vek.X > 0)
                        list.Add(new RelativeWay(vek.X, RelativeWayDirection.Right));
                    else
                        list.Add(new RelativeWay(vek.X, RelativeWayDirection.Left));
                }
                if (vek.Y != 0)
                {
                    if (vek.Y > 0)
                        list.Add(new RelativeWay(vek.Y, RelativeWayDirection.Down));
                    else
                        list.Add(new RelativeWay(vek.Y, RelativeWayDirection.Up));
                }
            }
            return list;
        }
    }
}
