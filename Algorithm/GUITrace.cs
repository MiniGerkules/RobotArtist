using System;
using System.Windows.Media.Imaging;

namespace Algorithm
{
    public struct GUITrace // GUI_Trace
    {
        public double brushWidthMM { get; private set; } // #brushWidth_mm
        public double canvasWidthMM { get; private set; } // #canvasW_mm
        public double canvasHeightMM { get; private set; } // #canvasH_mm
        public uint colorsAmount { get; private set; } // amount of colors in the final picture // nColors
        public GUITrace(uint colorsAmount = 255, double brushWidthMM = 2, double canvasWidthMM = 10, double canvasHeightMM = 10)
        {
            this.colorsAmount = colorsAmount;
            this.brushWidthMM = brushWidthMM;
            this.canvasWidthMM = canvasWidthMM;
            this.canvasHeightMM = canvasHeightMM;
        }
    }
}