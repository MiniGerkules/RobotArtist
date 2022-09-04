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
        public StrokeBrush(double brushWidthMM, double sfX)
        {
            this.thickness = (uint)Math.Round(brushWidthMM / sfX); // troubles here, thickness is gonna be zero when brushWidthMM < sfX when it is almost always should be like that
            this.smallThickness = (uint)Math.Ceiling((double)thickness / 2);
            this.maxStrokeLength = thickness * 10;
            this.bsQuad = ((double)thickness / 2) * ((double)thickness / 2); // is that ok?
        }

    }
}