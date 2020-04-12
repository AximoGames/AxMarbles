// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

using System;

#pragma warning disable CA1010 // Collections should implement generic interface

namespace Aximo.Marbles.PathFinding
{
    public class WayPointHeap
    {
        public WayPoint[] List;

        public WayPointHeap()
        {
            List = new WayPoint[1];
        }

        public void Clear()
        {
            List = new WayPoint[1];
            _Count = 0;
        }

        private int _Count;
        public int Count
        {
            get
            {
                return _Count;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return _Count == 0;
            }
        }

        public void Push(WayPoint data)
        {
            _Count += 1;
            if (_Count >= List.Length)
                Array.Resize(ref List, _Count + 10 + 1);
            List[_Count] = data;
            var pos = _Count;
            data.OpenIndex = pos;
            while (pos != 1)
            {
                var wp = List[pos];
                var parent = List[pos / 2];
                if (Compare(wp, parent) < 1)
                {
                    Swap(wp, parent);
                    pos /= 2;
                }
                else
                {
                    break;
                }
            }
        }

        public WayPoint Pop()
        {
            var wp = List[1];
            RemoveFirst();
            wp.OpenIndex = 0;
            return wp;
        }

        private void RemoveFirst()
        {
            var index1 = 0;
            var index2 = 0;
            var leftIndex = 0;
            var rightIndex = 0;

            List[1] = List[_Count];
            List[1].OpenIndex = 1;
            _Count -= 1;
            index2 = 1;
            while (true)
            {
                index1 = index2;
                leftIndex = 2 * index1;
                rightIndex = leftIndex + 1;
                if (rightIndex <= _Count)
                {
                    if (Compare(List[index1], List[leftIndex]) > -1)
                        index2 = leftIndex;
                    if (Compare(List[index2], List[rightIndex]) > -1)
                        index2 = rightIndex;
                }
                else
                {
                    if (leftIndex <= _Count)
                    {
                        if (Compare(List[index1], List[leftIndex]) > -1)
                            index2 = leftIndex;
                    }
                }
                if (index1 != index2)
                    Swap(List[index1], List[index2]);
                else
                    break;
            }
        }

        public void Sort(int index)
        {
            var pos = index;
            while (pos > 1)
            {
                var wp = List[pos];
                var parent = List[pos / 2];
                if (Compare(wp, parent) < 1)
                {
                    Swap(wp, parent);
                    pos /= 2;
                }
                else
                {
                    break;
                }
            }
        }

        private int Compare(WayPoint wp1, WayPoint wp2)
        {
            return wp1.GWCost + wp1.HCost - (wp2.GWCost + wp2.HCost);
        }

        private void Swap(WayPoint p1, WayPoint p2)
        {
            //Tausche Einträge
            List[p1.OpenIndex] = p2;
            List[p2.OpenIndex] = p1;

            //Tausche Indexe
            var tempPos = p1.OpenIndex;
            p1.OpenIndex = p2.OpenIndex;
            p2.OpenIndex = tempPos;
        }
    }
}
