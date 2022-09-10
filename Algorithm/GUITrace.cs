using System;
using System.Windows.Media.Imaging;

namespace Algorithm
{
    public struct GUITrace // GUI_Trace
    {
        // this constructor sholdn't be used, use constructor with parameters
        public GUITrace() // just some parameters to avoid fatal errors
        {
            this.colorsAmount = 120;
            this.brushWidthMM = 2;
            this.canvasWidthMM = 10; //390; 
            this.canvasHeightMM = 10; //290;
        }
        public double brushWidthMM { get; private set; } // brushWidth_mm
        public double canvasWidthMM { get; private set; } // canvasW_mm
        public double canvasHeightMM { get; private set; } // canvasH_mm
        //public uint usespp { get; private set; } // всегда PP, не PC - кодирование в пропорциях красок
        public uint colorsAmount { get; private set; } // amount of colors in the final picture // nColors
        public GUITrace(uint colorsAmount = 256, params double[] sizesListMM) // is that ok to set colorsAmount to 256?
        {
            this.colorsAmount = colorsAmount;
            this.brushWidthMM = 0;
            this.canvasWidthMM = 0;
            this.canvasHeightMM = 0;
            if (sizesListMM != null)
            {
                if (sizesListMM.Length >= 3)
                {
                    this.brushWidthMM = sizesListMM[0];
                    this.canvasWidthMM = sizesListMM[1];
                    this.canvasHeightMM = sizesListMM[2];
                }
                else throw new ArgumentException("sizes for brush width and canvas width and height should be done!");
            }
        }
    }
}