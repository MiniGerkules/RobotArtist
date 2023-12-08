using System;
using System.Windows.Media.Imaging;

namespace Algorithm
{
    /// <summary>
    /// StrokeBrush is a class describing the brush for drawing strokes
    /// it has parameters: Thickness, SmallThickness, maximum Length of the stroke, 
    /// and the size of brush = (Thickness/2)^2
    /// </summary>
    public class StrokeBrush
    {
        public int Thickness { get; private set; } // #brushSize
        public int SmallThickness { get; private set; } // #bs2
        public int MaxStrokeLength { get; private set; } // #maxLen
        public double BsQuad { get; private set; } // #bsQuad
        public StrokeBrush(double brushWidthMM, double sfX, int maxLenFactor) // sfX is a coefficient <= 1
        {
            Thickness = Math.Max(2, (int)Math.Round(brushWidthMM / sfX));
            SmallThickness = Math.Max(1, (int)Math.Ceiling((double)Thickness / 2));
            MaxStrokeLength = Thickness * maxLenFactor;
            BsQuad = ((double)Thickness / 2d) * ((double)Thickness / 2d); // is that ok?
        }

    }
}