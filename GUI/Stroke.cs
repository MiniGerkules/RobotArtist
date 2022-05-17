using System.Numerics;

namespace GUI
{
    class Stroke
    {
        public Point2D Start { get; }
        public Point2D End { get; }
        public CMYBWColor StroceColor { get; }

        public Stroke(Point2D start, Point2D end, CMYBWColor color)
        {
            Start = start;
            End = end;
            StroceColor = color;
        }
    }
}
