using System;
using System.IO;
using System.Linq;
using GeneralComponents;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using GUI.Colors;

namespace GUI.PLT {
    /// <summary>
    /// The class describes the plt code decoder
    /// </summary>
    class PLTDecoder : INotifyPropertyChanged {
        private readonly static uint numTicksInMM = 40; // Characteristic of PLT format

        public event PropertyChangedEventHandler PropertyChanged;

        public uint MaxX { get; private set; } = 0;
        public uint MaxY { get; private set; } = 0;

        public byte curPercent = 0;
        public byte CurPercentOfProcessing {
            get => curPercent;
            private set {
                if (0 <= value && value <= 100) {
                    curPercent = value;
                    NotifyPropertyChanged(nameof(CurPercentOfProcessing));
                }
            }
        }

        private readonly List<Stroke> decodedPlt = new();
        private Point2D? lastPoint = null;
        private PLTColor curColor = null;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// The method decodes the plt code passed in the string
        /// </summary>
        /// <param name="fileName"> Path to the file with plt code </param>
        /// <returns> List of strokes with specified colors </returns>
        public List<Stroke> Decode(in string fileName) {
            NewDecode();

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

            int curPos = 3;
            // Start from 3 to cut [IN] operator
            for (int endPos = pltCode.Length; curPos < endPos; ++curPos) {
                CurPercentOfProcessing = (byte)((curPos+1)*100 / endPos); // +1 to get 100 percent at the last iteration
                int startPosition = curPos;

                curPos = pltCode.IndexOf(';', startPosition);
                ProcessPart(pltCode[startPosition..curPos]);
            }

            CurPercentOfProcessing = 100;
            return decodedPlt;
        }

        private void NewDecode() {
            decodedPlt.Clear();

            lastPoint = null;
            curColor = null;

            MaxX = 0;
            MaxY = 0;
        }

        private void ProcessPart(string part) {
            switch (part[..2]) {
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

        private void ProcessPDCommand(string command) {
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
