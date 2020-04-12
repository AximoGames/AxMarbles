// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using OpenToolkit.Mathematics;

#pragma warning disable CA1010 // Collections should implement generic interface

namespace Aximo.Marbles.PathFinding
{
    public class PathFinder
    {

        public WayPointMap Map;
        private WayPointHeap OpenList = new WayPointHeap();
        private Vector2i StartPos;
        private Vector2i DestinationPos;
        private MarbleRegion[] Regions;
        private bool AllowJumpHoles = false;

        public bool _Searching = false;
        public bool Searching
        {
            get
            {
                return _Searching;
            }
        }

        public Vector2iList FindPath(Vector2i start, Vector2i ziel, MarbleRegion[] regionen)
        {
            return FindPath(start, ziel, regionen, new JumpHolePointToPointList());
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
        public Vector2iList FindPath(Vector2i start, Vector2i destination, MarbleRegion[] regionen, JumpHolePointToPointList jumpHoles)
        {
            //LockActions()
            if (_Searching)
                throw new Exception("Lock Error");
            Regions = regionen;
            StartPos = start;
            DestinationPos = destination;

            WayPoint wp = null;
            try
            {
                _Searching = true;

                Map.Reset();

                Map.JumpHoles = jumpHoles;
                AllowJumpHoles = jumpHoles.Count != 0;

                if (StartPos.Equals(DestinationPos))
                    return null;
                if (!Allowed(Map[DestinationPos]))
                    return null;

                wp = Map[StartPos];
                wp.HCost = CalcHCost(wp);
                wp.GWCost = 0;
                wp.Parent = null;
                OpenList.Push(wp);

                var callCount = 0;
                var startTime = DateTime.Now;

                while (!OpenList.IsEmpty)
                {
                    callCount += 1;
                    if (callCount == 100000)
                    {
                        callCount = 0;
                        if ((DateTime.UtcNow - startTime).TotalSeconds > 10)
                            return null;
                    }
                    wp = OpenList.Pop();
                    wp.IsClosed = true;
                    AddNextNodes(wp);
                    if (wp.Position.Equals(DestinationPos))
                        return GetWay(wp);
                }
                return null;
            }
            finally
            {
                OpenList.Clear();
                _Searching = false;
                //UnLockActions()
            }
        }

        public bool Allowed(WayPoint p)
        {
            return p.GCost < ushort.MaxValue && Array.IndexOf(Regions, p.Region) > -1;
        }

        public bool Allowed(Vector2i p)
        {
            return Allowed(Map[p.X, p.Y]);
        }

        public void AddNextNodes(WayPoint wp)
        {
            var wps = GetSuccessors(wp);

            if (AllowJumpHoles)
            {
                if (Map.JumpHoles.Contains(wp.Position))
                {
                    for (var iJump = 0; iJump < Map.JumpHoles[wp.Position].Count; iJump++)
                        wps.Add(Map[Map.JumpHoles[wp.Position][iJump].Position]);
                }
            }

            for (var i = wps.Count - 1; i >= 0; i--)
            {
                var p = (WayPoint)wps.Pop();
                if (Allowed(p))
                {
                    if (!p.IsClosed)
                    {
                        if (p.OpenIndex > 0)
                        {
                            if (p.GCost + wp.GWCost <= p.GWCost)
                            {
                                p.Parent = wp;
                                p.GWCost = p.GCost + wp.GWCost;
                                if (ManhattanDistance(p.Position, wp.Position) > 1)
                                    p.GWCost += Map.JumpCost;
                                OpenList.Sort(p.OpenIndex);
                            }
                        }
                        else
                        {
                            p.Parent = wp;
                            p.GWCost = p.GCost + wp.GWCost;
                            if (AllowJumpHoles && ManhattanDistance(p.Position, wp.Position) > 1)
                                p.GWCost += Map.JumpCost;
                            p.HCost = CalcHCost(p);
                            OpenList.Push(p);
                        }
                    }
                }
            }
        }

        public WayPointStack GetSuccessors(WayPoint p)
        {
            var list = new WayPointStack();
            var pos = new Vector2i();

            if (p.Parent != null)
                pos = p.Parent.Position;
            else
                pos = DestinationPos;

            if (Math.Abs(p.Position.X - pos.X) > Math.Abs(p.Position.Y - pos.Y))
            {
                //ow ist wichtiger
                if (p.Position.X < pos.X)
                {
                    //o vor w
                    if (p.Position.Y < pos.Y)
                    {
                        //s vor n
                        list.Add(Map.ItemByOffset(p.Position, 1, 0)); //o
                        list.Add(Map.ItemByOffset(p.Position, -1, 0)); //w
                        list.Add(Map.ItemByOffset(p.Position, 0, -1)); //n
                        list.Add(Map.ItemByOffset(p.Position, 0, 1)); //s
                    }
                    else
                    {
                        //n vor s
                        list.Add(Map.ItemByOffset(p.Position, 1, 0)); //o
                        list.Add(Map.ItemByOffset(p.Position, -1, 0)); //w
                        list.Add(Map.ItemByOffset(p.Position, 0, 1)); //s
                        list.Add(Map.ItemByOffset(p.Position, 0, -1)); //n
                    }
                }
                else
                {
                    //w vor o
                    if (p.Position.Y < pos.Y)
                    {
                        //s vor n
                        list.Add(Map.ItemByOffset(p.Position, -1, 0)); //w
                        list.Add(Map.ItemByOffset(p.Position, 1, 0)); //o
                        list.Add(Map.ItemByOffset(p.Position, 0, -1)); //n
                        list.Add(Map.ItemByOffset(p.Position, 0, 1)); //s
                    }
                    else
                    {
                        //n vor s
                        list.Add(Map.ItemByOffset(p.Position, -1, 0)); //w
                        list.Add(Map.ItemByOffset(p.Position, 1, 0)); //o
                        list.Add(Map.ItemByOffset(p.Position, 0, 1)); //s
                        list.Add(Map.ItemByOffset(p.Position, 0, -1)); //n
                    }
                }
            }
            else
            {
                //ns ist wichtiger
                if (p.Position.Y < pos.Y)
                {
                    //s vor n
                    if (p.Position.X < pos.X)
                    {
                        //o vor w
                        list.Add(Map.ItemByOffset(p.Position, 0, 1)); //s
                        list.Add(Map.ItemByOffset(p.Position, 0, -1)); //n
                        list.Add(Map.ItemByOffset(p.Position, -1, 0)); //w
                        list.Add(Map.ItemByOffset(p.Position, 1, 0)); //o
                    }
                    else
                    {
                        //w vor o
                        list.Add(Map.ItemByOffset(p.Position, 0, 1)); //s
                        list.Add(Map.ItemByOffset(p.Position, 0, -1)); //n
                        list.Add(Map.ItemByOffset(p.Position, 1, 0)); //o
                        list.Add(Map.ItemByOffset(p.Position, -1, 0)); //w
                    }
                }
                else
                {
                    //n vor s
                    if (p.Position.X < pos.X)
                    {
                        //o vor w
                        list.Add(Map.ItemByOffset(p.Position, 0, -1)); //n
                        list.Add(Map.ItemByOffset(p.Position, 0, 1)); //s
                        list.Add(Map.ItemByOffset(p.Position, -1, 0)); //w
                        list.Add(Map.ItemByOffset(p.Position, 1, 0)); //o
                    }
                    else
                    {
                        //w vor o
                        list.Add(Map.ItemByOffset(p.Position, 0, -1)); //n
                        list.Add(Map.ItemByOffset(p.Position, 0, 1)); //s
                        list.Add(Map.ItemByOffset(p.Position, 1, 0)); //o
                        list.Add(Map.ItemByOffset(p.Position, -1, 0)); //w
                    }
                }
            }

            return list;
        }

        public int CalcHCost(WayPoint wp)
        {
            //CalcHCost = ManhattanDistance(wp.Position, ZielPos) * Map.MinSektorCost

            var result = ManhattanDistance(wp.Position, DestinationPos);
            if (AllowJumpHoles)
            {
                Map.JumpHoles.ResetUsed();
                for (var i = 0; i < Map.JumpHoles.Count; i++)
                {
                    var aktTestPos = Map.JumpHoles[i].Position;
                    if (Allowed(aktTestPos))
                    {
                        var toJumpCost = ManhattanDistance(wp.Position, aktTestPos);
                        if (toJumpCost < result)
                        {
                            var jumpCost = CalcJumpHCost(aktTestPos, aktTestPos, result, toJumpCost + Map.JumpCost);
                            if (jumpCost < result)
                                result = jumpCost;
                        }
                    }
                }
            }
            return result * Map.MinSektorCost;
        }

        public int CalcJumpHCost(Vector2i jumpPoint, Vector2i lastPos, int bestWay, int cost)
        {
            var result = 0;
            result = bestWay;
            for (var i = 0; i < Map.JumpHoles[jumpPoint].Count; i++)
            {
                var aktPoint = Map.JumpHoles[jumpPoint][i].Position;
                if (!Map.JumpHoles[jumpPoint][i].Used)
                {
                    Map.JumpHoles[jumpPoint][i].Used = true;
                    if (Allowed(aktPoint))
                    {
                        if (!aktPoint.Equals(lastPos))
                        {
                            var fromAktPoint = ManhattanDistance(aktPoint, DestinationPos) + cost;
                            if (fromAktPoint < result)
                                result = fromAktPoint;
                            for (var j = 0; j < Map.JumpHoles.Count; j++)
                            {
                                var testPoint = Map.JumpHoles[j].Position;
                                if ((!testPoint.Equals(jumpPoint)) && Allowed(testPoint))
                                {
                                    var toJumpCost = ManhattanDistance(aktPoint, testPoint) + cost;
                                    if (toJumpCost < result)
                                    {
                                        var jumpCost = CalcJumpHCost(testPoint, aktPoint, result, toJumpCost + Map.JumpCost);
                                        if (jumpCost < result)
                                            result = jumpCost;
                                    }
                                }
                            }
                        }
                    }
                }
                Map.JumpHoles[jumpPoint][i].Used = false;
            }
            return 0; //TODO?
        }

        public int ManhattanDistance(Vector2i pos1, Vector2i pos2)
        {
            return Math.Abs(pos1.X - pos2.X) + Math.Abs(pos1.Y - pos2.Y);
        }

        private Vector2iList GetWay(WayPoint goal)
        {
            var list = new Vector2iList();
            GetWayRecursive(goal, list);
            return list;
        }

        private void GetWayRecursive(WayPoint wp, Vector2iList list)
        {
            if (wp.Parent != null)
                GetWayRecursive(wp.Parent, list);
            list.Add(wp.Position);
        }

        public class WayPointStack : Stack<WayPoint>
        {

            public void Add(WayPoint itm)
            {
                if (itm != null)
                    Push(itm);
            }

        }

    }

}
