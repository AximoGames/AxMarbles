using System;
using OpenTK;
using System.Collections.Generic;
using System.Linq;

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

        private List<Marble> LastCreatedMarbles = new List<Marble>();

        public void CreateRandomMarbles()
        {
            LastCreatedMarbles.Clear();
            for (var i = 0; i < 3; i++)
            {
                var newMarble = CreateRandomMarble();
                if (newMarble != null)
                {
                    newMarble.State = MarbleState.Adding;
                    LastCreatedMarbles.Add(newMarble);
                }
            }
            if (LastCreatedMarbles.Count > 0)
                OnNewMarbles();
        }

        public void CreatedAnimationFinished()
        {
            foreach (var marble in Marbles)
            {
                if (marble.State == MarbleState.Adding)
                {
                    marble.State = MarbleState.Default;

                    if (CheckMatch(marble.Position))
                    {
                        return;
                    }
                }
            }
        }

        public Marble CreateMarble(Vector2i pos, MarbleColor color)
        {
            if (this[pos] != null)
                throw new Exception($"Position {pos} not free");

            Console.WriteLine($"Create {color} Marble at {pos}");
            var marble = new Marble(color);
            marble.Position = pos;
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
        public bool CheckMatch(Vector2i origin)
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
                        if (marble.Color == MarbleColor.BombJoker)
                            ApplyBomb(marble, result.PrimaryColor);
                    }
                }
            }
            if (Matches.Count > 0)
            {
                OnMatch();
                return true;
            }
            else
            {
                return false;
            }
        }

        private List<Marble> BombedMarbles = new List<Marble>();

        private void ApplyBomb(Marble bomb, MarbleColor primaryColor)
        {
            foreach (var mar in Marbles)
            {
                if (mar.Color == primaryColor)
                {
                    var isRegular = false;
                    foreach (var result in Matches)
                    {
                        foreach (var resultMarble in result.Marbles)
                        {
                            if (resultMarble == mar)
                            {
                                isRegular = true;
                                break;
                            }
                        }
                    }
                    if (!isRegular)
                    {
                        mar.State = MarbleState.Removing;
                        BombedMarbles.Add(mar);
                    }
                }
            }
        }

        public void ScoreMatches()
        {
            LastMoveScore = GetMoveScores();
            TotalScore += LastMoveScore;
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
            Console.WriteLine($"FreePositions: {FreePositions.Count}");
        }

        private int GetMoveScores()
        {
            var sum = 0;
            foreach (var match in Matches)
            {
                var matchScore = 10;
                matchScore += Math.Max(match.Marbles.Count - 5, 0) * 10; // More then 5 Marbles
                sum += matchScore;
            }

            sum *= Matches.Count;

            sum += BombedMarbles.Count * 10;

            return sum;
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
                result.CalculatePrimaryColor();
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
            if (m1.Color == m2.Color)
                return true;

            var colors1 = m1.Color.GetRegularColors();
            var colors2 = m2.Color.GetRegularColors();
            foreach (var c1 in colors1)
                if (colors2.Contains(c1))
                    return true;

            return false;
        }

        public bool PositionInMap(Vector2i pos)
        {
            return pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height;
        }

        public class CheckRowResult
        {
            public List<Marble> Marbles = new List<Marble>();
            public int Score;
            public bool Valid;

            public MarbleColor PrimaryColor;

            public void CalculatePrimaryColor()
            {
                if (!Valid)
                    return;

                var colorHash = new Dictionary<MarbleColor, int>();
                foreach (var col in MarbleBoard.AllColors)
                    colorHash.Add(col, 0);

                foreach (var mar in Marbles)
                {
                    var marbleColors = mar.Color.GetRegularColors();
                    foreach (var col in marbleColors)
                        colorHash[col] += 1;
                }

                if (colorHash.Count > 0)
                    PrimaryColor = colorHash.OrderByDescending(e => e.Value).First().Key;
            }

        }

        public void NewGame()
        {
            ClearBoard();
            CreateRandomMarbles();
            //CreateTestmarbles();
        }

        private void CreateTestmarbles()
        {
            CreateMarble(new Vector2i(0, 2), MarbleColor.Red);
            CreateMarble(new Vector2i(0, 3), MarbleColor.Red);
            CreateMarble(new Vector2i(0, 4), MarbleColor.Red);
            CreateMarble(new Vector2i(0, 5), MarbleColor.BombJoker);
            CreateMarble(new Vector2i(2, 5), MarbleColor.Red);
            CreateMarble(new Vector2i(2, 6), MarbleColor.Red);
            CreateMarble(new Vector2i(2, 7), MarbleColor.Green);

            OnNewMarbles();
        }

        private int RandomMarblesCreated = 0;

        private Marble CreateRandomMarble()
        {
            RandomMarblesCreated++;
            // if (RandomMarblesCreated == 1) // Debug
            // {
            //     return CreateMarble(new Vector2i(0, 6), MarbleColor.Red);
            // }

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
            return random.Next(maxValue + 1);
        }

        private HashSet<Vector2i> FreePositions = new HashSet<Vector2i>();

        private Vector2i GetRandomPosition()
        {
            var pos = FreePositions.ToArray()[GetRandomNumber(FreePositions.Count - 1)];
            if (this[pos] != null)
                throw new Exception($"this[{pos}] != null");

            return pos;
        }

        private MarbleColor GetRandomColor()
        {
            var color = GetRandomColorInternal();
            if (GetRandomNumber(3) == 0)
            {
                if (GetRandomNumber(1) == 0)
                {
                    return MarbleColor.BombJoker;
                }
                else
                {
                    return MarbleColor.ColorJoker;
                }
            }
            else if (GetRandomNumber(3) == 0)
            {
                var color2 = GetRandomColorInternal();
                return color | color2;
            }
            else
            {
                return color;
            }
        }

        public static MarbleColor[] AllColors = new MarbleColor[] {
                MarbleColor.Red,
                MarbleColor.Green,
                MarbleColor.Blue,
                MarbleColor.Yellow ,
                MarbleColor.Orange,
                MarbleColor.White,
                MarbleColor.Black,
            };

        private MarbleColor GetRandomColorInternal()
        {
            return AllColors[GetRandomNumber(AllColors.Length - 1)];
        }

        public int TotalScore { get; private set; }
        public int LastMoveScore { get; private set; }

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
            foreach (var marble in Marbles.ToArray())
            {
                RemoveMarble(marble);
            }
            Marbles.Clear();
            BombedMarbles.Clear();
            Matches.Clear();
            TotalScore = 0;
            LastMoveScore = 0;
            FreePositions = new HashSet<Vector2i>(AllPositions);
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
            BombedMarbles.Remove(marble);
            this[marble.Position] = null;
            FreePositions.Add(marble.Position);
        }

    }

}