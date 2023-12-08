using GeneralComponents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Algorithm.Functions
{
    internal class Converters
    {
        /// <summary>
        /// The method converts the list of hsv-pixels (h - hue, s - Saturation, v - value) 
        /// to the list of rgb-pixels (r - red, g - green, b - blue)
        /// </summary>
        /// <param name="hsvPixels"> List of colors hsv-pixels represented as double[] </param>
        /// <returns> List of colors rgb-pixels represented as double[] </returns>
        public static List<double[]> Hsv2rgb(List<double[]> hsvPixels)
        {
            List<double[]> rgbPixels = new List<double[]>(hsvPixels.Count);
            for (int i = 0; i < hsvPixels.Count; i++)
                rgbPixels.Add(Hsv2rgb(hsvPixels[i]));
            return rgbPixels;
        }

        /// <summary>
        /// The method converts an hsv-pixel (h - hue, s - Saturation, v - value) 
        /// to the rgb-pixel (r - red, g - green, b - blue)
        /// </summary>
        /// <param name="hsvPixel"> Color hsv-pixel represented as double[] </param>
        /// <returns> Color rgb-pixel represented as double[] </returns>
        public static double[] Hsv2rgb(double[] hsvPixel)
        {
            double h = hsvPixel[0];
            double s = hsvPixel[1];
            double v = hsvPixel[2];
            double[] rgbPixel = new double[Constants.ColorLength];
            rgbPixel[0] = rgbPixel[1] = rgbPixel[2] = v * (1 - s);
            // red = hue-2/3; green = hue; blue = hue-1/3
            // Apply modulo 1 for red and blue to keep within range [0, 1]
            double[] hueForRgb = new double[Constants.ColorLength];
            hueForRgb[0] = Operations.Mod(h - 2 / 3d, 1);
            hueForRgb[1] = h;
            hueForRgb[2] = Operations.Mod(h - 1 / 3d, 1);
            double[] factor = new double[Constants.ColorLength];
            factor[0] = factor[1] = factor[2] = s * v;

            static int hueIsLess(double hue, double number)
            {
                if (hue < number)
                    return 1;
                return 0;
            }

            static int hueIsNotLess(double hue, double number)
            {
                if (hue < number)
                    return 0;
                return 1;
            }

            for (int i = 0; i < Constants.ColorLength; i++)
            {

                rgbPixel[i] += factor[i] * (6d * hueIsLess(hueForRgb[i], 1 / 6d) * hueForRgb[i]
                    + (hueIsNotLess(hueForRgb[i], 1 / 6d) & hueIsLess(hueForRgb[i], 1 / 2d))
                    + (hueIsNotLess(hueForRgb[i], 1 / 2d) & hueIsLess(hueForRgb[i], 2 / 3d)) * (4d - 6d * hueForRgb[i]));
            }
            return rgbPixel;
        }

        /// <summary>
        /// The method converts an rgb-pixel (r - red, g - green, b - blue)
        /// to the hsv-pixel (h - hue, s - saturation, v - value) 
        /// </summary>
        /// <param name="pixel"> Color rgb-pixel represented as double[] </param>
        /// <returns> Color hsv-pixel represented as double[] </returns>
        public static double[] Rgb2hsv(double[] pixel)
        {
            double r = pixel[0];
            double g = pixel[1];
            double b = pixel[2];
            double h = 0;
            double s = Math.Min(r, Math.Min(g, b));    //Min value of RGB
            double v = Math.Max(r, Math.Max(g, b));    //Max value of RGB
            double del = v - s;                        //Delta RGB value

            if (del == 0) // gray has no hue
                s = 0;
            else // not gray
            {
                // count blue hue
                if (v == b)
                {
                    h = (2 / 3d) + (1 / 6d) * (r - g) / (v - s);
                }
                // count green hue
                if (v == g)
                {
                    h = (1 / 3d) + (1 / 6d) * (b - r) / (v - s);
                }
                // count red hue
                if (v == r)
                {
                    h = (1 / 6d) * (g - b) / (v - s);
                }
            }
            if (h < 0)
                h++;
            if (del != 0)
                s = 1 - s / v;
            return new double[] { h, s, v };
        }

        /// <summary>
        /// The method converts Proportions and MixType of the Color to CMYBW-Color 
        /// </summary>
        /// <param name="proportions"> Color Proportions represented as double[] </param>
        /// <param name="mixType"> Color mixture type represented as double[] </param>
        /// <returns> CMYBW-Color represented as double[] (8 paints) </returns>
        public static double[] Proportions2PP(double[] proportions, ColorMixType mixType) // #prop2pp
        {
            int vTotal = 3000;
            double C = 0, M = 0, Y = 0; // initiate colors
            double a = proportions[0];
            double b = proportions[1];
            double c = proportions[2];
            double C1 = Math.Floor(a * c * vTotal);
            double C2 = Math.Floor((1 - a) * c * vTotal);
            double B = Math.Floor((1 - c) * b * vTotal);
            double W = Math.Floor((1 - b) * (1 - c) * vTotal);
            switch (mixType)
            {
                case ColorMixType.MagentaYellow1: // fall through
                case ColorMixType.MagentaYellow2:
                    M = C1;
                    Y = C2;
                    break;
                case ColorMixType.YellowCyan:
                    Y = C1;
                    C = C2;
                    break;
                case ColorMixType.CyanMagenta:
                    C = C1;
                    M = C2;
                    break;
            }
            return new double[] { C, M, Y, B, W, 0, 0, 0 };
        }

        /// <summary>
        /// The method converts a list of proportions and mixture types to the list of
        /// hsv-pixels using color database parts Ycell and Wcell
        /// </summary>
        /// <param name="proportions"> List of color proportions represented as double[] </param>
        /// <param name="mixTypes"> List of color mixture types represented as double[] </param>
        /// <param name="Ycell"> List of hsv-pixels represented as double[] </param>
        /// <param name="Wcell"> List of proportions represented as double[] </param>
        /// <param name="sheetsAmount"> Amount of sheets in database </param>
        /// <returns> List of colors hsv-pixels represented as double[] </returns>
        internal static List<double[]> Proportions2hsv(List<double[]> proportions, List<ColorMixType> mixTypes, List<List<List<double>>> Ycell, List<List<List<double>>> Wcell, int sheetsAmount = 4)
        {
            int K = 14;

            int Npts = proportions.Count;
            List<double[]> hsvcol = new List<double[]>(Npts);

            for (int i = 0; i < Npts; i++)
            {
                List<List<double>> Y = new List<List<double>>();
                List<List<double>> W = new List<List<double>>();
                // take the corresponding sets
                if ((int)(mixTypes[i]) < 2)
                {
                    Y.AddRange(Ycell[0]);
                    Y.AddRange(Ycell[1]);
                    for (int j = 0; j < Wcell[0].Count; j++)
                    {
                        W.Add(new List<double>());
                        W[j].AddRange(Wcell[0][j]);
                        W[j][0] = (Wcell[0][j][0] - 1); // for mixtype MY with H > 0.4 make H negative
                    }
                    W.AddRange(Wcell[1]);
                }
                else
                {
                    Y.AddRange(Ycell[(int)mixTypes[i]]); // Y for Proportions
                    W.AddRange(Wcell[(int)mixTypes[i]]); // W for HSV
                }

                int NY = Y.Count;
                Matrix2D dst = new Matrix2D(NY, 1);
                for (int k = 0; k < NY; k++)
                    dst[k] = Functions.ArraysManipulations.Distance(proportions[i], Y[k].ToArray()); // ##ok

                int[] indexes = dst.GetIndexesForSorted();

                double[] d = new double[K];

                for (int c = 0; c < K; c++)
                    d[c] = 1d / (dst[indexes[c]] + 1e-4);

                Matrix2D D = (new Matrix2D(d.ToList())).MakeDiag();

                double[] hsvcolcur = { 0, 0, 0 };

                int N = 4;

                List<List<double>> T = new List<List<double>> {
                            new List<double> { 0, 0, 0 },
                            new List<double> { 1, 0, 0 },
                            new List<double> { 0, 1, 0 },
                            new List<double> { 0, 0, 1 }
                        }; // deglexord(0,1,3);
                           // take first K Points
                K = Math.Min(NY, K); // decrease K if needed

                Matrix2D X = new Matrix2D(K, Y[0].Count);

                for (int p = 0; p < K; p++)
                    for (int q = 0; q < Y[p].Count; q++)
                        X[p, q] = Y[indexes[p]][q];

                Matrix2D E = new Matrix2D(K, N, 0); // evaluated polynomial
                Matrix2D h = Matrix2D.Eye(N);
                for (int k = 0; k < N; k++)
                {
                    Matrix2D product = Matrix2D.MulByRows(X.Pow(new Matrix2D(T[k])).RepeatRows(K));
                    for (int r = 0; r < K; r++)
                    {
                        for (int c = 0; c < E.Columns; c++)
                        {
                            E[r, c] += product[r] * h[k, c];
                        }
                    }
                }

                for (int j = 0; j < Constants.ColorLength; j++)
                {
                    Matrix2D Wj = new Matrix2D(K, 1);

                    for (int k = 0; k < K; k++)
                        Wj[k] = W[indexes[k]][j];

                    // h = (E'*E)\(E'*V); // OLS
                    Matrix2D Etransposed = E.Transpose();

                    Matrix2D h2 = GausMethod.Solve(
                        ((Etransposed * D * E) + (Matrix2D.Eye(N) * 1e-4)), 
                        (Etransposed * D * Wj));

                    // predict proportion
                    hsvcolcur[j] = 0;
                    Vector x = new Vector(proportions[i]);

                    for (int k = 0; k < N; k++)
                    {
                        double product = (x.Pow(new Vector(T[k]))).Product();

                        for (int c = 0; c < h2.Columns; c++)
                            hsvcolcur[j] += product * h2[k, c];
                    }
                }

                if (hsvcolcur[0] < 0)
                    hsvcolcur[0]++; // use this because models predict with shift
                                    // get into ranges [0,1]
                hsvcol.Add(Functions.ArraysManipulations.Saturation(hsvcolcur));
            }
            return hsvcol;
        }

        //public static Matrix3D strokesToImage(int brushSize, Matrix3D canvas, List<Stroke> map, byte CanvasColor)
        //{
        //    int m = canvas[0].Rows;
        //    int n = canvas[0].Columns;
        //    Matrix3D img2 = new Matrix3D(m, n, Constants.ColorLength, CanvasColor);
        //    double BsQuad = brushSize * brushSize / 4d;
        //    for (int i = 0; i < map.Count; i++) // go through strokes
        //    {
        //        Stroke stroke = map[i];
        //        double[] Color = stroke.Color;
        //        List<Point> Points = stroke.Points;
        //        double pX = Points[0].X; // start of the stroke
        //        double pY = Points[0].Y;
        //        for (int j = 1; j < Points.Count; j++) // go through dots of the stroke
        //        {
        //            Point candidate = Points[j];
        //            double N = Math.Max(Math.Abs(pX - candidate.X), Math.Abs(pY - candidate.Y));
        //            for (double t = 0; t <= 1; t += (1d / N))
        //            {
        //                double xo = Math.Round(candidate.X + (pX - candidate.X) * t);
        //                double yo = Math.Round(candidate.Y + (pY - candidate.Y) * t);
        //                for (double Xl = Math.Round(xo - brushSize / 2); Xl <= Math.Round(xo + brushSize / 2); Xl++)
        //                {
        //                    for (double Yl = Math.Round(yo - brushSize / 2); Yl <= Math.Round(yo + brushSize / 2); Yl++)
        //                    {
        //                        if (Xl >= 0 && Xl < m && Yl >= 0 && Yl < n)
        //                        {
        //                            if ((Xl - xo) * (Xl - xo) + (Yl - yo) * (Yl - yo) < BsQuad)
        //                            {
        //                                img2[(int)Xl, (int)Yl, 0] = Color[0];
        //                                img2[(int)Xl, (int)Yl, 1] = Color[1];
        //                                img2[(int)Xl, (int)Yl, 2] = Color[2];
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            pX = candidate.X;
        //            pY = candidate.Y;
        //        }
        //    }
        //    return img2;
        //}

    }
}
