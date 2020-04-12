// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using OpenToolkit.Mathematics;

#pragma warning disable CA1010 // Collections should implement generic interface

namespace Aximo.Marbles.PathFinding
{
    public class JumpHolePointList
    {
        private List<JumpHolePoint> InnerList = new List<JumpHolePoint>();

        public Vector2i Position;

        public void Add(Vector2i start)
        {
            InnerList.Add(new JumpHolePoint(start));
        }

        public JumpHolePoint this[int idx]
        {
            get
            {
                return InnerList[idx];
            }
            set
            {
                InnerList[idx] = value;
            }
        }

        public int Count
        {
            get
            {
                return InnerList.Count;
            }
        }

        public void ResetUsed()
        {
            foreach (var itm in InnerList)
                itm.Used = false;
        }

    }

}
