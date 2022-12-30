using System;
using System.Linq;
using System.Windows.Media;
using System.Collections.Generic;

using GeneralComponents;

namespace GUI.Colors {
    /// <summary>
    /// The class describes a color based on cyan, magenta, yellow, blue and white.
    /// </summary>
    internal class CMYBWColor : PLTColor {
        /// <summary> Number of neibors for HSV-color regression </summary>
        public static uint NumOfNeibForRegression { get; set; } = 20;

        private readonly static uint minColors = 5; // White/black pictures
        private readonly static uint maxColors = 8; // Color pictures
        private readonly uint cyan, magenta, yellow, blue, white;

        private (uint, uint, uint, uint, uint) GetColors() {
            return (cyan, magenta, yellow, blue, white);
        }

        public CMYBWColor(string[] colors) {
            (cyan, magenta, yellow, blue, white) = colors.Select(elem => uint.Parse(elem)).ToArray() switch {
                var arr when minColors == arr.Length || arr.Length == maxColors =>
                                                                    (arr[0], arr[1], arr[2], arr[3], arr[4]),
                _ => throw new ArgumentException("There isn't correct number of the colors!")
            };
        }

        public static bool operator ==(CMYBWColor first, CMYBWColor second) {
            return first.GetColors() == second.GetColors();
        }

        public static bool operator !=(CMYBWColor first, CMYBWColor second) {
            return !(first == second);
        }

        public override bool Equals(object obj) {
            return obj is CMYBWColor color && this == color;
        }

        public override int GetHashCode() {
            return HashCode.Combine(cyan, magenta, yellow, blue, white);
        }

        public override Color ToColor() {
            return ((HSVColor)this).ToColor();
        }

        public static explicit operator HSVColor(CMYBWColor color) {
            double a = 0, b = 0, c = 0;

            ColorMixType mixType = ColorMixType.MagentaYellow1;
            if (color.cyan + color.magenta + color.yellow > 0) {
                if (color.magenta > 0 && (color.yellow > 0 || color.cyan == 0)) {
                    a = (double)color.magenta / (color.magenta + color.yellow);
                    mixType = a > 0.4 ? ColorMixType.MagentaYellow1 : ColorMixType.MagentaYellow2;
                } else {
                    if (color.yellow > 0 && (color.cyan > 0 || color.magenta == 0)) {
                        a = (double)color.yellow / (color.cyan + color.yellow);
                        mixType = ColorMixType.YellowCyan;
                    } else {
                        a = (double)color.cyan / (color.cyan + color.magenta);
                        mixType = ColorMixType.CyanMagenta;
                    }
                }
            }

            if (color.blue + color.white > 0)
                b = (double)color.blue / (color.blue + color.white);
            else
                b = 0;

            uint hue = color.cyan + color.magenta + color.yellow;
            uint vTotal = hue + color.blue + color.white;
            c = (double)hue / vTotal;

            var hsv = DatabaseLoader.Database.GetHSV(mixType);
            var proportions = DatabaseLoader.Database.GetProportions(mixType);

            int numRows = hsv.Count;
            Matrix2D distances = new(numRows, 1);
            Matrix2D props = new(new List<double> { a, b, c });

            for (int i = 0; i < numRows; ++i) {
                Matrix2D matrix = new(proportions[i]);
                // squared Euclidean distance
                distances[i] = (double)((props - matrix) * (props - matrix).Transpose());
            }

            int[] indexes = distances.GetIndexesForSorted();
            int numOfColors = (int)Math.Min(NumOfNeibForRegression, numRows);

            Matrix2D D = (1 / distances.GetByIndexes(indexes[..numOfColors])).MakeDiag();
            Matrix2D nearestPoints = new(hsv.GetByIndexes(indexes[..numOfColors]));
            Matrix2D propsOfNearestPoints = new(proportions.GetByIndexes(indexes[..numOfColors]));

            Matrix2D E = new(numOfColors, 4, 0); // evaluated polynomial
            Matrix2D T = new(4, 3); // monomial orders
            T.MakeUnit(1);

            Matrix2D h = new(4, 4);
            h.MakeUnit(0);

            for (int i = 0; i < 4; ++i) {
                Vector fir = h.GetRow(i);
                Matrix2D sec = Matrix2D.MulByRows(propsOfNearestPoints.Pow(T.GetRow(i)));

                int columnRepeat = fir.Size;
                int rowsRepeat = sec.Rows;
                Matrix2D matrix = new(fir);
                matrix = matrix.RepeatRows(rowsRepeat);
                sec = sec.RepeatColumns(columnRepeat);

                Matrix2D mul = matrix ^ sec;
                E = E + mul;
            }

            double[] hsvColor = new double[3];
            Matrix2D hsvOfNearestPoints = new(hsv.GetByIndexes(indexes[..numOfColors]));

            for (int i = 0; i < hsvColor.Length; ++i) {
                //Matrix coefs = E.Transpose() * E;
                //Matrix answers = E.Transpose() * nearestPoints.GetColumn(i);
                //h = GausMethod.Solve(coefs, answers); // OLS

                Vector V = hsvOfNearestPoints.GetColumn(i);

                Matrix2D coefs = E.Transpose() * D * E;
                Matrix2D answers = E.Transpose() * D * V;
                h = GausMethod.Solve(coefs, answers);

                // Predict proportion
                double p = 0;
                Matrix2D X = props;

                for (int k = 0; k < 4; ++k) {
                    Vector fir = h.GetRow(k);
                    Matrix2D sec = Matrix2D.MulByRows(X.Pow(T.GetRow(k)));

                    Matrix2D matrix = new(fir); // Need refactoring
                    Matrix2D mul = matrix ^ sec;
                    p += (double)mul;
                }

                hsvColor[i] = p;
            }

            if (hsvColor[0] < 0)
                hsvColor[0] += 1; // Use this because models predict with shift

            for (int i = 0; i < hsvColor.Length; ++i)
                hsvColor[i] = Math.Min(Math.Max(hsvColor[i], 0), 1); // Get into ranges[0, 1]

            HSVColor col = new(hsvColor);
            return col;
        }
    }
}
