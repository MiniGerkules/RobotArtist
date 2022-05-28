using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace Algorithm
{
    public struct Strokes
    {
        Color color;
        List<int> beginXPositions;
        List<int> beginYPositions;
        int length;
        public Strokes(Color col, int len = 0)
        {
            color = col;
            beginXPositions = new List<int>();
            beginYPositions = new List<int>();
            length = len;
        }
    }

    public struct StrokeCandidate
    {
        int x;
        int y;
        double error;

        public StrokeCandidate (int newX, int newY, double err = double.MaxValue)
        {
            x = newX;
            y = newY;
            error = err;
        }
    }
}