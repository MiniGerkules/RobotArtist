using System;
using System.Collections.Generic;
using GeneralComponents;
using GeneralComponents.Colors;

namespace Algorithm
{
    public class Stroke
    {
        public List<System.Windows.Point> points;
        public double[] color;
        public double[] col8paints;
        public double[] proportions;
        public ColorMixType mixType;
        public double length = 0;
        public Stroke(System.Windows.Point point, double[] col, double[] col8paints, 
            double[] proportions, ColorMixType mixType, int length = 0)
        {
            points = new List<System.Windows.Point>{ point };
            color = col;
            this.col8paints = col8paints;
            this.proportions = proportions;
            this.mixType = mixType;
            this.length = length;
        }

        public List<GeneralComponents.PLT.Stroke> ConvertToPLTStroke(IColor icolor, double brushWidth)
        {
            if (icolor is RGBColor)
            {
                if (points.Count == 1)
                    return new List<GeneralComponents.PLT.Stroke>()
                { new GeneralComponents.PLT.Stroke(
                    new Point2D(
                        (uint)points[0].X,
                        (uint)points[0].Y),
                    new Point2D(
                        (uint)points[0].X,
                        (uint)points[0].Y),
                    new RGBColor((byte)color[0], (byte)color[1], (byte)color[2]),
                    brushWidth) };
                else
                {
                    var answer = new List<GeneralComponents.PLT.Stroke>();
                    for (int i = 1; i < points.Count; i++)
                    {
                        answer.Add(
                            new GeneralComponents.PLT.Stroke(
                            new Point2D(
                                (uint)points[i - 1].X,
                                (uint)points[i - 1].Y),
                            new Point2D(
                                (uint)points[i].X,
                                (uint)points[i].Y),
                        new RGBColor((byte)color[0], (byte)color[1], (byte)color[2]),
                        brushWidth));
                    }
                    return answer;
                }
            }
            else if (icolor is CMYBWColor)
            {
                // how to create this??? 
                // 1 color = 8 numbers where every number is a proportion of color
                throw new NotImplementedException();
            }
            else
                throw new ArgumentException();
        }
    }

    public struct StrokeCandidate
    {
        public double x;
        public double y;
        public double error;

        public StrokeCandidate (double newX, double newY, double err = double.MaxValue)
        {
            x = newX;
            y = newY;
            error = err;
        }
    }
}