using System;
using System.Windows.Media.Imaging;

namespace Algorithm
{
    public class StrokeBrush
    {
        public int thickness { get; private set; } // #brushSize
        public int smallThickness { get; private set; } // #bs2
        public int maxStrokeLength { get; private set; } // #maxLen
        public double bsQuad { get; private set; } // #bsQuad
        public StrokeBrush(double brushWidthMM, double sfX) // sfX is a coefficient <= 1
        {
            //try
            //{
                this.thickness = (int)Math.Round(brushWidthMM / sfX);
                if (thickness == 0)
                    throw new ArgumentException();
                this.smallThickness = (int)Math.Ceiling((double)thickness / 2);
                this.maxStrokeLength = thickness * 10;
                this.bsQuad = ((double)thickness / 2) * ((double)thickness / 2); // is that ok?
            //}
            //catch (ArgumentException e)
            //{
            //    Console.WriteLine("Thickness is 0 because brush width is greater than sfX coefficient");
            //    Console.WriteLine(e.Message);
            //}
        }

    }
}