// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using OpenToolkit;
using OpenToolkit.Mathematics;

#pragma warning disable CA1010 // Collections should implement generic interface

namespace Aximo.Marbles.PathFinding
{

    public class JumpHolePointToPointList
    {
        private SortedList<Vector2i, JumpHolePointList> InnerList = new SortedList<Vector2i, JumpHolePointList>();

        public void Add(Vector2i start, Vector2i ziel)
        {
            JumpHolePointList list = null;
            if (!InnerList.TryGetValue(start, out list))
            {
                list = new JumpHolePointList
                {
                    Position = start,
                };
                InnerList.Add(start, list);
            }

            list.Add(ziel);
        }

        public JumpHolePointList this[Vector2i pos]
        {
            get
            {
                return InnerList[pos];
            }
            set
            {
                InnerList[pos] = value;
            }
        }

        public JumpHolePointList this[int idx]
        {
            get
            {
                return InnerList.Values[idx];
            }
            set
            {
                InnerList[InnerList.Keys[idx]] = value;
            }
        }

        public int Count
        {
            get
            {
                return InnerList.Count;
            }
        }

        public bool Contains(Vector2i pos)
        {
            return InnerList.ContainsKey(pos);
        }

        public void ResetUsed()
        {
            foreach (var itm in InnerList.Values)
                itm.ResetUsed();
        }

    }

}
