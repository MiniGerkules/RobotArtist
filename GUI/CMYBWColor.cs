using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media;

namespace GUI {
    /// <summary>
    /// The class describes a color based on cyan, magenta, yellow, blue and white.
    /// </summary>
    internal class CMYBWColor : PLTColor {
        private static uint minColors = 5; // White/black pictures
        private static uint maxColors = 8; // Color pictures
        private uint cyan, magenta, yellow, blue, white;

        private (uint, uint, uint, uint, uint) GetColors() {
            return (cyan, magenta, yellow, blue, white);
        }

        public CMYBWColor(string[] colors) {
            (cyan, magenta, yellow, blue, white) = colors.Select(elem => uint.Parse(elem)).ToArray() switch {
                var arr when (minColors == arr.Length || arr.Length == maxColors) =>
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
                if ((color.magenta > 0) && (color.yellow > 0 || color.cyan == 0)) {
                    a = (double)color.magenta / (color.magenta + color.yellow);
                    mixType = a > 0.4 ? ColorMixType.MagentaYellow1 : ColorMixType.MagentaYellow2;
                } else {
                    if ((color.yellow > 0) && (color.cyan > 0 || color.magenta == 0)) {
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

            List<List<double>> hsv;
            List<List<double>> proportions;
            if (mixType == ColorMixType.MagentaYellow1 || mixType == ColorMixType.MagentaYellow2) {
                hsv = new(Database.Data[0].Count + Database.Data[1].Count);
                hsv.AddRange(Database.Data[0].Select(elem => elem.GetRange(0, 3)));
                hsv.ForEach(elem => elem[0] -= 1);
                hsv.AddRange(Database.Data[1].Select(elem => elem.GetRange(0, 3)));

                proportions = new(Database.Data[0].Count + Database.Data[1].Count);
                proportions.AddRange(Database.Data[0].Select(elem => elem.GetRange(3, 3)));
                proportions.AddRange(Database.Data[1].Select(elem => elem.GetRange(3, 3)));
            } else {
                hsv = new(Database.Data[(int)mixType].Count);
                hsv.AddRange(Database.Data[(int)mixType].Select(elem => elem.GetRange(0, 3)));

                proportions = new(Database.Data[(int)mixType].Count);
                proportions.AddRange(Database.Data[(int)mixType].Select(elem => elem.GetRange(3, 3)));
            }

            int numRows = hsv.Count;
            Matrix distances = new(numRows, 1);
            Matrix props = new(new List<double> { a, b, c });

            for (int i = 0; i < numRows; ++i) {
                Matrix matrix = new(proportions[i]);
                // squared Euclidean distance
                distances[i] = (double)((props - matrix) * (props - matrix).Transpose());
            }

            int[] indexes = distances.GetIndexesForSorted();
            int numOfColors = Math.Min(250, numRows);

            Matrix D = (1 / distances.GetByIndexes(indexes[..numOfColors])).MakeDiag();
            Matrix nearestPoints = new(Helpers.GetByIndexes(hsv, indexes[..numOfColors]));
            Matrix propsOfNearestPoints = new(Helpers.GetByIndexes(proportions, indexes[..numOfColors]));

            Matrix E = new(numOfColors, 4, 0); // evaluated polynomial
            Matrix T = new(4, 3); // monomial orders
            T.MakeUnit(1);

            Matrix h = new(4, 4);
            h.MakeUnit(0);

            for (int i = 0; i < 4; ++i) {
                Matrix fir = h.GetRow(i);
                Matrix sec = Matrix.MulByRows(propsOfNearestPoints.Pow(T.GetRow(i)));

                int columnRepeat = fir.Columns;
                int rowsRepeat = sec.Rows;
                fir = fir.RepeatRows(rowsRepeat);
                sec = sec.RepeatColumns(columnRepeat);

                Matrix mul = fir ^ sec;
                E = E + mul;
            }

            double[] hsvColor = new double[3];
            Matrix hsvOfNearestPoints = new(Helpers.GetByIndexes(hsv, indexes[..numOfColors]));

            for (int i = 0; i < hsvColor.Length; ++i) {
                //Matrix coefs = E.Transpose() * E;
                //Matrix answers = E.Transpose() * nearestPoints.GetColumn(i);
                //h = GausMethod.Solve(coefs, answers); // OLS

                Matrix V = hsvOfNearestPoints.GetColumn(i);

                Matrix coefs = E.Transpose() * D * E;
                Matrix answers = E.Transpose() * D * V;
                h = GausMethod.Solve(coefs, answers);

                // Predict proportion
                double p = 0;
                Matrix X = props;

                for (int k = 0; k < 4; ++k) {
                    Matrix fir = h.GetRow(k);
                    Matrix sec = Matrix.MulByRows(X.Pow(T.GetRow(k)));

                    Matrix mul = fir ^ sec;
                    p += (double)(mul);
                }

                hsvColor[i] = p;
            }

            if (hsvColor[0] < 0)
                hsvColor[0] += 1; // Use this because models predict with shift

            for (int i = 0; i < hsvColor.Length; ++i)
                hsvColor[i] = Math.Min(Math.Max(hsvColor[i], 0), 1); // Get into ranges[0, 1]

            HSVColor col = new HSVColor(hsvColor);
            return col;
        }
    }
}
