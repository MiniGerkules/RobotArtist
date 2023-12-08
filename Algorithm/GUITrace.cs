using System;
using System.Windows.Media.Imaging;

namespace Algorithm
{
    /// <summary>
    /// GUITrace is a struct including parameters for tracing -
    /// the canvas sizes, brush width and Color amount
    /// </summary>
    public struct GUITrace // GUI_Trace
    {
        public double BrushWidthMM { get; private set; } // #brushWidth_mm
        public double CanvasWidthMM { get; private set; } // #canvasW_mm
        public double CanvasHeightMM { get; private set; } // #canvasH_mm
        public uint ColorsAmount { get; private set; } // amount of colors in the final picture // nColors
        public GUITrace(uint colorsAmount = 255, double brushWidthMM = 2, double canvasWidthMM = 10, double canvasHeightMM = 10)
        {
            ColorsAmount = colorsAmount;
            BrushWidthMM = brushWidthMM;
            CanvasWidthMM = canvasWidthMM;
            CanvasHeightMM = canvasHeightMM;
        }
    }
}