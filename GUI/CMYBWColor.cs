using System;
using System.Linq;
using System.Windows.Media;
using System.Collections.Generic;

namespace GUI
{
    /// <summary>
    /// The class describes a color based on cyan, magenta, yellow, blue and white.
    /// </summary>
    internal struct CMYBWColor
    {
        private uint cyan, magenta, yellow, blue, white;
        private static uint numOfColors = 8;

        private (uint, uint, uint, uint, uint) GetColors()
        {
            return (cyan, magenta, yellow, blue, white);
        }

        public CMYBWColor() : this((0, 0, 0, 0, 0))
        {
        }

        public CMYBWColor((uint cyan, uint magenta, uint yellow, uint blue, uint white) colors)
        {
            (cyan, magenta, yellow, blue, white) = colors;
        }

        public CMYBWColor(string[] colors)
        {
            (cyan, magenta, yellow, blue, white) = colors.Select(elem => uint.Parse(elem)).ToArray() switch {
                var arr when arr.Length == numOfColors => (arr[0], arr[1], arr[2], arr[3], arr[4]),
                _ => throw new ArgumentException($"The number of colors is not equal to {numOfColors}!")
            };
        }

        public static bool operator ==(CMYBWColor first, CMYBWColor second)
        {
            return first.GetColors() == second.GetColors();
        }

        public static bool operator !=(CMYBWColor first, CMYBWColor second)
        {
            return !(first == second);
        }

        public override bool Equals(object obj)
        {
            return obj is CMYBWColor color && this == color;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(cyan, magenta, yellow, blue, white);
        }

        public static explicit operator Brush(CMYBWColor color)
        {
            double a = 0, b = 0, c = 0;

            ColorMixType mixType = ColorMixType.MagentaYellow;
            if (color.cyan + color.magenta + color.yellow > 0)
            {
                if ((color.magenta > 0) && (color.yellow > 0 || color.cyan == 0))
                {
                    mixType = ColorMixType.MagentaYellow;
                    a = (double)color.magenta / (color.magenta + color.yellow);
                }
                else
                {
                    if ((color.yellow > 0) && (color.cyan > 0 || color.magenta == 0))
                    {
                        mixType = ColorMixType.YellowCyan;
                        a = (double)color.yellow / (color.cyan + color.yellow);
                    }
                    else
                    {
                        mixType = ColorMixType.CyanMagenta;
                        a = (double)color.cyan / (color.cyan + color.magenta);
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
            if (mixType == ColorMixType.MagentaYellow)
            {
                hsv = new(Database.Data[0].Count + Database.Data[1].Count);
                hsv.AddRange(Database.Data[0].Select(elem => elem.GetRange(0, 3)));
                hsv.ForEach(elem => elem[0] -= 1);
                hsv.AddRange(Database.Data[1].Select(elem => elem.GetRange(0, 3)));

                proportions = new(Database.Data[0].Count + Database.Data[1].Count);
                proportions.AddRange(Database.Data[0].Select(elem => elem.GetRange(3, 3)));
                proportions.AddRange(Database.Data[1].Select(elem => elem.GetRange(3, 3)));
            }
            else
            {
                int sheet = (int)mixType + 1;

                hsv = new(Database.Data[sheet].Count);
                hsv.AddRange(Database.Data[sheet].Select(elem => elem.GetRange(0, 3)));

                proportions = new(Database.Data[sheet].Count);
                proportions.AddRange(Database.Data[sheet].Select(elem => elem.GetRange(3, 3)));
            }

            int numRows = hsv.Count;
            Matrix distances = new(numRows, 1);
            Matrix props = new(new List<double> { a, b, c });

            for (int i = 0; i < numRows; ++i) {
                Matrix matrix = new(proportions[i]);
                // Euclidean distance
                Matrix fir = (props - matrix);
                Matrix sec = (props - matrix).Transpose();
                Matrix mul = fir * sec;

                double distInSquare = (double)(mul);
                distances[i] = Math.Sqrt(distInSquare);
            }

            int[] indexes = distances.GetIndexesForSorted();
            int numOfColors = 50;

            Matrix nearestPoints = new(Helpers.GetByIndexes(hsv, indexes[..numOfColors]));
            Matrix propsOfNearestPoints = new(Helpers.GetByIndexes(proportions, indexes[..numOfColors]));

            double[] hsvColor = new double[3];
            for (int i = 0; i < hsvColor.Length; ++i)
            {
                Matrix E = new(numOfColors, 4, 0); // evaluated polynomial
                Matrix T = new(4, 3); // monomial orders
                T.MakeUnit(1);
                
                Matrix h = new(4, 4);
                h.MakeUnit(0);

                for (int k = 0; k < 4; ++k)
                {
                    Matrix fir = h.GetRow(k);
                    Matrix sec = Matrix.MulByRows(propsOfNearestPoints.Pow(T.GetRow(k)));
                    
                    int columnRepeat = fir.Columns;
                    int rowsRepeat = sec.Rows;
                    fir = fir.RepeatRows(rowsRepeat);
                    sec = sec.RepeatColumns(columnRepeat);

                    Matrix mul = fir ^ sec;
                    E = E + mul;
                }

                Matrix coefs = E.Transpose() * E;
                Matrix answers = E.Transpose() * nearestPoints.GetColumn(i);
                // firstPart * X = secondPart
                h = GausMethod.Solve(coefs, answers);

                // Predict proportion
                double p = 0;
                Matrix X = props;

                for (int k = 0; k < 4; ++k)
                {
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

            return new SolidColorBrush(GetRGBFromHSV(hsvColor[0], hsvColor[1], hsvColor[2]));
        }

        private static Color GetRGBFromHSV(double hue, double saturation, double value)
        {
            // https://en.wikipedia.org/wiki/HSL_and_HSV#HSV_to_RGB
            double hueInDegrees = hue * 360;
            double C = saturation * value; // цветность
            double H = hueInDegrees / 60;
            double X = C * (1 - Math.Abs(H % 2 - 1));

            double r1, g1, b1;
            if (0 <= H && H < 1)
                (r1, g1, b1) = (C, X, 0);
            else if (1 <= H && H < 2)
                (r1, g1, b1) = (X, C, 0);
            else if (2 <= H && H < 3)
                (r1, g1, b1) = (0, C, X);
            else if (3 <= H && H < 4)
                (r1, g1, b1) = (0, X, C);
            else if (4 <= H && H < 5)
                (r1, g1, b1) = (X, 0, C);
            else /* 5 <= H && H < 6 */
                (r1, g1, b1) = (C, 0, X);

            double m = value - C;
            byte red = (byte)((r1 + m) * 255);
            byte green = (byte)((g1 + m) * 255);
            byte blue = (byte)((b1 + m) * 255);

            return Color.FromRgb(red, green, blue);
        }
    }
}
