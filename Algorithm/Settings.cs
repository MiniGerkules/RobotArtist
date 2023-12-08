using System;
using System.Windows.Media.Imaging;

namespace Algorithm
{
    /// <summary>
    /// Settings is a class including parameters for tracing -
    /// the Color of the canvas, amount of total iterations, do blur or not,
    /// go normal to gradients or not, GuiTrace, possible canvas Color fault,
    /// amount of iterations with small overlap, minimum and maximum Length factor,
    /// minimum and maximum initial overlap ratio, pixel tolerance: possible, average
    /// and tolerance when Error is small enough to accept the stroke immideately,
    /// is there a need to use 8-Color paints (CMYBW-Color) or not (use RGB)
    /// </summary>
    public class Settings
    {
        public byte CanvasColor { get; private set; } // #CanvasColor
        public uint AmountOfTotalIters { get; private set; } // #TotalIter
        public bool DoBlur { get; private set; } // #do_blur
        public bool GoNormal { get; private set; } // #gonormal
        public GUITrace GuiTrace { get; private set; } // #GUI_Trace
        public double CanvasColorFault { get; private set; } // #canvasEps - possible Color deviation from the original canvas Color
        public uint ItersAmountWithSmallOverlap { get; private set; } // #itersMinOverlap
        public int[] MinLenFactor { get; private set; } // #minlenfactor - min Length of the stroke in brush's diameters
        public int MaxLenFactor { get; private set; } // #maxlenfactor - max Length of the stroke in brush's diameters
        public double MinInitOverlapRatio { get; private set; } // #minOverlap
        public double MaxInitOverlapRatio { get; private set; } // #maxOverlap
        public double PixTol { get; private set; } // #PixTol - possible Color deviation from the original at the end 
        public double PixTolAverage { get; private set; } // #pixTol2
        public double PixTolAccept { get; private set; } // Error at which a stroke is unconditionally accepted
        public bool UseColor8Paints { get; private set; } // if false than use usual Color, else use Color in 8 paints

        public Settings(
            GUITrace guiTrace,
            byte canvasColor = 255, // all 255 is white tone of the canvas #CanvasColor
            uint amountOfTotalIters = 3, // #TotalIter
            bool doBlur = false, // true,
            bool goNormal = true, // #gonormal - if true than strokes are drawn perpendicular to the gradient, if false - than along
            double canvasColorFault = 2,
            uint itersAmountWithSmallOverlap = 1, // #itersMinOverlap
            int[] minLenFactor = null, // #minlenfactor
            int maxLenFactor = 30, // #maxlenfactor
            double minInitOverlapRatio = 0.6,
            double maxInitOverlapRatio = 0.8,
            double pixTol = 9, // 6, // possible Color deviation from the original at the end
            double pixTolAverage = 100,
            double pixTolAccept = 4, // Error at which a stroke is unconditionally accepted
            bool useColor8Paints = false
            )
        {
            CanvasColor = canvasColor;
            AmountOfTotalIters = amountOfTotalIters;
            DoBlur = doBlur;
            GuiTrace = guiTrace;
            CanvasColorFault = canvasColorFault;
            ItersAmountWithSmallOverlap = itersAmountWithSmallOverlap;
            MinInitOverlapRatio = minInitOverlapRatio;
            MaxInitOverlapRatio = maxInitOverlapRatio;
            PixTol = pixTol;
            PixTolAverage = pixTolAverage;
            PixTolAccept = pixTolAccept;
            GoNormal = goNormal;
            if (minLenFactor == null)
                MinLenFactor = new int[] { 3, 1, 0 };
            else
                MinLenFactor = minLenFactor; // not checking it is a good value
            MaxLenFactor = maxLenFactor;
            UseColor8Paints = useColor8Paints;
        }
    }
}
