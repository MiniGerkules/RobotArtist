using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using GeneralComponents;

namespace Algorithm
{
    public struct Stroke
    {
        public List<System.Windows.Point> points;
        public double[] color;
        double[] col8paints;
        double[] proportions;
        ColorMixType mixType;
        public double length = 0;
        public Stroke(System.Windows.Point point, double[] col, double[] col8paints, 
            double[] proportions, ColorMixType mixType, int length = 0)
        {
            points = new List<System.Windows.Point>();
            points.Add(point);
            color = col;
            this.col8paints = col8paints;
            this.proportions = proportions;
            this.mixType = mixType;
            this.length = length;
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