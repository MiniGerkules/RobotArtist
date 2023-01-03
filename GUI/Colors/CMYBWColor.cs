using System;
using System.Linq;
using System.Windows.Media;
using System.Collections.Generic;

using GeneralComponents;

namespace GUI.Colors {
    /// <summary>
    /// The class describes a color based on cyan, magenta, yellow, blue and white.
    /// </summary>
    internal class CMYBWColor : IColor {
        /// <summary> Number of neibors for HSV-color regression </summary>
        public static uint NumOfNeibForRegression { get; set; } = 20;

        private readonly static uint minColors = 5; // White/black pictures
        private readonly static uint maxColors = 8; // Color pictures
        private readonly uint cyan, magenta, yellow, blue, white;

        private readonly ColorMixType mixType;
        private readonly Matrix2D props;

        private (uint, uint, uint, uint, uint) GetColors() {
            return (cyan, magenta, yellow, blue, white);
        }

        public CMYBWColor(string[] colors) {
            (cyan, magenta, yellow, blue, white) = colors.Select(elem => uint.Parse(elem)).ToArray() switch {
                var arr when minColors == arr.Length || arr.Length == maxColors =>
                                                                    (arr[0], arr[1], arr[2], arr[3], arr[4]),
                _ => throw new ArgumentException("There isn't correct number of the colors!")
            };

            (mixType, props) = DetermineProportions();
        }

        private (ColorMixType, Matrix2D) DetermineProportions() {
            double a = 0;
            ColorMixType mixType = ColorMixType.MagentaYellow1;

            if (cyan + magenta + yellow > 0) {
                if (magenta > 0 && (yellow > 0 || cyan == 0)) {
                    a = (double)magenta / (magenta+yellow);
                    mixType = a > 0.4 ? ColorMixType.MagentaYellow1 : ColorMixType.MagentaYellow2;
                } else {
                    if (yellow > 0 && (cyan > 0 || magenta == 0)) {
                        a = (double)yellow / (cyan+yellow);
                        mixType = ColorMixType.YellowCyan;
                    } else {
                        a = (double)cyan / (cyan+magenta);
                        mixType = ColorMixType.CyanMagenta;
                    }
                }
            }

            double b = blue + white > 0 ? (double)blue / (blue+white) : 0;

            uint hue = cyan + magenta + yellow;
            uint vTotal = hue + blue + white;
            double c = (double)hue / vTotal;

            return (mixType, new(new List<double> { a, b, c }));
        }

        public static bool operator ==(CMYBWColor first, CMYBWColor second) {
            return first.GetColors() == second.GetColors();
        }

        public static bool operator !=(CMYBWColor first, CMYBWColor second) {
            return !(first == second);
        }

        public override bool Equals(object? obj) {
            return obj != null && obj is CMYBWColor color && this == color;
        }

        public override int GetHashCode() {
            return HashCode.Combine(cyan, magenta, yellow, blue, white);
        }

        public Color GetRealColor() {
            return ToHSVColor().GetRealColor();
        }

        public Color GetArtificialColor() {
            var rgb = new double[3];
            Random rand = new();

            switch (mixType) {
                case ColorMixType.MagentaYellow1:
                case ColorMixType.MagentaYellow2:
                    rgb[0] = 0.5 + 0.5*rand.NextDouble();
                    rgb[1] = 0.5 + 0.5*rand.NextDouble();
                    rgb[2] = 0.5 * rand.NextDouble();
                    break;
                case ColorMixType.YellowCyan:
                    rgb[0] = 0.5 * rand.NextDouble();
                    rgb[1] = 0.5 + 0.5*rand.NextDouble();
                    rgb[2] = 0.5 * rand.NextDouble();
                    break;
                case ColorMixType.CyanMagenta:
                    rgb[0] = 0.5 + 0.5*rand.NextDouble();
                    rgb[1] = 0.5 * rand.NextDouble();
                    rgb[2] = 0.5 + 0.5*rand.NextDouble();
                    break;
            }

            for (int i = 0; i < rgb.Length; ++i) rgb[i] *= 230;
            return Color.FromRgb((byte)rgb[0], (byte)rgb[1], (byte)rgb[2]);
        }

        public HSVColor ToHSVColor() {
            var hsv = DatabaseLoader.Database.GetHSV(mixType);
            var proportions = DatabaseLoader.Database.GetProportions(mixType);

            Matrix2D distances = new(hsv.Count, 1);
            for (int i = 0; i < hsv.Count; ++i) {
                Matrix2D matrix = new(proportions[i]);
                // squared Euclidean distance
                distances[i] = (double)((props - matrix) * (props - matrix).Transpose());
            }

            int[] indexes = distances.GetIndexesForSorted();
            int numOfColors = (int)Math.Min(NumOfNeibForRegression, hsv.Count);

            Matrix2D D = (1 / distances.GetByIndexes(indexes[..numOfColors])).MakeDiag();
            //Matrix2D nearestPoints = new(hsv.GetByIndexes(indexes[..numOfColors]));
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
