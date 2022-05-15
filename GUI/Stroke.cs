using System.Numerics;

namespace GUI
{
    class Stroke
    {
        private readonly Point2D start;
        private readonly Point2D end;
        private readonly Color color;

        public Stroke(Point2D start, Point2D end, Color color)
        {
            this.start = start;
            this.end = end;
            this.color = color;
        }
    }
}
