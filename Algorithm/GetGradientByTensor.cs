using GeneralComponents;

namespace Algorithm
{
    /**
     * U and V are components of gradient vectors. 
     * Any of U and V is a two dimensional array with sizes of the picture. 
     * The gradient in point (i,j) = [U(i,j), V(i,j)]. 
     * This struct is used in getDirection function, 
     * wich returns the direction, perpendicular to the gradient.
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
            int s = 6;
            int g = brushThickness * s; // filter parameter

            // % by Farid and Simoncelli
            Vector k = new Vector(new double[] { 0.004711, 0.069321, 0.245410, 0.361117, 0.245410, 0.069321, 0.004711 });
            Vector d = new Vector(new double[] { 0.018708, 0.125376, 0.193091, 0.000000, -0.193091, -0.125376, -0.018708 });
            //double p1 = 0.183;
            //Matrix2D Dx = new Matrix2D( new List<List<double>> { // -Dx
            //    new List<double> { -0.5 * p1, 0, 0.5 * p1 },
            //    new List<double> {  p1 - 0.5, 0, 0.5 - p1 },
            //    new List<double> { -0.5 * p1, 0, 0.5 * p1 }
            //    });

            //double[,] Dx = { // -Dx
            //    { -0.5 * p1, 0, 0.5 * p1 },
            //    {  p1 - 0.5, 0, 0.5 - p1 },
            //    { -0.5 * p1, 0, 0.5 * p1 }
            //};

            //double[,] Dy = { // transposed -Dx
            //    { -0.5 * p1, p1 - 0.5, -0.5 * p1 },
            //    {         0,        0,         0 },
            //    {  0.5 * p1, 0.5 - p1,  0.5 * p1 }
            //};

            //Matrix2D Dy = new Matrix2D(new List<List<double>> { // transposed -Dx
            //new List<double> { -0.5 * p1, p1 - 0.5, -0.5 * p1 },
            //new List<double> {         0,        0,         0 },
            //new List<double> {  0.5 * p1, 0.5 - p1,  0.5 * p1 }
            //});

            Matrix2D fx = Functions.conv2(new Matrix2D(k), new Matrix2D(d), imgGray, "same"); //  % derivative horizontally (wrt X)
            Matrix2D fy = Functions.conv2(new Matrix2D(d), new Matrix2D(k), imgGray, "same"); // % derivative vertically (wrt Y)

            //Matrix2D E = fx.Square();
            //Matrix2D F = Functions.getMatrixWithMultipliedElements(fx, fy);
            //Matrix2D G = fy.Square();

            // smoothing: fspecial('gaussian',6,0.1);
            //double[,] h = { { 0,         0,         0,         0,         0,         0},
            //                { 0,         0,         0,         0,         0,         0},
            //                { 0,         0,    0.2500,    0.2500,         0,         0},
            //                { 0,         0,    0.2500,    0.2500,         0,         0},
            //                { 0,         0,         0,         0,         0,         0},
            //                { 0,         0,         0,         0,         0,         0} };

            Matrix2D h = Functions.fspecial(0, "gaussian", new int[] { s * g, s * g }, s);

            //E = Functions.conv2(E, h, "same"); // E  = imfilter(E,h);
            //F = Functions.conv2(F, h, "same");
            //G = Functions.conv2(G, h, "same");

            //Matrix2D D = ((E - G).Square() + (F.Square() * 4)).Sqrt();

            //Matrix2D lam1 = (E + G + D) * 0.5;

            //// major eigenvector = gradient. If 0 make direction 1
            //Matrix2D U =
            //    Functions.getMatrixWithMultipliedElements(F, 
            //    Functions.getMatrixWithElementsNotEqualsToZero10(F)) +
            //    Functions.getMatrixWithElementsEqualsToZero10(F);
            //Matrix2D V = lam1 - E;
            //V = 
            //    Functions.getMatrixWithMultipliedElements(V, 
            //    Functions.getMatrixWithElementsNotEqualsToZero10(V)) +
            //    Functions.getMatrixWithElementsEqualsToZero10(V);
            //// now U and V are normalized
            //// filter U and V
            //double[,] H = Functions.getSquareMatrixOfSize_FilledBy_(g, 1d / (g * g));
            //U = Functions.conv2(U, H, "same");
            //V = Functions.conv2(V, H, "same");
            U = Functions.conv2(fx, h, "same");
            V = Functions.conv2(fy, h, "same");

            // % tensor code
            Matrix2D E = U ^ U;
            Matrix2D F = U ^ V;
            Matrix2D G = V ^ V;

            Matrix2D D = ((E - G).Square() + F.Square() * 4d).Sqrt();
            Matrix2D lam1 = (E + G + D) * 0.5;

            U = F;
            V = lam1 - E;

            Matrix2D scalingFactor = (U.Square() + V.Square()).Sqrt(); // #scl
            scalingFactor = Functions.getMatrixWithElementsEqualsToZero10(scalingFactor)
                + scalingFactor; // here is some difference because matlab rounds
            U /= scalingFactor;
            V /= scalingFactor;
            //this.U = U;
            //this.V = V;
        }
    }
}