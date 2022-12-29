using System;
using System.IO;
using System.Linq;
using GeneralComponents;
using System.Collections.Generic;

using GUI.Colors;

namespace GUI.PLT {
    /// <summary>
    /// The class describes the plt code decoder
    /// </summary>
    class PLTDecoder : NotifierOfPropertyChange {
        private readonly static uint numTicksInMM = 40; // Characteristic of PLT format
        public static int MaxPercent => 100;

        private int curPercent = 0;
        public int CurPercent {
            get => curPercent;
            private set {
                if (0 <= value && value <= MaxPercent) {
                    curPercent = value;
                    NotifyPropertyChanged(nameof(CurPercent));
                }
            }
        }

        private Point2D? lastPoint = null;
        private PLTColor curColor = null;

        /// <summary>
        /// The method decodes the plt code passed in the string
        /// </summary>
        /// <param name="fileName"> The path to plt file that should decode </param>
        /// <returns> List of strokes with specified colors </returns>
        public PLTDecoderRes Decode(in string fileName) {
            string pltCode;
            try {
                pltCode = File.ReadAllText(fileName);
            } catch (Exception) {
                throw new Exception("ERROR! Can't read the file!");
            }

            pltCode = pltCode.Trim();
            if (!pltCode.StartsWith("in;", true, null))
                throw new Exception("ERROR! PLT-code should start with [IN] operator!");
            if (pltCode[^1] != ';')
                throw new Exception("ERROR! PLT file have to end with ';' character!");

            List<Stroke> decodedPlt = new();
            lastPoint = null; curColor = null;
            int curPos = 3; // Start from 3 to cut [IN] operator
            for (int endPos = pltCode.Length; curPos < endPos; ++curPos) {
                CurPercent = curPos*MaxPercent / endPos;
                int startPosition = curPos;

                curPos = pltCode.IndexOf(';', startPosition);
                ProcessPart(decodedPlt, pltCode[startPosition..curPos]);
            }

            CurPercent = MaxPercent;
            return new(decodedPlt);
        }

        private void ProcessPart(List<Stroke> decodedPlt, in string part) {
            switch (part[..2]) {
                case "PP":
                    curColor = new CMYBWColor(part[2..].Split(','));
                    break;
                case "PC":
                    curColor = new RGBColor(part[2..].Split(','));
                    break;
                case "PD":
                    ProcessPDCommand(decodedPlt, part);
                    break;
                case "PU":
                    lastPoint = null;
                    break;
                default:
                    throw new ArgumentException($"ERROR! Unknown [{part[..2]}] operator!");
            }
        }

        private void ProcessPDCommand(List<Stroke> decodedPlt, in string command) {
            if (curColor == null)
                throw new ArgumentException("Invalid plt code. Color don't set before painting!");

            uint[] coords = command[2..].Split(',').Select(elem => uint.Parse(elem)).ToArray();
            Point2D newPoint = new(coords);
            newPoint.Divide(numTicksInMM);

            if (lastPoint != null)
                decodedPlt.Add(new(lastPoint.Value, newPoint, curColor));

            lastPoint = newPoint;
        }
    }
}
