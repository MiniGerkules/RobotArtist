using System;
using System.Windows.Media.Imaging;

namespace Algorithm
{
    public class Settings
    {
        public byte canvasColor { get; private set; } // canvasColor
        public uint amoutOfLloydIters { get; private set; } // 2, options
        public uint amountOfTotalIters { get; private set; } // TotalIter
        public bool doBlur { get; private set; } // do_blur
        public GUITrace guiTrace { get; private set; } // GUI_Trace
        public double canvasColorFault { get; private set; } // canvasEps
        public uint itersAmountWithSmallOverlap { get; private set; } // itersMinOverlap
        public double minInitOverlapRatio { get; private set; } // minOverlap
        public double maxInitOverlapRatio { get; private set; } // maxOverlap
        public double pixTol { get; private set; } // possible color deviation from the original at the end // pixTol
        public double pixTolAverage { get; private set; } // pixTol2
        public double pixTolAccept { get; private set; } // Error at which a stroke is unconditionally accepted

        public Settings(
            GUITrace guiTrace,
            byte canvasColor = 255, // all 255 is white
            uint amoutOfLloydIters = 0, // 2, options
            uint amountOfTotalIters = 2,
            bool doBlur = false, // true,
            double canvasColorFault = 2,
            uint itersAmountWithSmallOverlap = 1,
            double minInitOverlapRatio = 0.6,
            double maxInitOverlapRatio = 1, // 0.8,
            double pixTol = 15, // 6, // possible color deviation from the original at the end
            double pixTolAverage = 100,
            double pixTolAccept = 4 // Error at which a stroke is unconditionally accepted
            )
        {
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
        }
    }
}
