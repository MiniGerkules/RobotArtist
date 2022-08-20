using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.IO;

namespace GUI
{
    /// <summary>
    /// The class describes the plt code decoder
    /// </summary>
    class PLTDecoder
    {
        public readonly static uint numTicksInMM = 40;
        public uint MaxX { get; private set; } = 0;
        public uint MaxY { get; private set; } = 0;

        private readonly List<Stroke> decodedPlt = new();
        private Point2D? lastPoint = null;
        private PLTColor curColor = null;

        /// <summary>
        /// The method decodes the plt code passed in the string
        /// </summary>
        /// <param name="pltCode"> A string containing the plt code </param>
        /// <returns> List of strokes with specified colors </returns>
        public List<Stroke> Decode(string pltCode)
        {
            if (pltCode[..2].ToLower() != "in" || pltCode[pltCode.Length - 1] != ';')
                throw new ArgumentException("ERROR! Invalid code. PLT-code should start with [IN] operator!");

            pltCode = pltCode[3..(pltCode.Length - 1)];
            NewDecode();

            foreach (string part in pltCode.Split(';'))
                ProcessPart(part);

            return decodedPlt;
        }

        private void NewDecode()
        {
            decodedPlt.Clear();
            lastPoint = null;
            curColor = null;
            MaxX = 0;
            MaxY = 0;
        }

        private void ProcessPart(string part)
        {
            switch (part[..2])
            {
                case "PP":
                    curColor = new CMYBWColor(part[2..].Split(','));
                    break;
                case "PC":
                    curColor = new RGBColor(part[2..].Split(','));
                    break;
                case "PD":
                    ProcessPDCommand(part);
                    break;
                case "PU":
                    lastPoint = null;
                    break;
                default:
                    throw new ArgumentException($"ERROR! Unknown [{part[..2]}] operator!");
            }
        }

        private void ProcessPDCommand(string command)
        {
            if (curColor == null)
                throw new ArgumentException("Invalid plt code. Color don't set before painting!");

            uint[] coords = command[2..].Split(',').Select(elem => uint.Parse(elem)).ToArray();
            Point2D newPoint = new(coords);
            newPoint.Divide(numTicksInMM);

            MaxX = Math.Max(MaxX, newPoint.X);
            MaxY = Math.Max(MaxY, newPoint.Y);

            if (lastPoint != null)
                decodedPlt.Add(new(lastPoint.Value, newPoint, curColor));

            lastPoint = newPoint;
        }
    }
}
