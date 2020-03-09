using OpenTK;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Aximo.Marbles
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

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //~ Enumeration ERelativeWay																																												~
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    [Flags]
    public enum RelativeWayDirection : int
    {
        None = 0,
        Left = 1,
        Right = 2,
        Up = 4,
        Down = 8
    }

    public class RelativeWayList : List<RelativeWay>
    {

        public Vector2iList ToVektorList()
        {
            var list = new Vector2iList();
            var vek = new Vector2i();

            //			SRelativeWay Way = new SRelativeWay();
            foreach (var Way in this)
            {
                vek.X += Way.RelX;
                vek.Y += Way.RelY;
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


            //			SPoint Vek = new SPoint();
            foreach (var Vek in vektorList)
            {
                if (Vek.X != 0)
                {
                    if (Vek.X > 0)
                        list.Add(new RelativeWay(Vek.X, RelativeWayDirection.Right));
                    else
                        list.Add(new RelativeWay(Vek.X, RelativeWayDirection.Left));
                }
                if (Vek.Y != 0)
                {
                    if (Vek.Y > 0)
                        list.Add(new RelativeWay(Vek.Y, RelativeWayDirection.Down));
                    else
                        list.Add(new RelativeWay(Vek.Y, RelativeWayDirection.Up));
                }
            }
            return list;
        }

    }

    public class Vector2iList : List<Vector2i>
    {

    }

    public enum MarbleRegion
    {
        Default
    }

    public class PathFinding
    {

        public WayPointMap Map;
        private WayPointHeap OpenList = new WayPointHeap();
        private Vector2i StartPos;
        private Vector2i ZielPos;
        private MarbleRegion[] Regionen;
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
        public Vector2iList FindPath(Vector2i start, Vector2i ziel, MarbleRegion[] regionen, JumpHolePointToPointList jumpHoles)
        {
            //LockActions()
            if (_Searching)
                throw new Exception("Lock Error");
            Regionen = regionen;
            StartPos = start;
            ZielPos = ziel;

            WayPoint wp = null;
            try
            {
                _Searching = true;

                Map.Reset();

                Map.JumpHoles = jumpHoles;
                AllowJumpHoles = jumpHoles.Count != 0;

                if (StartPos.Equals(ZielPos))
                    return null;
                if (!(Allowed(Map[ZielPos])))
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
                    if (wp.Position.Equals(ZielPos))
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
            return p.GCost < ushort.MaxValue && Array.IndexOf(Regionen, p.Region) > -1;
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
                pos = ZielPos;

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

            var result = ManhattanDistance(wp.Position, ZielPos);
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
                            var fromAktPoint = ManhattanDistance(aktPoint, ZielPos) + cost;
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

        //******************************************************************************************************************
        //* Klasse	TWegPunktStack																																													*
        //******************************************************************************************************************
        public class WayPointStack : Stack
        {

            public void Add(WayPoint itm)
            {
                if (itm != null)
                    Push(itm);
            }

        }

    }


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
                    pos = pos / 2;
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
                    pos = pos / 2;
                }
                else
                {
                    break;
                }
            }
        }

        private int Compare(WayPoint wp1, WayPoint wp2)
        {
            return (wp1.GWCost + wp1.HCost) - (wp2.GWCost + wp2.HCost);
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

    public class WayPointMap
    {

        public int JumpCost = 10;
        public int MinSektorCost = 10;
        private int LenX;
        private int LenY;
        public WayPoint[,] ar;
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

                if (ar[x, y] == null)
                {
                    var itm = new WayPoint(this);
                    itm.Position = new Vector2i(x, y);
                    //itm.GCost = CByte(SektorMapArray(x, y).DefaultPlanet.EinflugKosten)
                    itm.GCost = (ushort)Board.GetPathFindingCosts(x, y);
                    itm.IsClosed = false;
                    itm.Region = MarbleRegion.Default;
                    ar[x, y] = itm;
                }
                return ar[x, y];
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
            if (ar == null)
            {
                ar = new WayPoint[LenX, LenY];
                return;
            }
            var x = 0;
            var y = 0;
            for (y = 0; y < LenY; y++)
                for (x = 0; x < LenX; x++)
                {
                    if (!(ar[x, y] == null))
                    {
                        ar[x, y].Parent = null;
                        ar[x, y].GWCost = 0;
                        ar[x, y].HCost = 0;
                        ar[x, y].OpenIndex = 0;
                        ar[x, y].IsClosed = false;
                        //---
                        ar[x, y].GCost = Board.GetPathFindingCosts(x, y);
                    }
                }
        }

        public void Clear()
        {
            ar = null;
        }

    }

    public class JumpHolePoint
    {
        public Vector2i Position;
        public bool Used = false;

        public JumpHolePoint(Vector2i pos)
        {
            Position = pos;
        }
    }

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

    public class JumpHolePointToPointList
    {
        private SortedList<Vector2i, JumpHolePointList> InnerList = new SortedList<Vector2i, JumpHolePointList>();

        public void Add(Vector2i start, Vector2i ziel)
        {
            JumpHolePointList list = null;
            if (!InnerList.TryGetValue(start, out list))
            {
                list = new JumpHolePointList();
                list.Position = start;
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