using GeneralComponents;
using System;
using Point = System.Windows.Point;

namespace Algorithm
{
    /// <summary>
    /// Gradient cosists of two matricies (Matrix2D) - U and V
    /// U and V are components of gradient vectors. 
    /// Any of U and V is a two dimensional array with sizes of the picture. 
    /// The gradient in point (i,j) = [U(i,j), V(i,j)]. 
    /// This struct is used in getDirection function, 
    /// wich returns the direction, perpendicular to the gradient.
    /// </summary>
    public struct Gradient
    {
        public Matrix2D U { get; private set; }
        public Matrix2D V { get; private set; }

        /// <summary>
        /// The constructor creates a Gradient's matricies from imgGray
        /// using Farid and Simoncelli number matricies
        /// </summary>
        /// <param name="imgGray"> The gray image to find gradients for </param>
        /// <param name="brushThickness"> Brush Thickness </param>
        /// <returns> Gradient matricies </returns>
        public Gradient(Matrix2D imgGray, int brushThickness) // GetGradientByTensor function
        {
            int s = 6;
            int g = brushThickness * s; // filter parameter

            // by Farid and Simoncelli
            Vector k = new Vector(new double[] { 0.004711, 0.069321, 0.245410, 0.361117, 0.245410, 0.069321, 0.004711 });
            Vector d = new Vector(new double[] { 0.018708, 0.125376, 0.193091, 0.000000, -0.193091, -0.125376, -0.018708 });

            Matrix2D fx = Functions.Filtration.Conv2(new Matrix2D(k), new Matrix2D(d), imgGray); //  % derivative horizontally (wrt X)
            Matrix2D fy = Functions.Filtration.Conv2(new Matrix2D(d), new Matrix2D(k), imgGray, "full"); // % derivative vertically (wrt Y)

            Matrix2D h = Functions.Filtration.Fspecial(0, "gaussian", new int[] { s * g, s * g }, s);

            U = Functions.Filtration.Conv2(fx, h, "same");
            V = Functions.Filtration.Conv2(fy, h, "same");

            // tensor code
            Matrix2D E = U ^ U;
            Matrix2D F = U ^ V;
            Matrix2D G = V ^ V;

            Matrix2D D = ((E - G).Square() + F.Square() * 4d).Sqrt();
            Matrix2D lam1 = (E + G + D) * 0.5;

            U = F;
            V = lam1 - E;

            Matrix2D scalingFactor = (U.Square() + V.Square()).Sqrt(); // #scl
            scalingFactor = scalingFactor.IsEqualsZero() + scalingFactor; // here is some difference because matlab rounds
            U /= scalingFactor;
            V /= scalingFactor;
        }

        /// <summary>
        /// The method defines the angle of stroke to draw by calculating cos and sin
        /// using start point of the stroke, stroke and information about the way of painting
        /// based on gradients matrix
        /// </summary>
        /// <param name="startPoint"> The point stroke starts with </param>
        /// <param name="stroke"> The stroke </param>
        /// <param name="goNormal"> Defines go normal to gradient or lengthwise </param>
        /// <param name="cosA"> The cos of the angle </param>
        /// <param name="sinA"> The sin of the angle </param>
        /// <returns> Out parameters are cos and sin of the angle </returns>
        public void GetDirection(Point startPoint, Stroke stroke, bool goNormal, out double cosA, out double sinA)
        {
            if (stroke.Points.Count == 0)
                throw new ArgumentException();

            if (goNormal)
            {
                cosA = -U[(int)startPoint.X, (int)startPoint.Y];
                sinA = V[(int)startPoint.X, (int)startPoint.Y]; // normally to gradient
            }
            else
            {
                cosA = U[(int)startPoint.X, (int)startPoint.Y];
                sinA = V[(int)startPoint.X, (int)startPoint.Y]; // collinearly to gradient
            }

            if (stroke.Points.Count > 1) // if not a single point, get previous direction
            {
                double dX = startPoint.X - stroke.Points[stroke.Points.Count - 2].X;
                double dY = startPoint.Y - stroke.Points[stroke.Points.Count - 2].Y;

                // get scalar product
                double scalar = cosA * dX + sinA * dY;
                if (scalar < 0) // if in opposite direction
                {
                    cosA = -cosA;
                    sinA = -sinA;
                }
            }
        }
    }
}