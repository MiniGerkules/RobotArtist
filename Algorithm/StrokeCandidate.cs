using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorithm
{
    /// <summary>
    /// StrokeCandidate is a struct describing the point of (X, Y)
    /// with Error that will be if stroke will continue to that point
    /// </summary>
    public struct StrokeCandidate
    {
        public double X;
        public double Y;
        public double Error;

        public StrokeCandidate(double newX, double newY, double err = double.MaxValue)
        {
            X = newX;
            Y = newY;
            Error = err;
        }
    }
}
