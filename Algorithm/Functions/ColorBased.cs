using System;
using Matrix3D = GeneralComponents.Matrix3D;

namespace Algorithm.Functions
{
    internal static class ColorBased
    {
        /// <summary>
        /// The method defines the matrix filled by 1 and 0
        /// 1 is placed on the element's position that is less or equal to scalar
        /// 0 is placed on the element's position that is greater than the scalar
        /// </summary>
        /// <param name="InitialImage"> A matrix representing an image </param>
        /// <param name="pX"> X-coordinate of the center of circle area </param>
        /// <param name="pY"> Y-coordinate of the center of circle area </param>
        /// <param name="bs2"> Brush Thickness </param>
        /// <param name="bsQuad"> Brush squared Thickness </param>
        /// <param name="m"> Height of an image </param>
        /// <param name="n"> Width of an image </param>
        /// <returns> A Color (represented as double[]) that is mean on image in circle area </returns>
        public static double[] GetMeanColor(Matrix3D InitialImage, int pX, int pY, double bs2, double bsQuad, int m, int n) // will return [R, G, B] - one pixel
        {
            double sumR = 0;
            double sumG = 0;
            double sumB = 0;
            int ncol = 0;
            for (double Xl = Math.Max(Math.Round(pX - bs2), 0); Xl < Math.Round(pX + bs2); Xl++)
            {
                for (double Yl = Math.Max(Math.Round(pY - bs2), 0); Yl < Math.Round(pY + bs2); Yl++)
                {
                    if (Xl >= 0 && Xl < m && Yl >= 0 && Yl < n) // if indexes are inside the canvas
                    {
                        if ((Xl - pX) * (Xl - pX) + (Yl - pY) * (Yl - pY) < bsQuad) // if indexes solve the circle equation
                        {
                            sumR += InitialImage[(int)Xl, (int)Yl, 0];
                            sumG += InitialImage[(int)Xl, (int)Yl, 1];
                            sumB += InitialImage[(int)Xl, (int)Yl, 2];
                            ncol++;
                        }
                    }
                }
            }
            // average Color of region count
            if (ncol > 0)
            {
                sumR /= ncol;
                sumG /= ncol;
                sumB /= ncol;
            }
            return new double[] { sumR, sumG, sumB };
        }
    }
}
