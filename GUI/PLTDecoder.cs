using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace GUI
{
    /// <summary>
    /// The class describes the plt code decoder
    /// </summary>
    class PLTDecoder
    {
        private List<Stroke> decodedPlt = new();
        private Vector<uint>? lastPoint = null;
        private Color curColor = null;

        /// <summary>
        /// The method decodes the plt code passed in the string
        /// </summary>
        /// <param name="pltCode"> A string containing the plt code </param>
        /// <returns> List of strokes with specified colors </returns>
        public List<Stroke> Decode(string pltCode)
        {
            if (pltCode.Substring(0, 2).ToLower() != "in")
                throw new ArgumentException("ERROR! Invalid code. PLT-code should start with [IN] operator!");

            pltCode = pltCode.Remove(0, 3);
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
        }

        private void ProcessPart(string part)
        {
            switch (part.Substring(0, 2))
            {
                case "PP":
                    curColor = new(part[3..].Split(','));
                    break;
                case "PD":
                    if (curColor == null)
                        throw new ArgumentException("Invalid plt code. No color is set before painting!");

                    Vector<uint> newPoint = new(part[3..].Split(',').Select(elem => uint.Parse(elem)).ToArray());
                    if (lastPoint != null)
                        decodedPlt.Add(new(lastPoint.Value, newPoint, curColor));

                    lastPoint = newPoint;
                    break;
                case "PU":
                    lastPoint = null;
                    break;
                default:
                    throw new ArgumentException($"ERROR! Unknown [{part.Substring(0, 2)}] operator!");
            }
        }
    }
}
