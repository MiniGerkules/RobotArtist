using GUI.Colors;
using GeneralComponents;

namespace GUI.PLT {
    public class Stroke {
        public Point2D Start { get; }
        public Point2D End { get; }
        public IColor StroceColor { get; }
        public double BrushWidth { get; }

        public Stroke(Point2D start, Point2D end, IColor color, double brushWidth) {
            Start = start;
            End = end;
            StroceColor = color;
            BrushWidth = brushWidth;
        }
    }
}
