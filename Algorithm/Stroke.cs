using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeneralComponents;
using GeneralComponents.Colors;

namespace Algorithm
{
    /// <summary>
    /// Stroke is a class describing a stroke:
    /// stroke has a Color (RGB) and a mixture type,
    /// stroke has a Color in 8 paints (CMYBW-Color) and Proportions for these paints,
    /// stroke has a Length and a list of Points that are a dots of the stroke line
    /// (one stroke consists of several strokes of the same type but other directions)
    /// </summary>
    public class Stroke
    {
        public List<System.Windows.Point> Points;
        public double[] Color;
        public double[] Col8paints;
        public double[] Proportions;
        public ColorMixType MixType;
        public double Length = 0;
        public Stroke(System.Windows.Point point, double[] col, double[] col8paints,
            double[] proportions, ColorMixType mixType, int length = 0)
        {
            Points = new List<System.Windows.Point> { point };
            Color = col;
            Col8paints = col8paints;
            Proportions = proportions;
            MixType = mixType;
            Length = length;
        }

        public List<GeneralComponents.PLT.Stroke> ConvertToPLTStroke(IColor icolor, double brushWidth, uint height)
        {
            if (icolor is RGBColor)
            {
                var answer = new List<GeneralComponents.PLT.Stroke>();
                for (int i = 1; i < Points.Count; i++)
                {
                    answer.Add(
                        new GeneralComponents.PLT.Stroke(
                        new Point2D(
                            (uint)Points[i - 1].X + 1,
                            ((uint)Points[i - 1].Y + 1)),
                        new Point2D(
                            (uint)Points[i].X + 1,
                            ((uint)Points[i].Y + 1 )),
                    new RGBColor((byte)Color[0], (byte)Color[1], (byte)Color[2]),
                    brushWidth));
                }
                return answer;
            }
            else if (icolor is CMYBWColor)
            {
                var answer = new List<GeneralComponents.PLT.Stroke>();
                for (int i = 1; i < Points.Count; i++)
                {
                    answer.Add(
                        new GeneralComponents.PLT.Stroke(
                        new Point2D(
                            (uint)Points[i - 1].X + 1,
                            ((uint)Points[i - 1].Y + 1)),
                        new Point2D(
                            (uint)Points[i].X + 1,
                            ((uint)Points[i].Y + 1)),
                    new CMYBWColor(Col8paints.Take(5).Select(x => (uint)x).ToArray()),
                    brushWidth));
                }
                return answer;
            }
            else
                throw new ArgumentException("the Color is unknown");
        }
    }
}