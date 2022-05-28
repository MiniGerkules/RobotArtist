using System;
using System.Windows.Media.Imaging;

namespace Algorithm
{
    public class StrokeBrush
    {
        public uint thickness { get; private set; }
        public uint smallThickness { get; private set; }
        public uint maxStrokeLength  { get; private set; }
        public uint bsQuad { get; private set; }
        public StrokeBrush(double brushWidthMM, double sfX)
        {
            this.thickness = (uint)Math.Round(brushWidthMM / sfX);
            this.smallThickness = (uint)Math.Ceiling((double)thickness / 2);
            this.maxStrokeLength = thickness * 10;
            this.bsQuad = (uint)(((double)thickness / 2) * ((double)thickness / 2)); // is that ok?
        }

    }
    public struct GUITrace
    {
        public double brushWidthMM { get; private set; }
        public double canvasWidthMM { get; private set; }
        public double canvasHeightMM { get; private set; }
        public uint usespp { get; private set; }
        public uint colorsAmount { get; private set; }
        public GUITrace(uint colorsAmount, uint usespp = 0, params double[] sizesListMM)
        {
            this.usespp = usespp;
            this.colorsAmount = colorsAmount;
            this.brushWidthMM = sizesListMM[0];
            this.canvasWidthMM = sizesListMM[1];
            this.canvasHeightMM = sizesListMM[2];

        }

    }

    public class Settings
    {
        public BitmapImage image { get; private set; }
        public byte canvasColor { get; private set; }
        public uint amoutOfLloydIters { get; private set; } // 2, options
        public uint amountOfTotalIters { get; private set; }
        public bool doBlur { get; private set; }
        public GUITrace guiTrace { get; private set; }
        public double canvasColorFault { get; private set; }
        public uint itersAmountWithSmallOverlap { get; private set; }
        public double minInitOverlapRatio { get; private set; }
        public double maxInitOverlapRatio { get; private set; }
        public double pixTol { get; private set; } // possible color deviation from the original at the end
        public double pixTolAverage { get; private set; }
        public double pixTolAccept { get; private set; } // error at which a stroke is unconditionally accepted

        //public double sfX { get; private set; }
        //public double sfY { get; private set; }
        public StrokeBrush strokeBrush { get; private set; }
        public uint maxAmountOfIters { get; private set; }
        public Settings(
            BitmapImage image,
            GUITrace guiTrace,
            byte canvasColor = 255,
            uint amoutOfLloydIters = 0, // 2, options
            uint amountOfTotalIters = 2,
            bool doBlur = true,
            double canvasColorFault = 2,
            uint itersAmountWithSmallOverlap = 1,
            double minInitOverlapRatio = 0.6,
            double maxInitOverlapRatio = 1,
            double pixTol = 6, // possible color deviation from the original at the end
            double pixTolAverage = 100,
            double pixTolAccept = 4 // error at which a stroke is unconditionally accepted
            )
        {
            // just equals properties:

            this.image = image;
            this.canvasColor = canvasColor;
            this.amountOfTotalIters = amountOfTotalIters;
            this.amoutOfLloydIters = amoutOfLloydIters;
            this.doBlur = doBlur;
            this.guiTrace = guiTrace;
            this.canvasColorFault = canvasColorFault;
            this.itersAmountWithSmallOverlap = itersAmountWithSmallOverlap;
            this.minInitOverlapRatio = minInitOverlapRatio;
            this.maxInitOverlapRatio = maxInitOverlapRatio;
            this.pixTol = pixTol;
            this.pixTolAverage = pixTolAverage;
            this.pixTolAccept = pixTolAccept;

            // counting properties:
            // матрица mxn => m строк, n столбцов => n = width, m = height
            //sfY = sfX = Math.Min(guiTrace.canvasWidthMM / image.PixelWidth, 
            //    guiTrace.canvasHeightMM / image.PixelHeight); // from size in mm get size in pix?
            strokeBrush = new StrokeBrush(guiTrace.brushWidthMM, 
                Math.Min(guiTrace.canvasWidthMM / image.PixelWidth, guiTrace.canvasHeightMM / image.PixelHeight));

            maxAmountOfIters = strokeBrush.thickness;

        }
    }
}
