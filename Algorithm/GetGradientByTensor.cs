using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using GeneralComponents;

namespace Algorithm
{
    /**
     * U и V это компоненты по x и y векторов градиентов. 
     * Каждый из них - двумерный массив размера с картинку. 
     * Градиент в точке (i,j) определяется компонентами [U(i,j), V(i,j)]. 
     * В дальнейшем градиент используется в функции getDirection, 
     * которая выдает направление, перпендикулярное градиенту.
     * 
     * 
     **/
    public struct Gradient
    {
        public Matrix2D U { get; private set; }
        public Matrix2D V { get; private set; }

       private Gradient(Matrix2D u, Matrix2D v)
       {
            U = u;
            V = v;
       }

        public Gradient(Matrix2D imgGray, int brushThickness) // GetGradientByTensor function
        {
            int g = (int)brushThickness * 4; // filter parameter
            double p1 = 0.183;
            //Matrix2D Dx = new Matrix2D( new List<List<double>> { // -Dx
            //    new List<double> { -0.5 * p1, 0, 0.5 * p1 },
            //    new List<double> {  p1 - 0.5, 0, 0.5 - p1 },
            //    new List<double> { -0.5 * p1, 0, 0.5 * p1 }
            //    });

            double[,] Dx = { // -Dx
                { -0.5 * p1, 0, 0.5 * p1 },
                {  p1 - 0.5, 0, 0.5 - p1 },
                { -0.5 * p1, 0, 0.5 * p1 }
            };

            double[,] Dy = { // transposed -Dx
                { -0.5 * p1, p1 - 0.5, -0.5 * p1 },
                {         0,        0,         0 },
                {  0.5 * p1, 0.5 - p1,  0.5 * p1 }
            };

            //Matrix2D Dy = new Matrix2D(new List<List<double>> { // transposed -Dx
            //new List<double> { -0.5 * p1, p1 - 0.5, -0.5 * p1 },
            //new List<double> {         0,        0,         0 },
            //new List<double> {  0.5 * p1, 0.5 - p1,  0.5 * p1 }
            //});

            Matrix2D fx = Functions.conv2(imgGray, Dx, "full");
            Matrix2D fy = Functions.conv2(imgGray, Dy, "full");

            Matrix2D E = fx.Square();
            Matrix2D F = Functions.getMatrixWithMultipliedElements(fx, fy);
            Matrix2D G = fy.Square();

            // smoothing: fspecial('gaussian',6,0.1);
            double[,] h = { { 0,         0,         0,         0,         0,         0},
                            { 0,         0,         0,         0,         0,         0},
                            { 0,         0,    0.2500,    0.2500,         0,         0},
                            { 0,         0,    0.2500,    0.2500,         0,         0},
                            { 0,         0,         0,         0,         0,         0},
                            { 0,         0,         0,         0,         0,         0} };

            E = Functions.conv2(E, h, "same"); // E  = imfilter(E,h);
            F = Functions.conv2(F, h, "same");
            G = Functions.conv2(G, h, "same");

            Matrix2D D = ((E - G).Square() + (F.Square() * 4)).Sqrt();

            Matrix2D lam1 = (E + G + D) * 0.5;

            // major eigenvector = gradient. If 0 make direction 1
            Matrix2D U =
                Functions.getMatrixWithMultipliedElements(F, 
                Functions.getMatrixWithElementsNotEqualsToZero10(F)) +
                Functions.getMatrixWithElementsEqualsToZero10(F);
            Matrix2D V = lam1 - E;
            V = 
                Functions.getMatrixWithMultipliedElements(V, 
                Functions.getMatrixWithElementsNotEqualsToZero10(V)) +
                Functions.getMatrixWithElementsEqualsToZero10(V);
            // now U and V are normalized
            // filter U and V
            double[,] H = Functions.getSquareMatrixOfSize_FilledBy_(g, 1d / (g * g));
            U = Functions.conv2(U, H, "same");
            V = Functions.conv2(V, H, "same");
            Matrix2D scalingFactor = (U.Square() + V.Square()).Sqrt(); // #scl
            scalingFactor = Functions.getMatrixWithElementsEqualsToZero10(scalingFactor) 
                + scalingFactor; // here is some difference because matlab rounds
            U = U / scalingFactor;
            V = V / scalingFactor;
            this.U = U;
            this.V = V;
        }

    }
}