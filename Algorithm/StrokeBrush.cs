using System;
using System.Windows.Media.Imaging;

namespace Algorithm
{
    public class StrokeBrush
    {
        public uint thickness { get; private set; }
        public uint smallThickness { get; private set; }
        public uint maxStrokeLength { get; private set; }
        public double bsQuad { get; private set; }
        public StrokeBrush(double brushWidthMM, double sfX) // sfX is a coefficient <= 1
        {
            //try
            //{
                this.thickness = (uint)Math.Round(brushWidthMM / sfX);
                if (thickness == 0)
                    throw new ArgumentException();
                this.smallThickness = (uint)Math.Ceiling((double)thickness / 2);
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