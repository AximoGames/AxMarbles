using System;
using OpenTK;

namespace AxEngine
{
    public class Marble : IDisposable
    {

        public Marble(MarbleColor color)
        {
            Color = color;
        }

        public MarbleColor Color;
        public Vector2i Position;

        public CubeObject RenderObject;
        public MarbleState State;

        public void Dispose()
        {
        }
    }

}