using System;
using OpenTK;
using System.Collections.Generic;

namespace AxEngine
{
    public class MarbleBoard
    {

        public MarbleBoard()
        {
            Width = 9;
            Height = 9;
            MarbleArray = new Marble[Width, Height];
        }

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

        public void MoveMarble(Marble marble, Vector2i target)
        {
            if (marble == null)
                return;

            RemoveMarble(marble);
            RemoveMarble(target);

            marble.Position = target;
            this[marble.Position] = marble;
            Marbles.Add(marble);
        }

        public void NewGame()
        {
            ClearBoard();
            for (var i = 0; i < 15; i++)
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
            return random.Next(maxValue);
        }

        private Vector2i GetRandomPosition()
        {
            var posX = GetRandomNumber(Width);
            var posY = GetRandomNumber(Height);
            return new Vector2i(posX, posY);
        }

        private MarbleColor GetRandomColor()
        {
            return (MarbleColor)GetRandomNumber(3);
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
        }

    }

}