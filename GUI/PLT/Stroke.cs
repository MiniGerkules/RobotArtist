using GUI.Colors;
using GeneralComponents;

namespace GUI.PLT {
    class Stroke {
        public Point2D Start { get; }
        public Point2D End { get; }
        public PLTColor StroceColor { get; }

        public Stroke(Point2D start, Point2D end, PLTColor color) {
            Start = start;
            End = end;
            StroceColor = color;
        }
    }
}
