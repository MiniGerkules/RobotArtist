using GeneralComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using Point = System.Windows.Point;

namespace Algorithm.Functions
{
    internal class PiecesProcessing
    {
        /// <summary>
        /// Draws a stroke on the canvas and other related matricies
        /// </summary>
        /// <returns> Returns renewed error matrix </returns>
        public static Matrix2D DrawPiece(Point startPoint, StrokeCandidate candidate, double bs2, double bsQuad,
    ref Matrix3D canvas, ref Matrix3D canvas2, ref Matrix2D ColorClass, ref Matrix2D VolumeOfWhite, double[] meanColorPixel, double[] col2,
    Matrix2D imggray, int mSize, int nSize, ColorMixType mixTypes, double volumeOfWhite)
        {
            double N = Math.Max(Math.Abs(startPoint.X - candidate.X), Math.Abs(startPoint.Y - candidate.Y));

            for (double t = 0; t <= 1; t += (1 / N))
            {
                double xo = Math.Round(candidate.X + (startPoint.X - candidate.X) * t);
                double yo = Math.Round(candidate.Y + (startPoint.Y - candidate.Y) * t);
                for (double Xl = Math.Round(xo - bs2); Xl <= Math.Round(xo + bs2); Xl++)
                {
                    for (double Yl = Math.Round(yo - bs2); Yl <= Math.Round(yo + bs2); Yl++)
                    {
                        if (Xl >= 0 && Xl < mSize && Yl >= 0 && Yl < nSize)
                        {
                            if ((Xl - xo) * (Xl - xo) + (Yl - yo) * (Yl - yo) < bsQuad)
                            {
                                // test for overlap: number of mixtype is >=, and
                                // amount of white is lower
                                if (((int)ColorClass[(int)Xl, (int)Yl] == (int)mixTypes) && (VolumeOfWhite[(int)Xl, (int)Yl] < volumeOfWhite)
                                    || ((int)ColorClass[(int)Xl, (int)Yl] == 0) || ((int)ColorClass[(int)Xl, (int)Yl] < (int)mixTypes))
                                {
                                    // if canvas is free
                                    for (int i = 0; i < meanColorPixel.Length; i++)
                                        canvas[(int)Xl, (int)Yl, i] = meanColorPixel[i];
                                    ColorClass[(int)Xl, (int)Yl] = (int)mixTypes;
                                    VolumeOfWhite[(int)Xl, (int)Yl] = volumeOfWhite;

                                    for (int i = 0; i < col2.Length; i++)
                                        canvas2[(int)Xl, (int)Yl, i] = col2[i]; // new variant
                                }
                            }
                        }
                    }
                }
            }
            return (canvas.mean() - imggray).Abs();
        }

        /// <summary>
        /// Tests a candidate starts from startPoint. If error is acceptable returns true,
        /// false otherwise
        /// </summary>
        /// <returns> Returns true if piece is accepted and false otherwise  </returns>
        public static bool TestNewPieceAccepted(Point startPoint, Matrix3D img, byte canvasColor,         // #testNewPiece
            double canvasEps, Matrix3D canvas2, Matrix2D ColorClass, Matrix2D VolumeOfWhite,              // startPoint .X and .Y are #pX and #pY
            double pixTol, double pixTolAverage, double[] meanColorPixel, int mSize, int nSize,
            double overlap, double bs2, double bsQuad, ColorMixType mixTypes, double volumeOfWhite,
            ref StrokeCandidate candidate) // #candidate_out = #candidate = candidate
        {

            bool accepted = false;
            // if we are outside the frame, do nothing
            if ((candidate.X < 0 || candidate.Y < 0 || candidate.X >= mSize || candidate.Y >= nSize))
                candidate.Error = double.MaxValue;
            else
            {
                // if dot is with small Error and doesn't match with canvas Color on the data picture
                int nX = (int)candidate.X;
                int nY = (int)candidate.Y;
                double[] doublePix = new double[] { img[nX, nY, 0], img[nX, nY, 1], img[nX, nY, 2] }; // one pixel
                List<double> errorPixel = new List<double>(); // #errPix
                for (int i = 0; i < doublePix.Length; i++)
                    errorPixel.Add(Math.Abs(meanColorPixel[i] - doublePix[i]));
                double meanOfErrorPixel = errorPixel.Average(); // #meanerrPix
                double avrcol = 0; // average Color
                if ((meanOfErrorPixel <= pixTol) && (canvas2[nX, nY, 1] == 0) &&
                    ((meanOfErrorPixel > canvasColor + canvasEps) || (meanOfErrorPixel < canvasColor - canvasEps)))
                {
                    int ncol = 0;
                    double[] colSum = new double[Functions.Constants.ColorLength];
                    double currentOverlap = 0; // #overlap
                    // line
                    double N = Math.Max(Math.Abs(startPoint.X - nX), Math.Abs(startPoint.Y - nY));
                    if (N == 0)
                        N = 1;
                    for (double t = 0; t <= 1; t += 1 / N)
                    {
                        double xo = Math.Round(nX + (startPoint.X - nX) * t);
                        double yo = Math.Round(nY + (startPoint.Y - nY) * t);

                        for (int X1 = (int)Math.Round(xo - bs2); X1 <= Math.Round(xo + bs2); X1++)
                        {
                            for (int Y1 = (int)Math.Round(yo - bs2); Y1 <= Math.Round(yo + bs2); Y1++)
                            {
                                if ((X1 >= 0) && (X1 < mSize) && (Y1 >= 0) && (Y1 < nSize))
                                {
                                    // test for overlap: number of mixtype is >=, and
                                    // amount of white is lower
                                    if (((ColorClass[X1, Y1] == (int)mixTypes) &&
                                        (VolumeOfWhite[X1, Y1] < volumeOfWhite)) ||
                                        (ColorClass[X1, Y1] == 0) ||
                                        (ColorClass[X1, Y1] < (int)mixTypes))
                                    { // if canvas is free
                                        double r2 = ((double)X1 - xo) * ((double)X1 - xo) + ((double)Y1 - yo) * ((double)Y1 - yo);
                                        if (r2 < bsQuad)
                                        {
                                            for (int i = 0; i < Functions.Constants.ColorLength; i++)
                                                colSum[i] += img[X1, Y1, i];
                                            ncol++;
                                            for (int i = 0; i < canvas2.Layers; i++)
                                            {
                                                if (canvas2[X1, Y1, i] != 0)
                                                {
                                                    currentOverlap++; // #overlap
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (ncol > 0)
                    {
                        for (int i = 0; i < Functions.Constants.ColorLength; i++)
                            avrcol += colSum[i] / ncol;

                        avrcol /= Functions.Constants.ColorLength;
                        currentOverlap /= ncol;
                    }
                    else
                        currentOverlap = overlap + 1; // large enough to not accept the pixel
                    // if the average Color of the stroke
                    // is in limits of acceptable
                    double meanColor = meanColorPixel.ToList().Average();
                    if ((Math.Abs(avrcol - meanColor) <= pixTolAverage) && (currentOverlap <= overlap))
                    {
                        double errCand = Math.Abs(avrcol - meanColor) + currentOverlap / ncol;
                        if (errCand < candidate.Error)
                        {
                            accepted = true;
                            candidate.X = nX; // was #candidate_out
                            candidate.Y = nY;
                            candidate.Error = errCand; 
                        }
                    }
                }
            }
            return accepted;
        }
    }
}
