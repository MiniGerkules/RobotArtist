using System.Numerics;

namespace GUI
{
    class Stroke
    {
        private readonly Vector<uint> start;
        private readonly Vector<uint> end;
        private readonly Color color;

        public Stroke(Vector<uint> start, Vector<uint> end, Color color)
        {
            this.start = start;
            this.end = end;
            this.color = color;
        }
    }
}
