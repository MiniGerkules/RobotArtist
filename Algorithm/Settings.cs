using System;
using System.Windows.Media.Imaging;

namespace Algorithm
{
    public class Settings
    {
        public byte canvasColor { get; private set; } // #canvasColor
        //public uint amoutOfLloydIters { get; private set; } // 2, options
        public uint amountOfTotalIters { get; private set; } // #TotalIter
        public bool doBlur { get; private set; } // #do_blur
        public bool goNormal { get; private set; } // #gonormal
        public GUITrace guiTrace { get; private set; } // #GUI_Trace
        public double canvasColorFault { get; private set; } // #canvasEps - possible color deviation from the original canvas color
        public uint itersAmountWithSmallOverlap { get; private set; } // #itersMinOverlap
        public int[] minLenFactor { get; private set; } // #minlenfactor - min length of the stroke in brush's diameters
        public int maxLenFactor { get; private set; } // #maxlenfactor - max length of the stroke in brush's diameters
        public double minInitOverlapRatio { get; private set; } // #minOverlap
        public double maxInitOverlapRatio { get; private set; } // #maxOverlap
        public double pixTol { get; private set; } // #pixTol - possible color deviation from the original at the end 
        public double pixTolAverage { get; private set; } // #pixTol2
        public double pixTolAccept { get; private set; } // Error at which a stroke is unconditionally accepted

        public Settings(
            GUITrace guiTrace,
            byte canvasColor = 255, // all 255 is white tone of the canvas #canvasColor
            //uint amoutOfLloydIters = 0, // 2, options
            uint amountOfTotalIters = 3, // #TotalIter
            bool doBlur = false, // true,
            bool goNormal = true, // #gonormal - if true than strokes are drawn perpendicular to the gradient, if false - than along
            double canvasColorFault = 2,
            uint itersAmountWithSmallOverlap = 1, // #itersMinOverlap
            int[] minLenFactor = null, // #minlenfactor
            int maxLenFactor = 30, // #maxlenfactor
            double minInitOverlapRatio = 0.6,
            double maxInitOverlapRatio = 0.8,
            double pixTol = 9, // 6, // possible color deviation from the original at the end
            double pixTolAverage = 100,
            double pixTolAccept = 4 // Error at which a stroke is unconditionally accepted
            )
        {
            this.canvasColor = canvasColor;
            this.amountOfTotalIters = amountOfTotalIters;
            //this.amoutOfLloydIters = amoutOfLloydIters;
            this.doBlur = doBlur;
            this.guiTrace = guiTrace;
            this.canvasColorFault = canvasColorFault;
            this.itersAmountWithSmallOverlap = itersAmountWithSmallOverlap;
            this.minInitOverlapRatio = minInitOverlapRatio;
            this.maxInitOverlapRatio = maxInitOverlapRatio;
            this.pixTol = pixTol;
            this.pixTolAverage = pixTolAverage;
            this.pixTolAccept = pixTolAccept;
            this.goNormal = goNormal;
            if (minLenFactor == null)
                this.minLenFactor = new int[] { 3, 1, 0 };
            else
                this.minLenFactor = minLenFactor; // not checking it is a good value
            this.maxLenFactor = maxLenFactor;
        }
    }
}
