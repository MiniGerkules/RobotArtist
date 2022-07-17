using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        double[,] U;
        double[,] V;

       private Gradient(double[,] u, double[,] v)
       {
            U = u;
            V = v;
       }

        public Gradient(double[,] imgGray, uint brushThickness) // GetGradientByTensor function
        {
            int g = (int)brushThickness * 4; // filter parameter
            double p1 = 0.183;
            double[,] Dx = {
            { 0.5 * p1, 0, -0.5 * p1 },
            { 0.5 - p1, 0, p1 - 0.5  },
            { 0.5 * p1, 0, -0.5 * p1 }
            };
            double[,] Dy = { // transposed Dx
            { 0.5 * p1, 0.5 - p1, 0.5 * p1 },
            { 0,        0,        0        },
            {-0.5 * p1, p1 - 0.5, -0.5 * p1}
            };
            
            double[,] fx = Functions.conv2(imgGray, Functions.minus(Dx), "same");
            double[,] fy = Functions.conv2(imgGray, Functions.minus(Dy), "same");

            double[,] E = Functions.getMatrixWithSquaredElements(fx);
            double[,] F = Functions.getMatrixWithMultipliedElements(fx, fy);
            double[,] G = Functions.getMatrixWithSquaredElements(fy);

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

            double[,] D = Functions.getMatrixWithRootExtractedElements(
                Functions.plus(
                    Functions.getMatrixWithSquaredElements(Functions.minus(E, G)),
                    Functions.getMatrixMultipliedByScalar(Functions.getMatrixWithSquaredElements(F), 4)
                ));

            double[,] lam1 = Functions.getMatrixMultipliedByScalar(Functions.plus(E, G, D), 0.5);

            // major eigenvector = gradient. If 0 make direction 1
            double[,] U = Functions.plus(
                Functions.getMatrixWithMultipliedElements(F, 
                Functions.getMatrixWithElementsNotEqualsToZero10(F)),
                Functions.getMatrixWithElementsEqualsToZero10(F));
            double[,] V = Functions.minus(lam1, E);
            V = Functions.plus(
                Functions.getMatrixWithMultipliedElements(V, 
                Functions.getMatrixWithElementsNotEqualsToZero10(V)),
                Functions.getMatrixWithElementsEqualsToZero10(V));
            // now U and V are normalized
            // filter U and V
            double[,] H = Functions.getSquareMatrixOfSize_FilledBy_(g, 1 / (g * g));
            U = Functions.conv2(U, H, "same");
            V = Functions.conv2(V, H, "same");
            double[,] scalingFactor = Functions.getMatrixWithRootExtractedElements(
                Functions.plus(Functions.getMatrixWithSquaredElements(U),
                Functions.getMatrixWithSquaredElements(V)));
            scalingFactor = Functions.plus(Functions.getMatrixWithElementsEqualsToZero10(
                scalingFactor), scalingFactor);
            U = Functions.getMatrixWithDividedElements(U, scalingFactor);
            V = Functions.getMatrixWithDividedElements(V, scalingFactor);
            this.U = U;
            this.V = V;
        }

    }
}