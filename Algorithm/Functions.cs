﻿using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using GeneralComponents;
using Matrix3D = GeneralComponents.Matrix3D;
using Point = System.Windows.Point;

namespace Algorithm
{
    public static class Functions
    {
        const double eps = 2.2204e-16;
        /*
         * returns a vector of vectorSize of random integer numbers in range 1:limit
         */
        public static int[] Randperm(int limit, int vectorSize)
        {
            int[] answer = new int[vectorSize];
            var rand = new Random();
            for (int i = 0; i < vectorSize; i++)
                answer[i] = rand.Next(limit - 1) + 1;
            return answer;
        }
        public static Matrix2D drawPiece(Point startPoint, StrokeCandidate candidate, double bs2, double bsQuad, 
            ref Matrix3D canvas, ref Matrix3D canvas2, ref Matrix2D ColorClass, ref Matrix2D VolumeOfWhite, double[] meanColorPixel, double[] col2,
            Matrix2D imggray, int mSize, int nSize, ColorMixType mixTypes, double volumeOfWhite)
        {
            double N = Math.Max(Math.Abs(startPoint.X - candidate.x), Math.Abs(startPoint.Y - candidate.y));
            double bsQuad12 = ((double)(bs2) / 2) * ((double)(bs2) / 2);

            Matrix3D canvasOut = canvas;
            Matrix2D colorClass = ColorClass;
            Matrix2D volOfWhite = VolumeOfWhite;
            Matrix3D canvas2Out = canvas2;

            for (double t = 0; t <= 1; t += (1 / N))
            {
                double xo = Math.Round(candidate.x + (startPoint.X - candidate.x) * t);
                double yo = Math.Round(candidate.y + (startPoint.Y - candidate.y) * t);
                for (double Xl = Math.Round(xo - bs2); Xl <= Math.Round(xo + bs2); Xl++)
                {
                    for (double Yl = Math.Round(yo - bs2); Yl <= Math.Round(yo + bs2); Yl++)
                    {
                        if (Xl >= 0 && Xl < mSize && Yl >= 0 && Yl < nSize)
                        {
                            if ((Xl - xo) * (Xl - xo) + (Yl - yo) * (Yl - yo) < bsQuad)
                            {
                                // % test for overlap: number of mixtype is >=, and
                                // % amount of white is lower
                                if (((int)ColorClass[(int)Xl, (int)Yl] == (int)mixTypes) && (VolumeOfWhite[(int)Xl, (int)Yl] < volumeOfWhite)
                                    || ((int)ColorClass[(int)Xl, (int)Yl] == 0) || ((int)ColorClass[(int)Xl, (int)Yl] < (int)mixTypes))
                                {
                                    // %if canvas is free
                                    for (int i = 0; i < meanColorPixel.Length; i++)
                                        canvasOut[(int)Xl, (int)Yl, i] = meanColorPixel[i];
                                    colorClass[(int)Xl, (int)Yl] = (int)mixTypes;
                                    volOfWhite[(int)Xl, (int)Yl] = volumeOfWhite;

                                    for (int i = 0; i < col2.Length; i++)
                                        canvas2Out[(int)Xl, (int)Yl, i] = col2[i]; // % new variant
                                }
                            }
                        }
                    }
                }
            }

            canvas = canvasOut;
            ColorClass = colorClass;
            VolumeOfWhite = volOfWhite;
            canvas2 = canvas2Out;

            return (canvasOut.mean() - imggray).Abs();
        }


        // startPoint .X and .Y are #pX and #pY
        // #testNewPiece
        public static bool testNewPieceAccepted(Point startPoint, Matrix3D img, byte canvasColor, 
            double canvasEps, Matrix3D canvas2, Matrix2D ColorClass, Matrix2D VolumeOfWhite,
            double pixTol, double pixTolAverage, double[] meanColorPixel, int mSize, int nSize,
            double overlap, double bs2, double bsQuad, ColorMixType mixTypes, double volumeOfWhite, 
            ref StrokeCandidate candidate/*, out double error*/) // #candidate_out = #candidate = candidate
        { // it seems like there is no need in error
            
            bool accepted = false;
            // StrokeCandidate candidate_out = candidate;
            if ((candidate.x < 0 || candidate.y < 0 || candidate.x >= mSize || candidate.y >= nSize))
                candidate.error = double.MaxValue; // changed!
            else
            {
                // if dot is with small error and doesn't match with canvas color on the data picture
                int nX = (int)candidate.x;
                int nY = (int)candidate.y;
                double[] doublePix = new double[] { img[nX, nY, 0], img[nX, nY, 1], img[nX, nY, 2] }; // one pixel
                List<double> averageModule = new List<double>();
                for (int i = 0; i < doublePix.Length; i++)
                    averageModule.Add(Math.Abs(meanColorPixel[i] - doublePix[i]));
                double errPix = averageModule.Average();
                double avrcol = 0; // average color
                double meanOfDoublePix = doublePix.ToList().Average();
                if ((errPix <= pixTol) && (canvas2[nX, nY, 1] == 0) && 
                    ((meanOfDoublePix > canvasColor + canvasEps) || (meanOfDoublePix < canvasColor - canvasEps)))
                {
                    int ncol = 0;
                    double[] colSum = new double[3];
                    double currentOverlap = 0; // #overlap
                    // %line
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
                                    // %test for overlap: number of mixtype is >=, and
                                    // %amount of white is lower
                                    if (((ColorClass[X1, Y1] == (int)mixTypes) && 
                                        (VolumeOfWhite[X1, Y1] < volumeOfWhite)) || 
                                        (ColorClass[X1, Y1] == 0) || 
                                        (ColorClass[X1, Y1] < (int)mixTypes)) //don't enter when should!!!!
                                    { // %if canvas is free
                                        double r2 = ((double)X1 - xo) * ((double)X1 - xo) + ((double)Y1 - yo) * ((double)Y1 - yo);
                                        if (r2 < bsQuad)
                                        {
                                            for (int i = 0; i < 3; i++)
                                                colSum[i] += img[X1, Y1, i];
                                            ncol++;
                                            if (canvas2[X1, Y1, 0] != 0)
                                                currentOverlap++; // #overlap
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (ncol > 0)
                    {
                        for (int i = 0; i < 3; i++)
                            avrcol += colSum[i] / ncol;

                        avrcol /= 3;
                        currentOverlap /= ncol;
                    }
                    else
                        currentOverlap = overlap + 1; // large enough to not accept the pixel
                    // if the average color of the stroke
                    // is in limits of acceptable
                    double meanColor = meanColorPixel.ToList().Average();
                    if ((Math.Abs(avrcol - meanColor) <= pixTolAverage) && (currentOverlap <= overlap))
                    {
                        double errCand = Math.Abs(avrcol - meanColor) + currentOverlap / ncol;
                        if (errCand < candidate.error)
                        {
                            accepted = true;
                            candidate.x = nX; // was candidate_out
                            candidate.y = nY; // was candidate_out
                            candidate.error = errCand; // was candidate_out
                        }
                    }
                }
                //error = candidate_out.error;
                //candidate = candidate_out;
            }
            return accepted;
        }

        public static void getDirection (Point startPoint, Stroke stroke, Gradient gradient, bool goNormal, out double cosA, out double sinA)
        {
            if (stroke.points.Count == 0)
                throw new ArgumentException();

            if (goNormal)
            {
                cosA = -gradient.U[(int)startPoint.X, (int)startPoint.Y];
                sinA = gradient.V[(int)startPoint.X, (int)startPoint.Y]; // normally to gradient
            }
            else
            {
                cosA = gradient.U[(int)startPoint.X, (int)startPoint.Y];
                sinA = gradient.V[(int)startPoint.X, (int)startPoint.Y]; // collinearly to gradient
            }

            if (stroke.points.Count > 1) // %if not a single point, get previous direction
            {
                double dX = startPoint.X - stroke.points[stroke.points.Count - 1].X;
                double dY = startPoint.Y - stroke.points[stroke.points.Count - 1].Y;

                // %get scalar product
                double scalar = cosA * dX + sinA * dY;
                if (scalar < 0) // %if in opposite direction
                {
                    cosA = -cosA;
                    sinA = -sinA;
                }
            }
        }

        public static double[] proportions2pp(double[] proportions, ColorMixType mixTypes) // #prop2pp
        {
            int vTotal = 3000;
            double C = 0, M = 0, Y = 0; //% initiate colors
            double a = proportions[0];
            double b = proportions[1];
            double c = proportions[2];
            double C1 = Math.Floor(a * c * vTotal);
            double C2 = Math.Floor((1 - a) * c * vTotal);
            double B = Math.Floor((1 - c) * b * vTotal);
            double W = Math.Floor((1 - b) * (1 - c) * vTotal);
            switch(mixTypes)
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

        public static Matrix3D imageToMatrix3D(BitmapImage image)
        {
            FormatConvertedBitmap imageInRGB24 = new FormatConvertedBitmap(image, PixelFormats.Rgb24, BitmapPalettes.Halftone256, 0);
            int bpp = 24; // bits per pixel
            int layersAmount = 3;
            //bpp = Image.Format.BitsPerPixel; // omg pictures opens in bgr32
            int width = imageInRGB24.PixelWidth; // amount of columns
            int height = imageInRGB24.PixelHeight; // amount of rows
            int depth = ((bpp + 7) / 8); // layers amount (will be 3)
            int stride = width * depth;
            int size = height * stride; // rows in bytes
            byte[] pixels = new byte[size];
            imageInRGB24.CopyPixels(pixels, stride, 0);

            List<List<List<double>>> listedBytes = new List<List<List<double>>>(layersAmount);

            for (int k = 0; k < layersAmount; k++)
                listedBytes.Add(new List<List<double>>(height));

            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < layersAmount; k++)
                    listedBytes[k].Add(new List<double>(width));

                for (int i = 0; i < width; i++)
                {
                    int index = j * stride + ((bpp + 7) / 8) * i;

                    listedBytes[0][j].Add(pixels[index]); // R
                    listedBytes[1][j].Add(pixels[index + 1]); // G
                    listedBytes[2][j].Add(pixels[index + 2]); // B
                }
            }

            return new Matrix3D(listedBytes);
        }

        public static double[] GaussSolution(double[,] matrix, double[] solution)
        {
            int rowsAmount = matrix.GetLength(0);
            int columnsAmount = matrix.GetLength(1);

            if (rowsAmount != columnsAmount)
                throw new ArgumentException("Matrix2D should be square!");

            for (int i = 0; i < rowsAmount; i++) // ходим по строкам
            {
                // надо найти строку с наименьшим по модулю не равным нулю i элементом и поменять местами
                // строку, в котрой этот элемент, со строкой i-той, то есть текущей
                if (sortFromIndex(i, matrix, solution)) // тут мы это и делаем, и если все хорошо, то заходим
                {
                    // прямой ход Гаусса
                    solution[i] /= matrix[i, i];
                    for (int k = columnsAmount - 1; k >= i; k--)
                    {
                        matrix[i, k] /= matrix[i, i]; // чтобы первый ненулевой в строке элемент = 1

                    }
                    for (int j = i + 1; j < rowsAmount; j++) // обнуляем элементы под текущим элементом, т е вычитаем строки
                    {
                        solution[j] -= solution[i] * matrix[j, i];
                        for (int k = columnsAmount - 1; k >= i; k--)
                        {
                            matrix[j, k] -= matrix[i, k] * matrix[j, i];

                        }
                    }
                }
                else
                {
                    // а вот что тогда? - попадем сюда, если решений у системы бесконечно много
                    // в принципе можно просто обнулить переменную, выбрав таким образом конкретное решение
                    //  и тогда уже в предыдущих строках тоже надо занулить эту переменную
                    for (int k = 0; k <= i; k++)
                        matrix[k, i] = 0;
                }
            }

            // обратный ход Гаусса
            for (int i = rowsAmount - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    solution[j] -= solution[i] * matrix[j, i];
                    matrix[j, i] = 0;
                }
            }
            // теперь в solution содержится ответ = решение СЛАУ
            // единственное, что если в строке все нули, а справа не ноль, то решений СЛАУ нет
            // а если все нули и справа ноль, то их бесконечно много (тут просто выберем значение ноль для переменной с любым значением, то есть можно ничего не менять)
            for (int i = 0; i < rowsAmount; i++)
            {
                if (matrix[i, i] == 0 && solution[i] != 0)
                {
                    // а вот что делать тут и правда не ясно!
                    // искать псевдорешение? это ыыыы
                    throw new Exception("Solution of the system doesn't exist!");
                }
            }
            return solution;
        }

        public static bool sortFromIndex(int index, double[,] array, double[] rightPart) // returns false if didn't find the non Zero
        { // false will show that the linear system has infinitely many solutions
            int rowsAmount = array.GetLength(0);
            int columnsAmount = array.GetLength(1);
            int minNonZeroAbsIndex = -1;
            for (int i = index; i < rowsAmount; i++)
            {
                if (minNonZeroAbsIndex == -1 && array[i, index] != 0)
                    minNonZeroAbsIndex = i;
                else if (array[i, index] != 0)
                {
                    if (Math.Abs(array[minNonZeroAbsIndex, index]) > Math.Abs(array[i, index]))
                        minNonZeroAbsIndex = i;
                }
            }
            if (minNonZeroAbsIndex == -1)
                return false;
            if (index != minNonZeroAbsIndex)
            {
                for (int j = index; j < columnsAmount; j++)
                    // swap elements of the rows, previous should be zero
                    (array[index, j], array[minNonZeroAbsIndex, j]) = (array[minNonZeroAbsIndex, j], array[index, j]);
                (rightPart[index], rightPart[minNonZeroAbsIndex]) = (rightPart[minNonZeroAbsIndex], rightPart[index]);
            }
            return true;
        }

        public static double EuclideanDistance(double[] first, double[] second)
        {
            double distance = 0;
            int length = first.Length;
            if (length != second.Length)
                throw new ArgumentException("dimensions of vectors should match!");
            for (int i = 0; i < length; i++)
                distance += (first[i] - second[i]) * (first[i] - second[i]);
            //distance = Math.Sqrt(distance);
            return distance;
        }

        public static double[] saturation(double[] prop)
        {
            double[] val = new double[prop.Length];
            for (int i = 0; i < prop.Length; i++)
            {
                val[i] = Math.Min(Math.Max(prop[i], 0), 1);
            }
            return val;
        }

        internal static void PredictProportions(out double[] proportions, out ColorMixType mixTypes, double[] hsvColor, List<List<List<double>>> Ycell, List<List<List<double>>> Wcell, int sheetsAmount = 4)
        {

            int K = 25;
            double tolerance = 0.2; // #tol - tolerance of color error in rgb

            List<List<double>> Tbl = new List<List<double>>();
            List<int> Clss = new List<int>(); // one big column of indicies

            // Ycell and Wcell are guaranteed not null here

            for (int i = 0; i < sheetsAmount; i++)
            {
                int Nset = Ycell[i].Count;

                for (int j = 0; j < Ycell[i].Count; j++)
                    Tbl.Add(Ycell[i][j]);

                int[] range = new int[Nset];

                for (int k = 0; k < Nset; k++)
                    range[k] = i;

                Clss.AddRange(range);
            }

            int N = Tbl.Count;
            // #Nhsv is 1, so delete it

            //now, create classification table and class list
            double[] hsvNewColor = hsvColor.Clone() as double[]; // #hsvnew
            proportions = new double[3];
            mixTypes = (ColorMixType)0; // #cls
            double[] hsvArray = new double[3];

            Matrix2D M = (new Matrix2D(new List<double> { 1d, 1d / 16, 1d })).MakeDiag();

            // then, replace the current color with the accessible one
            double[] hsvColorCurrent = hsvNewColor.Clone() as double[];
            // make prediction from the closest point
            int Ks = 10;
            Matrix2D dst = new Matrix2D(N, 1); // distances - taken as maximum range

            for (int n = 0; n < N; n++)
                dst[n] = EuclideanDistance(hsvColorCurrent, Tbl[n].ToArray());

            int[] I = dst.GetIndexesForSorted();
            //int[] clssIndexes = I.Take(Ks).ToArray();
            int[] clvec = new int[Ks];

            for (int i = 0; i < Ks; i++)
                clvec[i] = Clss[I[i]];

            int[] classes = new int[4];

            for (int i = 0; i < Ks; i++)
                classes[clvec[i]]++;

            int clsvect = classes.ToList().IndexOf(classes.Max());
            mixTypes = (ColorMixType)clsvect; // #cls(i) (i = 1)
            // then, find second possible class
            int ctr = 0; // index
            int ctri = 0; // index
            Matrix2D class2vect = new Matrix2D(K, 1);
            while (ctr < Clss.Count && ctri < K)
            {
                if ((ColorMixType)Clss[I[ctr]] != mixTypes)
                {
                    class2vect[ctri] = Clss[I[ctr]];
                    ctri++;
                }
                ctr++;
            }

            classes = new int[4];

            for (int i = 0; i < ctri; i++)
                classes[(int)class2vect[i]]++;

            int class2 = classes.ToList().IndexOf(classes.Max());
            bool flag = true;
            ctr = 0; // index
            double[] props0 = new double[3]; // initial proportions?
            double[] propscur = new double[3]; // current proportions
            double err0 = 0;

            double[] hsvinv = { 0, 0, 0 };

            while (flag)
            {
                List<List<double>> Y = Ycell[(int)mixTypes];
                List<List<double>> W = Wcell[(int)mixTypes];
                int NY = Y.Count; // amount of rows in Y
                                  //List<Matrix2D> dist = new List<Matrix2D> (); // distances - taken as maximum range
                Matrix2D dist = new Matrix2D(NY, 1);
                for (int n = 0; n < NY; n++)
                {
                    Matrix2D hsvColorCurrentMatrix = new Matrix2D(hsvColorCurrent.ToList());
                    Matrix2D Yn = new Matrix2D(Y[n]);
                    Matrix2D temp = (hsvColorCurrentMatrix - Yn);
                    dist[n, 0] = (double)(temp * M * temp.Transpose());
                }
                int[] indexes = dist.GetIndexesForSorted();

                Matrix2D d = new Matrix2D(1, K);
                Matrix2D ds = new Matrix2D(1, K); // transposed

                for (int p = 0; p < K; p++)
                {
                    d[0, p] = 1d / dist[indexes[p]];
                    ds[0, p] = Math.Sqrt(d[p]);
                }

                Matrix2D D = d.MakeDiag();
                // hsvpossible = ds'*Y(I(1:K),:)/sum(ds); 
                List<List<double>> Y_IK = new List<List<double>>();
                for (int i = 0; i < K; i++)
                    Y_IK.Add(Y[indexes[i]]);
                Matrix2D hsvpossible = ds * (new Matrix2D(Y_IK)) / ds.GetSum();
                double al = 0.7;
                for (int i = 0; i < hsvNewColor.Length; i++)
                    hsvNewColor[i] = hsvpossible[i] * (1 - al) + hsvColorCurrent[i] * al;

                hsvColorCurrent = hsvNewColor.Clone() as double[];


                // %take first K points, make linear regression
                for (int j = 0; j < 3; j++)
                {
                    Matrix2D X = new Matrix2D(Y_IK);

                    Matrix2D E = new Matrix2D(K, 4); // evaluated polynomial
                    List<List<double>> T = new List<List<double>> {
                            new List<double> { 0, 0, 0 },
                            new List<double> { 1, 0, 0 },
                            new List<double> { 0, 1, 0 },
                            new List<double> { 0, 0, 1 }
                        }; // monomial orders
                    Matrix2D h = Matrix2D.Eye(4);

                    for (int k = 0; k < 4; k++)
                    {
                        double[] product = prod2(matrixToMatrixDegrees(X, (new Matrix2D(T[k])).RepeatRows(K)));
                        for (int r = 0; r < K; r++)
                        {
                            for (int c = 0; c < E.Columns; c++)
                            {
                                E[r, c] += product[r] * h[k, c];
                            }
                        }
                    }

                    double delt = 1e-14; // %for Tikhonov regularization

                    Matrix2D Wj = new Matrix2D(K, 1);

                    for (int k = 0; k < K; k++)
                        Wj[k] = W[indexes[k]][j];

                    // h = (E'*D*E)\(E'*D*W(I(1:K),j)); %WLS
                    Matrix2D Etransposed = E.Transpose();

                    Matrix2D answers = Etransposed * D * Wj;

                    Matrix2D coefs = Etransposed * D * E + (Matrix2D.Eye(4)) * delt;

                    h = GausMethod.Solve(coefs, answers); // answers don't match, but they pretty close

                    // predict proportion

                    double p = 0;

                    for (int k = 0; k < 4; k++)
                        p += h[k] * prod2(matrixToMatrixDegrees(hsvColorCurrent, T[k].ToArray()));
                    propscur[j] = p;
                }

                // get into ranges [0,1]
                propscur = saturation(propscur);
                // test inversion
                hsvinv = prop2hsv(propscur, mixTypes, Wcell, Ycell);

                // here a section about h_alt, remake
                double[] hsvinv2 = hsvinv.Clone() as double[];
                hsvinv2[0] = (Math.Abs(hsvinv[0] - hsvColorCurrent[0]) < Math.Abs(hsvinv[0] - 1 - hsvColorCurrent[0])) ? hsvinv[0] : (hsvinv[0] - 1);

                double[] hsvdemanded = hsvColor.Clone() as double[];

                double err = Math.Sqrt(EuclideanDistance(hsvdemanded, hsvinv2));

                if (ctr < 1)
                {
                    // if the values are calculated for the first time
                    if (err > tolerance)
                    {
                        ctr++;
                        mixTypes = (ColorMixType)class2;
                        props0 = propscur;
                        err0 = err;
                    }
                    else
                        flag = false; // go out of the loop
                }
                else
                {
                    // if the values are calculated for the second time
                    if (err > err0) // if error is greater, return to first variant
                    {
                        propscur = props0;
                        mixTypes = (ColorMixType)clsvect;
                        //hsvinv = prop2hsv(propscur, mixTypes, Wcell, Ycell);
                    }
                    flag = false; // anyway, go out of the loop
                }
            }
            proportions = propscur;
            // now have right hsvNewColor
            hsvArray = hsvNewColor.Clone() as double[];
        }
        internal static double[] prop2hsv(double[] proportions, ColorMixType mixTypes, List<List<List<double>>> Ycell, List<List<List<double>>> Wcell, int sheetsAmount = 4)
        {
            int K = 150;

            // Ycell and Wcell are granted not null

            double[] hsvcol = new double[3];

            List<List<double>> Y = new List<List<double>>();
            List<List<double>> W = new List<List<double>>();
            // take the corresponding sets
            if ((int)(mixTypes) < 2)
            {
                Y = Ycell[0];
                Y.AddRange(Ycell[1]);
                W = Wcell[0];
                for (int j = 0; j < W.Count; j++)
                    W[j][0]--; // for mixtype MY with H > 0.4 make H negative
                W.AddRange(Wcell[1]);
            }
            else
            {
                Y = Ycell[(int)mixTypes]; // Y for proportions
                W = Wcell[(int)mixTypes]; // W for HSV
            }

            int NY = Y.Count;
            Matrix2D dst = new Matrix2D(NY, 1);
            for (int k = 0; k < NY; k++)
                dst[k] = Math.Sqrt(EuclideanDistance(proportions, Y[k].ToArray()));

            int[] indexes = dst.GetIndexesForSorted();

            double[] hsvcolcur = new double[3];

            int N = 4;

            List<List<double>> T = new List<List<double>> {
                            new List<double> { 0, 0, 0 },
                            new List<double> { 1, 0, 0 },
                            new List<double> { 0, 1, 0 },
                            new List<double> { 0, 0, 1 }
                        }; // deglexord(0,1,3);
            // take first K points
            K = Math.Min(NY, K); // decrease K if needed

            Matrix2D X = new Matrix2D(K, Y[0].Count);

            for (int p = 0; p < K; p++)
                for (int q = 0; q < Y[p].Count; q++)
                    X[p, q] = Y[indexes[p]][q];

            Matrix2D E = new Matrix2D(K, N, 0); // evaluated polynomial
            double[][] h = Eye(N);
            for (int k = 0; k < N; k++)
            {
                double[] product = prod2(matrixToMatrixDegrees(X, (new Matrix2D(T[k])).RepeatRows(K)));
                for (int r = 0; r < K; r++)
                {
                    for (int c = 0; c < E.Columns; c++)
                    {
                        E[r, c] += product[r] * h[k][c];
                    }
                }
            }

            for (int j = 0; j < 3; j++)
            {
                Matrix2D Wj = new Matrix2D(K, 1);

                for (int k = 0; k < K; k++)
                    Wj[k] = W[indexes[k]][j];

                // h = (E'*E)\(E'*V); %OLS
                Matrix2D Etransposed = E.Transpose();

                GeneralComponents.Matrix2D h2 = GeneralComponents.GausMethod.Solve(
                    (Etransposed * E), (Etransposed * Wj));

                // predict proportion

                hsvcolcur[j] = 0;
                double[] x = proportions;

                for (int k = 0; k < N; k++)
                {
                    double product = prod2(matrixToMatrixDegrees(x, T[k].ToArray()));

                    for (int c = 0; c < h2.Columns; c++)
                        hsvcolcur[j] += product * h2[k, c];
                }
            }

            if (hsvcolcur[0] < 0)
                hsvcolcur[0]++; // use this because models predict with shift
                                // get into ranges [0,1]
            hsvcol = saturation(hsvcolcur);

            return hsvcol;
        }
        public static double[] prod2(Matrix2D matrix) // prod(matrix, 2) 
        {
            int length = matrix.Rows;
            double[] answer = new double[length];
            for (int i = 0; i < length; i++)
            {
                answer[i] = 1;
                for (int j = 0; j < matrix.Columns; j++)
                {
                    answer[i] *= matrix[i, j];
                }
            }
            return answer;
        }

        public static double prod2(double[] array)
        {
            double answer = 1;
            for (int i = 0; i < array.Length; i++)
                answer *= array[i];
            return answer;
        }

        public static double[,] repmat(double[] data, int repeatRows = 1, int repeatColumns = 1)
        {
            double[,] answer = new double[repeatRows, data.Length * repeatColumns];

            for (int j = 0; j < data.Length * repeatColumns; j++)
            {
                for (int i = 0; i < repeatRows; i++)
                {
                    answer[i, j] = data[j - (j / data.Length) * data.Length];
                }
            }
            return answer;
        }

        public static double[][] Eye(int dimension)
        {
            if (dimension <= 0)
                throw new ArgumentException("dimension should be > 0");
            if (dimension == 1)
                return new double[][] { new double[] { 1 } };
            double[][] answer = new double[dimension][];
            for (int i = 0; i < dimension; i++)
            {
                answer[i] = new double[dimension];
                answer[i][i] = 1;
            }

            return answer;
        }

        /**
         * linspace returns [amount] dots wich are evenly distributed in [start; end]
         * including extreme points
         * Example: linspace(-5,5,7) is an array: -5.0000 -3.3333 -1.6667 0 1.6667 3.3333 5.0000
         **/
        public static double[] linspace(double start, double end, int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("amount of dots should be a positive integer number!");
            double[] result = new double[amount];
            if (amount == 1)
                result[0] = (start + end) / 2;
            else
            {
                double step = (end - start) / (amount - 1);
                for (int i = 0; i < amount; i++)
                {
                    result[i] = start + i * step;
                }
            }
            return result;
        }

        /** rgb is 255 (byte) format, hsv is 1-format, h - hue, s - saturation, v - value
         * gets an rgb-pixel and return an hsv-pixel
        **/
        public static double[] rgb2hsv(double[] pixel)
        {
            double r = pixel[0];
            double g = pixel[1];
            double b = pixel[2];
            double h = 0;
            double s = Math.Min(r, Math.Min(g, b));    //Min. value of RGB
            double v = Math.Max(r, Math.Max(g, b));    //Max. value of RGB
            double del = v - s;             //Delta RGB value

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
            s = 1 - s / v;
            return new double[] { h, s, v };
        }

        /** rgb is 1-format, hsv is 1-format, h - hue, s - saturation, v - value
         * gets an hsv-pixel and return an rgb-pixel
        **/
        public static double[] hsv2rgb(double[] hsvPixel)
        {
            double h = hsvPixel[0];
            double s = hsvPixel[1];
            double v = hsvPixel[2];
            double[] rgbPixel = new double[3];
            rgbPixel[0] = rgbPixel[1] = rgbPixel[2] = v * (1 - s);
            //  red = hue-2/3 : green = hue : blue = hue-1/3
            // Apply modulo 1 for red and blue to keep within range [0, 1]
            double[] hueForRgb = new double[3];
            hueForRgb[0] = mod(h - 2 / 3d, 1);
            hueForRgb[1] = h;
            hueForRgb[2] = mod(h - 1 / 3d, 1);
            double[] factor = new double[3];
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

            for (int i = 0; i < 3; i++)
            {

                rgbPixel[i] += factor[i] * (6d * hueIsLess(hueForRgb[i], 1 / 6d) * hueForRgb[i]
                    + (hueIsNotLess(hueForRgb[i], 1 / 6d) & hueIsLess(hueForRgb[i], 1 / 2d))
                    + (hueIsNotLess(hueForRgb[i], 1 / 2d) & hueIsLess(hueForRgb[i], 2 / 3d)) * (4d - 6d * hueForRgb[i]));
            }
            return rgbPixel;
        }

        public static double mod(double dividend, double divisor)
        {
            if (divisor == 0)
                throw new ArgumentException("divisor is zero");
            return (dividend - divisor * Math.Floor(dividend / divisor));
        }

        public static Matrix2D fspecial(int radius, string type = "disk", int[] hSize = null, double sigma = 0.5)
        {
            Matrix2D answer = new Matrix2D (new double[,]{ { 1 } });

            if (type == "disk") // %   H = FSPECIAL('disk',RADIUS) returns a circular averaging filter
                                // % (pillbox)within the square matrix of side 2 * RADIUS + 1.
                                // % The default RADIUS is 5.
            {
                if (radius < 0)
                    throw new ArgumentException("fspecial: RADIUS for disk must be a real scalar");
                else if (radius == 0)
                    return answer;
                int ax = radius; // index of the "x-axis" and "y-axis"
                double corner = Math.Floor(radius / Math.Sqrt(2) + 0.5) - 0.5; // corner corresponding to 45 degrees
                double rsq = radius * radius;
                // First set values for points completely covered by the disk
                int dimension = 2 * radius + 1;
                double[,] X = new double[dimension, dimension];
                double[,] Y = new double[dimension, dimension];
                for (int i = 0; i < dimension; i++)
                {
                    for (int j = 0; j < dimension; j++)
                    {
                        X[i, j] = j - radius;
                        Y[i, j] = i - radius;
                    }
                }
                double[,] rhi = plus(getMatrixWithSquaredElements(plus(getMatrixAbs(X), 0.5)),
                    getMatrixWithSquaredElements(plus(getMatrixAbs(Y), 0.5)));
                answer = new Matrix2D(getMatrixWithLessOrEqualElements10(rhi, rsq));
                double[] xx = linspace(0.5, radius - 0.5, radius);
                double[] ii = arrayWithExtratedRootFromAbsElements(scalarMinusArray(rsq, getArrayWithSquaredElements(xx))); // intersection points for sqrt (r^2 - x^2)
                                                                                                                            // Set the values at the axis caps
                double tmp = Math.Sqrt(rsq - 0.25);
                double rint = (0.5 * tmp + rsq * Math.Atan(0.5 / tmp)) / 2; // value of integral on the right
                double cap = 2 * rint - radius + 0.5; // at the caps, lint = rint
                answer[ax, ax + radius] = cap;
                answer[ax, ax - radius] = cap;
                answer[ax + radius, ax] = cap;
                answer[ax - radius, ax] = cap;
                if (radius == 1)
                {
                    double y = ii[0];
                    double lint = rint;
                    tmp = Math.Sqrt(rsq - y * y);
                    rint = (y * tmp + rsq * Math.Atan(y / tmp)) / 2;
                    double val = rint - lint - 0.5 * (y - 0.5);
                    answer[ax - radius, ax - radius] = val;
                    answer[ax + radius, ax - radius] = val;
                    answer[ax - radius, ax + radius] = val;
                    answer[ax + radius, ax + radius] = val;
                }
                else
                {
                    // Set the values elsewhere on the rim
                    int idx = 0; // index in the vector ii
                    double x = 0.5; // bottom left corner of the current square
                    double y = radius - 0.5;
                    double rx = 0.5; // x on the right of the integrable region
                    bool ybreak = false; // did we change our y last time
                    do
                    {
                        double i = x + 0.5;
                        double j = y + 0.5;
                        double lint = rint;
                        double lx = rx;
                        double val = 0;
                        if (ybreak)
                        {
                            ybreak = false;
                            val = lx - x;
                            idx++;
                            x++;
                            rx = x;
                            val -= y * (x - lx);
                        }
                        else if (ii[idx + 1] < y)
                        {
                            ybreak = true;
                            y--;
                            rx = ii[(int)(y + 0.5)];
                            val = (y + 1) * (x - rx);
                        }
                        else
                        {
                            val = -y;
                            idx++;
                            x++;
                            rx = x;
                            if (Math.Floor(ii[idx] - 0.5) == y)
                                y++;
                        }
                        tmp = Math.Sqrt(rsq - rx * rx);
                        rint = (rx * tmp + rsq * Math.Atan(rx / tmp)) / 2;
                        val += rint - lint;
                        int axplusi = (int)(ax + i);
                        int axminusi = (int)(ax - i);
                        int axplusj = (int)(ax + j);
                        int axminusj = (int)(ax - j);
                        answer[axplusi, axplusj] = val;
                        answer[axplusi, axminusj] = val;
                        answer[axminusi, axplusj] = val;
                        answer[axminusi, axminusj] = val;
                        answer[axplusj, axplusi] = val;
                        answer[axplusj, axminusi] = val;
                        answer[axminusj, axplusi] = val;
                        answer[axminusj, axminusi] = val;
                    }
                    while ((y >= corner) && (x <= corner));


                }
                // Normalize
                answer /= (Math.PI * rsq);
            }
            else if (type == "gaussian") // %   H = FSPECIAL('gaussian',HSIZE,SIGMA) returns a rotationally
                                         // % symmetric Gaussian lowpass filter of size HSIZE with standard
                                         // % deviation SIGMA(positive).HSIZE can be a vector specifying the
                                         // % number of rows and columns in H or a scalar, in which case H is a
                                         // % square matrix.Not recommended. Use imgaussfilt or imgaussfilt3
                                         // % instead.
                                         // % The default HSIZE is [3 3], the default SIGMA is 0.5.
            {
                int defaultSize = 2 * (int)Math.Ceiling(2 * sigma) + 1;
                if (hSize == null)
                    hSize = new int[] { defaultSize, defaultSize };
                double[] siz = new double[] { (hSize[0] - 1) / 2d, (hSize[1] - 1) / 2d };
                double std = sigma;
                double[] x = createMinusPlusArray(siz[2]);
                double[] y = createMinusPlusArray(siz[1]);
                var xy = meshgrid(x, y);
                Matrix2D xOnx = (new Matrix2D(xy.x)) ^ (new Matrix2D(xy.x));
                Matrix2D yOny = (new Matrix2D(xy.y)) ^ (new Matrix2D(xy.y));
                Matrix2D arg = (-xOnx - yOny) / (2d * std * std);
                Matrix2D h = exp(arg);
                double max = h.GetMaxValue();
                for (int i = 0; i < h.Rows; i++)
                    for (int j = 0; j < h.Columns; j++)
                        if (h[i, j] < max * eps) // const eps = 2.2204e-16
                            h[i, j] = 0;
                double sumh = h.GetSum();
                if (sumh != 0)
                    h /= sumh;
                answer = h;
            }
            return answer;
        }

        public static double[] createMinusPlusArray(double number)
        {
            double[] answer = new double[(int)(number * 2) + 1];
            for (int i = 0; i <= number * 2; i++)
                answer[i] = i - number;
            return answer;
        }

        public static double[] scalarMinusArray(double scalar, double[] array)
        {
            int size = array.Length;
            double[] result = new double[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = scalar - array[i];
            }
            return result;
        }
        public static double[] arrayWithExtratedRootFromAbsElements(double[] array)
        {
            int size = array.Length;
            double[] result = new double[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = Math.Sqrt(Math.Abs(array[i]));
            }
            return result;
        }

        public static Matrix2D conv2(Matrix2D data, Matrix2D kernel, string mode = "same") // mode: only "full" and "same" options
        {
            int dataMsize = data.Rows;
            int dataNsize = data.Columns;
            int kernelMsize = kernel.Rows;
            int kernelNsize = kernel.Columns;
            Matrix2D answer = new Matrix2D(dataMsize, dataNsize); // fills with zeros by default

            if (mode == "full")
            {
                answer = new Matrix2D(dataMsize + kernelMsize - 1,
                dataNsize + kernelNsize - 1);
                for (int i = 0; i < dataMsize + kernelMsize - 1; i++)
                {
                    for (int j = 0; j < dataNsize + kernelNsize - 1; j++)
                    {
                        for (int m = 0; m < dataMsize; m++)
                        {
                            for (int n = 0; n < dataNsize; n++)
                            {
                                if ((i >= m) && (j >= n) && (i - m < kernelMsize) && (j - n < kernelNsize))
                                    answer[i, j] += data[m, n] * kernel[i - m, j - n];
                            }
                        }

                    }
                }
            }
            else if (mode == "same") // imfilter(E,h) = conv2(E,h,'same')
            { // i and j loops are answer matrix indexes, m and n are kernel matrix indexes
                for (int i = 0; i < dataMsize; i++)
                {
                    for (int j = 0; j < dataNsize; j++)
                    {
                        for (int m = 0; m < kernelMsize; m++)
                        {
                            for (int n = 0; n < kernelNsize; n++)
                            {
                                // indexI and indexJ are for data
                                int indexI = i + m - (kernelMsize - 1) / 2;
                                int indexJ = j + n - (kernelNsize - 1) / 2;

                                if ((indexI >= 0) && (indexJ >= 0) && (indexJ < dataNsize) && (indexI < dataMsize))
                                    answer[i, j] += data[indexI, indexJ] * kernel[m, n];
                            }
                        }

                    }
                }

            }
            else if (mode == "replicate") // imfilter(I, H, 'replicate')
            { // rows of the output matrix is the same as rows of I
                for (int i = 0; i < dataMsize; i++) // i and j are for answer
                {
                    for (int j = 0; j < dataNsize; j++)
                    {
                        for (int m = 0; m < kernelMsize; m++) // m and n are for kernel
                        {
                            for (int n = 0; n < kernelNsize; n++)
                            {
                                // indexI and indexJ are for data
                                int indexI = i + m - (kernelMsize - 1) / 2;
                                int indexJ = j + n - (kernelNsize - 1) / 2;

                                // replicate the numbers outside the data but inside the kernel
                                if (indexI < 0)
                                    indexI = 0;
                                else if (indexI >= dataMsize)
                                    indexI = dataMsize - 1;
                                if (indexJ < 0)
                                    indexJ = 0;
                                else if (indexJ >= dataNsize)
                                    indexJ = dataNsize - 1;

                                // count answer
                                answer[i, j] += data[indexI, indexJ] * kernel[m, n];

                            }
                        }

                    }
                }
            }
            return answer;
        }

        public static Matrix3D conv2(Matrix3D rgb, Matrix2D kernel, string mode = "same") // mode: only "full" and "same" options
        {
            int dataMsize = rgb[0].Rows;
            int dataNsize = rgb[0].Columns;
            int dataKsize = rgb.Layers;
            int kernelMsize = kernel.Rows;
            int kernelNsize = kernel.Columns;
            Matrix3D answer = new Matrix3D(dataMsize, dataNsize, dataKsize); // fills with zeros by default

            if (mode == "full")
            {
                answer = new Matrix3D(dataMsize + kernelMsize - 1, dataNsize + kernelNsize - 1, dataKsize);

                for (int k = 0; k < dataKsize; k++) // go through R,G,B -layers
                {
                    for (int i = 0; i < dataMsize + kernelMsize - 1; i++)
                    {
                        for (int j = 0; j < dataNsize + kernelNsize - 1; j++)
                        {
                            for (int m = 0; m < dataMsize; m++)
                            {
                                for (int n = 0; n < dataNsize; n++)
                                {
                                    if ((i >= m) && (j >= n) && (i - m < kernelMsize) && (j - n < kernelNsize))
                                        answer[i, j, k] += rgb[m, n, k] * kernel[i - m, j - n];
                                }
                            }

                        }
                    }
                }
            }
            else if (mode == "same") // imfilter(E,h) = conv2(E,h,'same')
            { // i and j loops are answer matrix indexes, m and n are kernel matrix indexes
                for (int k = 0; k < dataKsize; k++)
                {
                    for (int i = 0; i < dataMsize; i++)
                    {
                        for (int j = 0; j < dataNsize; j++)
                        {
                            for (int m = 0; m < kernelMsize; m++)
                            {
                                for (int n = 0; n < kernelNsize; n++)
                                {
                                    // indexI and indexJ are for data
                                    int indexI = i + m - (kernelMsize - 1) / 2;
                                    int indexJ = j + n - (kernelNsize - 1) / 2;

                                    if ((indexI >= 0) && (indexJ >= 0) && (indexJ < dataNsize) && (indexI < dataMsize))
                                        answer[i, j, k] += rgb[indexI, indexJ, k] * kernel[m, n];
                                }
                            }

                        }
                    }
                }

            }
            else if (mode == "replicate") // imfilter(I, H, 'replicate')
            { // rows of the output matrix is the same as rows of I
                for (int k = 0; k < dataKsize; k++)
                {
                    for (int i = 0; i < dataMsize; i++) // i and j are for answer
                    {
                        for (int j = 0; j < dataNsize; j++)
                        {
                            for (int m = 0; m < kernelMsize; m++) // m and n are for kernel
                            {
                                for (int n = 0; n < kernelNsize; n++)
                                {
                                    // indexI and indexJ are for data
                                    int indexI = i + m - (kernelMsize - 1) / 2;
                                    int indexJ = j + n - (kernelNsize - 1) / 2;

                                    // replicate the numbers outside the data but inside the kernel
                                    if (indexI < 0)
                                        indexI = 0;
                                    else if (indexI >= dataMsize)
                                        indexI = dataMsize - 1;
                                    if (indexJ < 0)
                                        indexJ = 0;
                                    else if (indexJ >= dataNsize)
                                        indexJ = dataNsize - 1;

                                    // count answer
                                    answer[i, j, k] += rgb[indexI, indexJ, k] * kernel[m, n];

                                }
                            }

                        }
                    }
                }
            }
            return answer;
        }

        public static Matrix2D conv2(Matrix2D H1, Matrix2D H2, Matrix2D A, string mode = "same")
        {
            Matrix2D column = new Matrix2D(H1.Columns * H1.Rows, 1);
            Matrix2D row = new Matrix2D(1, H2.Columns * H2.Rows);
            for (int i = 0; i < H1.Columns; i++)
            {
                for (int j = 0; j < H1.Rows; j++)
                {
                    column[i * H1.Rows + j] = H1[i, j];
                }
            }
            for (int i = 0; i < H2.Columns; i++)
            {
                for (int j = 0; j < H2.Rows; j++)
                {
                    row[i * H2.Rows + j] = H2[i, j];
                }
            }
            return conv2(column * row, A, mode);
        }
        public static double[,] getMatrixAbs(double[,] m1)
        {
            int sizeM = m1.GetLength(0);
            int sizeN = m1.GetLength(1);
            double[,] matrixcp = new double[sizeM, sizeN];
            for (int i = 0; i < sizeM; i++)
            {
                for (int j = 0; j < sizeN; j++)
                {
                    matrixcp[i, j] = Math.Abs(m1[i, j]);
                }
            }
            return matrixcp;
        }
        public static double[,] plus(double[,] m1, double m2)
        {
            int sizeM = m1.GetLength(0);
            int sizeN = m1.GetLength(1);
            double[,] matrixcp = new double[sizeM, sizeN];
            for (int i = 0; i < sizeM; i++)
            {
                for (int j = 0; j < sizeN; j++)
                {
                    matrixcp[i, j] = m1[i, j] + m2;
                }
            }
            return matrixcp;
        }
        public static double[,] plus(double[,] m1, double[,] m2)
        {
            int sizeM = m1.GetLength(0);
            int sizeN = m1.GetLength(1);
            double[,] matrixcp = new double[sizeM, sizeN];
            if ((sizeM == m2.GetLength(0)) && (sizeN == m2.GetLength(1)))
            {
                for (int i = 0; i < sizeM; i++)
                {
                    for (int j = 0; j < sizeN; j++)
                    {
                        matrixcp[i, j] = m1[i, j] + m2[i, j];
                    }
                }
            }
            return matrixcp;
        }
        public static double[] getArrayWithSquaredElements(double[] matrix)
        {
            int size = matrix.Length;
            double[] matrixcp = new double[size];
            for (int i = 0; i < size; i++)
            {
                matrixcp[i] = matrix[i] * matrix[i];
            }
            return matrixcp;
        }
        public static double[,] getMatrixWithSquaredElements(double[,] matrix)
        {
            double[,] matrixcp = new double[matrix.GetLength(0), matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrixcp[i, j] = matrix[i, j] * matrix[i, j];
                }
            }
            return matrixcp;
        }
        /**
         * returns matrix where element is 1 if element of matrix is <= scalar 
         * and returns zero otherwise
         * (<= 0 in matlab)
        **/
        public static double[,] getMatrixWithLessOrEqualElements10(double[,] m1, double scalar)
        {
            int sizeM = m1.GetLength(0);
            int sizeN = m1.GetLength(1);
            double[,] matrixcp = new double[sizeM, sizeN];
            for (int i = 0; i < sizeM; i++)
            {
                for (int j = 0; j < sizeN; j++)
                {
                    if (m1[i, j] <= scalar)
                        matrixcp[i, j] = 1;
                    else
                        matrixcp[i, j] = 0;
                }
            }
            return matrixcp;
        }

        /**
         * returns matrix where element is 0 if it is not equals zero and element is 1 if equals
         * (== 0 in matlab)
        **/
        public static Matrix2D getMatrixWithElementsEqualsToZero10(Matrix2D matrix)
        {
            Matrix2D matrixcp = new Matrix2D(matrix.Rows, matrix.Columns);
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Columns; j++)
                {
                    if (matrix[i, j] == 0)
                        matrixcp[i, j] = 1;
                    else
                        matrixcp[i, j] = 0;
                }
            }
            return matrixcp;
        }
        public static Matrix2D matrixToMatrixDegrees(Matrix2D data, Matrix2D degrees)
        {
            int rows = data.Rows;
            int columns = data.Columns;
            if (rows != degrees.Rows || columns != degrees.Columns)
                throw new ArgumentException("matrix dimensions should match!");
            Matrix2D answer = new Matrix2D(rows, columns);
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                    answer[i, j] = Math.Pow(data[i, j], degrees[i, j]);
            return answer;
        }

        public static double[] matrixToMatrixDegrees(double[] data, double[] degrees)
        {
            int size = data.Length;
            if (size != degrees.Length)
                throw new ArgumentException("matrix dimensions should match!");
            double[] answer = new double[size];
            for (int i = 0; i < size; i++)
                answer[i] = Math.Pow(data[i], degrees[i]);
            return answer;
        }
        public static double[] getMeanColor(Matrix3D InitialImage, int pX, int pY, double bs2, double bsQuad, int m, int n) // will return [R, G, B] - one pixel
        {
            int xo = pX + 1;
            int yo = pY + 1;
            double sumR = 0;
            double sumG = 0;
            double sumB = 0;
            int ncol = 0;
            // в квадрате +/ -bs2
            // тут почти что просто скалдываются все числа в каждом из слоев
            for (double Xl = Math.Round(xo - bs2); Xl < Math.Round(xo + bs2); Xl++)
            {
                for (double Yl = Math.Round(yo - bs2); Yl < Math.Round(yo + bs2); Yl++)
                {
                    if (Xl > 0 && Xl < m && Yl > 0 && Yl < n) // если квадрат не вылез за рамки холста, было <= m, <= n
                    {
                        if ((Xl - xo) * (Xl - xo) + (Yl - yo) * (Yl - yo) < bsQuad) // если расстояние до точки удовлетворяет уравнению круга
                        {
                            sumR += InitialImage[(int)Xl, (int)Yl, 0];
                            sumG += InitialImage[(int)Xl, (int)Yl, 1];
                            sumB += InitialImage[(int)Xl, (int)Yl, 2];
                            ncol++;
                        }
                    }
                }
            }
            // average colour of region count
            sumR /= ncol;
            sumG /= ncol;
            sumB /= ncol;

            return new double[] { sumR, sumG, sumB };
        }

        public static (double[,] x, double[,] y) meshgrid(double[] x, double[] y)
        {
            double[,] xx = new double[0, 0];
            double[,] yy = new double[0, 0];
            if (x.Length != 0 && y.Length != 0)
            {
                xx = repmat(x, y.Length, y.Length);
                yy = repmat(y, x.Length, x.Length);
            }
            return (xx, yy);
        }

        public static Matrix2D exp(Matrix2D matrix)
        {
            Matrix2D answer = new Matrix2D(matrix);
            for (int i = 0; i < matrix.Rows; i++)
                for (int j = 0; j < matrix.Columns; j++)
                    answer[i, j] = Math.Exp(answer[i, j]);
            return answer;
        }
    }
}