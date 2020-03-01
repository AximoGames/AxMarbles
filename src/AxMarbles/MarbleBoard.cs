using System;
using OpenTK;
using System.Collections.Generic;

namespace AxEngine
{

    public enum MarbleState
    {
        Default,
        Adding,
        Removing,
    }

    public class MarbleBoard
    {

        public ushort GetPathFindingCosts(int x, int y)
        {
            var marble = this[x, y];
            if (marble == null)
                return 10;
            return ushort.MaxValue;
        }

        public MarbleBoard()
        {
            Width = 9;
            Height = 9;
            MarbleArray = new Marble[Width, Height];

            PathFinding = new PathFinding();
            PathFinding.Map = new WayPointMap(this);
        }

        private PathFinding PathFinding;

        public int Width { get; private set; }
        public int Height { get; private set; }

        private Marble[,] MarbleArray;

        public Marble this[int x, int y]
        {
            get => MarbleArray[x, y];
            private set => MarbleArray[x, y] = value;
        }

        public Marble this[Vector2i pos]
        {
            get => MarbleArray[pos.X, pos.Y];
            private set => MarbleArray[pos.X, pos.Y] = value;
        }

        public Marble CreateMarble(Vector2i pos, MarbleColor color)
        {
            var marble = new Marble(color);
            MoveMarble(marble, pos);
            return marble;
        }

        public void MoveMarble(Vector2i pos, Vector2i target)
        {
            MoveMarble(this[pos], target);
        }

        public Vector2iList FindPath(Marble marble, Vector2i target)
        {
            return PathFinding.FindPath(marble.Position, target, new MarbleRegion[] { MarbleRegion.Default });
        }

        public void MoveMarble(Marble marble, Vector2i target)
        {
            if (marble == null)
                return;

            RemoveMarble(marble);
            RemoveMarble(target);

            marble.Position = target;
            this[marble.Position] = marble;
            Marbles.Add(marble);
            FreePositions.Remove(marble.Position);
        }

        public Action OnMatch;
        public Action OnNewMarbles;

        private List<CheckRowResult> Matches = new List<CheckRowResult>();
        public void CheckMatch(Vector2i origin)
        {
            var results = new List<CheckRowResult>();
            results.Add(CheckRow(origin, new Vector2i(1, 0)));
            results.Add(CheckRow(origin, new Vector2i(0, 1)));
            results.Add(CheckRow(origin, new Vector2i(1, 1)));
            results.Add(CheckRow(origin, new Vector2i(1, -1)));
            foreach (var result in results)
            {
                if (result.Valid)
                {
                    Matches.Add(result);
                    foreach (var marble in result.Marbles)
                    {
                        marble.State = MarbleState.Removing;
                    }
                }
            }
            if (Matches.Count > 0)
            {
                OnMatch();
            }
            else
            {
                for (var i = 0; i < 3; i++)
                {
                    var newMarble = CreateRandomMarble();
                    newMarble.State = MarbleState.Adding;
                }
                OnNewMarbles();
            }
        }

        public void ScoreMatches()
        {
            for (var i = Marbles.Count - 1; i >= 0; i--)
            {
                var marble = Marbles[i];
                if (marble.State == MarbleState.Removing)
                {
                    RemoveMarble(marble);
                    marble.Dispose();
                    Matches.Clear();
                }
            }
        }

        private CheckRowResult CheckRow(Vector2i origin, Vector2i step)
        {
            var result = new CheckRowResult();
            CheckRow2(origin, step, result);
            CheckRow2(origin, step * -1, result);
            result.Marbles.Add(this[origin]);
            if (result.Marbles.Count >= 5)
            {
                Console.WriteLine("MATCH");
                result.Valid = true;
            }
            return result;
        }

        private void CheckRow2(Vector2i origin, Vector2i step, CheckRowResult result)
        {
            var originMarble = this[origin];
            var pos = origin + step;
            while (PositionInMap(pos))
            {
                if (MarblesAreCompatible(originMarble, this[pos]))
                {
                    result.Marbles.Add(this[pos]);
                }
                else
                {
                    break;
                }
                pos += step;
            }
        }

        private bool MarblesAreCompatible(Marble m1, Marble m2)
        {
            if (m1 == null || m2 == null)
                return false;
            return m1.Color == m2.Color;
        }

        private bool PositionInMap(Vector2i pos)
        {
            return pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height;
        }

        public class CheckRowResult
        {
            public List<Marble> Marbles = new List<Marble>();
            public int Score;
            public bool Valid;
        }

        public void NewGame()
        {
            ClearBoard();
            for (var i = 0; i < 1; i++)
            {
                CreateRandomMarble();
            }
        }

        private Marble CreateRandomMarble()
        {
            var pos = GetRandomPosition();
            var color = GetRandomColor();
            var marble = CreateMarble(pos, color);
            return marble;
        }

        private Random random = new Random(9);
        private int GetRandomNumber(int maxValue)
        {
            if (maxValue == 0)
                return 0;
            return random.Next(maxValue);
        }

        private List<Vector2i> FreePositions = new List<Vector2i>();

        private Vector2i GetRandomPosition()
        {
            return FreePositions[GetRandomNumber(FreePositions.Count)];
        }

        private MarbleColor GetRandomColor()
        {
            var colors = new MarbleColor[] {
                MarbleColor.Red,
                MarbleColor.Green,
                MarbleColor.Blue,
                MarbleColor.Yellow ,
                MarbleColor.Orange,
                MarbleColor.White,
                MarbleColor.Black,
            };
            return colors[GetRandomNumber(colors.Length)];
        }

        public IEnumerable<Vector2i> AllPositions
        {
            get
            {
                for (var y = 0; y < Height; y++)
                    for (var x = 0; x < Width; x++)
                        yield return new Vector2i(x, y);
            }
        }

        public List<Marble> Marbles = new List<Marble>();

        public void ClearBoard()
        {
            Marbles.Clear();
            FreePositions.AddRange(AllPositions);
            foreach (var marble in Marbles.ToArray())
            {
                RemoveMarble(marble);
            }
        }

        public void RemoveMarble(Vector2i pos)
        {
            RemoveMarble(this[pos]);
        }

        public void RemoveMarble(Marble marble)
        {
            if (marble == null)
                return;

            Marbles.Remove(marble);
            this[marble.Position] = null;
            FreePositions.Add(marble.Position);
        }

    }

}