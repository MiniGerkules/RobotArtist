using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;



namespace Algorithm
{
    public static class Functions
    {

        public enum MIXTYPE { MY1, MY2, CY, CM }

        public static void getYcellWcell(List<List<List<double>>> colorDB,out List<List<List<double>>> Ycell, out List<List<List<double>>> Wcell)
        {
            // in colorDB [i][j][k] double-element where i is sheet, j is row on this sheet, k is column on this sheet
            Ycell = new(colorDB.Count);
            Wcell = new(colorDB.Count);

            for (int i = 0; i < colorDB.Count; i++)
            {
                Ycell.Add(new(colorDB[i].Count));
                Wcell.Add(new(colorDB[i].Count));

                for (int j = 0; j < colorDB[i].Count; j++)
                {
                    if (colorDB[i][j].Count != 6)
                        throw new ArgumentException("amount of columns on every sheet should equals 6!");

                    Ycell[i].Add(new(colorDB[i][j].Count));
                    Wcell[i].Add(new(colorDB[i][j].Count));

                    for (int k = 0; k < colorDB[i][j].Count; k++)
                    {
                        if (k < 3)
                            Ycell[i][j].Add(colorDB[i][j][k]);
                        else
                            Wcell[i][j].Add(colorDB[i][j][k]);
                    }
                }
            }
        }

        /**
         * 
         * 
         * 
         * 
         */
        public static void PredictProportions(out double[,] proportions, out MIXTYPE[] mixTypes, out double[,] hsvNewColor, double[,] hsvColor, List<List<List<double>>> Ycell = null, List<List<List<double>>> Wcell = null, int sheetsAmount = 4) 
        {
            static double[] saturation(double[] prop) 
            { 
                double[] val = new double[prop.Length];
                for(int i = 0; i < prop.Length; i++)
                {
                    val[i] = Math.Min(Math.Max(prop[i], 0), 1);
                }
                return val;
            };
            int K = 22;
            double tolerance = 0.1; // tolerance of color error in rgb

            List<List<double>> Tbl = new List<List<double>>();
            List<double> Clss = new List<double>(); // один большой столбец

            if ((Ycell == null) && (Wcell == null))
            {
                // считать функцией из хелпера?
            }

            // далее Ycell и Wcell гарантированно не null

            for (int i = 0; i < sheetsAmount; i++)
            {
                int Nset = Ycell[i].Count;

                for (int j = 0; j < sheetsAmount; j++)
                    Tbl.Add(Ycell[i][j]);

                double[] range = new double[Nset];

                for (int k = 0; k < Nset; k++)
                    range[k] = i;

                Clss.AddRange(range);
            }

            //now, create classification table and class list
            int N = Tbl.Count;
            int hsvPixelsAmount = hsvColor.GetLength(0);
            hsvNewColor = new double[hsvPixelsAmount, hsvColor.GetLength(1)];
            proportions = new double[hsvPixelsAmount, 3];
            mixTypes = new MIXTYPE[hsvPixelsAmount];
            double[,] hsvArray = new double[hsvPixelsAmount, 3];
            for (int i = 0; i < hsvPixelsAmount; i++)
            {
                // first, check for consistency - if the color is outside a possible range
                double tol = 0.1; // tolerance, 1 / 2 of width of a color stripe
                double hOfColor = hsvColor[i, 1];
                List<List<double>> slicepts = new List<List<double>>();

                int Npts = 0;
                for (int k = 0; k < N; k++)
                {
                    if (Math.Abs(Tbl[k][i] - hOfColor) <= tol)
                    {
                        slicepts.Add(Tbl[k]);
                        Npts++;
                    }
                }
                // inside a slice, divide space into quadrants
                int[] quads = new int[4];
                for (int k = 0; k < hsvPixelsAmount; k++) // among points in a slice
                {
                    double sk = slicepts[k][1];
                    double vk = slicepts[k][2];
                    double s = hsvColor[i, 1];
                    double v = hsvColor[i, 2];

                    if (sk > s)
                    {
                        if (vk > v)
                            quads[0] = 1;
                        else
                            quads[3] = 1;
                    }
                    else
                    {
                        if (vk > v)
                            quads[1] = 1;
                        else
                            quads[2] = 1;
                    }
                }
                if (quads[0] + quads[1] + quads[2] + quads[3] < 4)
                {
                    // this means that interpolation is impossible
                    // take Kp closest points and select mean values
                    int Kp = 10;
                    double[] dst = new double[hsvPixelsAmount]; // distances - taken as maximum range
                    for (int n = 0; n < hsvPixelsAmount; n++)
                    {
                        dst[n] = Math.Sqrt(3); // непонятно, зачем нужна эта строчка
                        dst[n] = Math.Sqrt((hsvColor[i, 0] - slicepts[n][0]) * (hsvColor[i, 0] - slicepts[n][0]) +
                            (hsvColor[i, 1] - slicepts[n][1]) * (hsvColor[i, 1] - slicepts[n][1]) +
                            (hsvColor[i, 2] - slicepts[n][2]) * (hsvColor[i, 2] - slicepts[n][2]));
                        // Euclidean distance in (H,S,V) space

                        // 103 строка, пока не ясно, что именно делает это sort

                    }

                }

            }
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
         * gets a rgb-pixel and return a hsv-pixel
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
            return new double[] { h, s, v};
        }

        public static double[,] zeros(int Msize, int Nsize)
        {
            return new double[Msize, Nsize];
        }

        //public static double[,] rgb2hsv(double[,] pixel) 
        //{
        //    double[,] hsvArray = new double[pixel.GetLength(0), pixel.GetLength(1)];
        //    for (int i = 0; i < pixel.GetLength(0); i++)
        //    {

        //    }
        //}

        public static double[,] fspecial(int radius, string type = "disk")
        {
            double[,] answer = { { 1 } };
            if (type == "disk")
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
                answer = new double[dimension, dimension];
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
                answer = getMatrixWithLessOrEqualElements10(rhi, rsq);
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
                answer = getMatrixMultipliedByScalar(answer, 1 / (Math.PI * rsq));
            }
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
        public static void printMatrixToConsole(double[,] matrix)
        {
            int dataMsize = matrix.GetLength(0);
            int dataNsize = matrix.GetLength(1);
            for (int m = 0; m < dataMsize; m++)
            {
                for (int n = 0; n < dataNsize; n++)
                {
                    Console.Write(matrix[m, n] + " ");
                }
                Console.WriteLine();
            }
        }
        public static double[,] conv2(double[,] data, double[,] kernel, string mode = "same") // mode: only "full" and "same" options
        {
            int dataMsize = data.GetLength(0);
            int dataNsize = data.GetLength(1);
            int kernelMsize = kernel.GetLength(0);
            int kernelNsize = kernel.GetLength(1);
            double[,] answer = new double[dataMsize, dataNsize]; // fills with zeros by default


            if (mode == "full")
            {
                answer = new double[dataMsize + kernelMsize - 1,
                dataNsize + kernelNsize - 1];
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
            { // size of the output matrix is the same as size of I
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

        public static RGBLayers conv2(RGBLayers rgb, double[,] kernel, string mode = "same") // mode: only "full" and "same" options
        {
            double[,,] data = rgb.layers;
            int dataMsize = data.GetLength(0);
            int dataNsize = data.GetLength(1);
            int dataKsize = data.GetLength(2);
            int kernelMsize = kernel.GetLength(0);
            int kernelNsize = kernel.GetLength(1);
            double[,,] answer = new double[dataMsize, dataNsize, dataKsize]; // fills with zeros by default

            if (mode == "full")
            {
                answer = new double[dataMsize + kernelMsize - 1,
                dataNsize + kernelNsize - 1, dataKsize];
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
                                        answer[i, j, k] += data[m, n, k] * kernel[i - m, j - n];
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
                                        answer[i, j, k] += data[indexI, indexJ, k] * kernel[m, n];
                                }
                            }

                        }
                    }
                }

            }
            else if (mode == "replicate") // imfilter(I, H, 'replicate')
            { // size of the output matrix is the same as size of I
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
                                    answer[i, j, k] += data[indexI, indexJ, k] * kernel[m, n];

                                }
                            }

                        }
                    }
                }
            }
            //byte[,,] pixes = answer.Clone() as byte[,,];
            return new RGBLayers(data.GetLength(2), answer);
        }

        public static double[,] minus(double[,] matrix)
        {
            double[,] matrixcp = new double[matrix.GetLength(0), matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrixcp[i, j] = -matrix[i, j];
                }
            }
            return matrixcp;
        }

        public static double[,] minus(double[,] m1, double[,] m2)
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
                        matrixcp[i, j] = m1[i, j] - m2[i, j];
                    }
                }
            }
            return matrixcp;
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

        public static double[,] plus(double[,] m1, double[,] m2, double[,] m3)
        {
            int sizeM = m1.GetLength(0);
            int sizeN = m1.GetLength(1);
            double[,] matrixcp = new double[sizeM, sizeN];
            if ((sizeM == m2.GetLength(0)) && (sizeN == m2.GetLength(1))
                && (sizeM == m3.GetLength(0)) && (sizeN == m3.GetLength(1)))
            {
                for (int i = 0; i < sizeM; i++)
                {
                    for (int j = 0; j < sizeN; j++)
                    {
                        matrixcp[i, j] = m1[i, j] + m2[i, j] + m3[i, j];
                    }
                }
            }
            return matrixcp;
        }

        public static double[,] getMatrixMultipliedByScalar(double[,] matrix, double scalar)
        {
            double[,] matrixcp = new double[matrix.GetLength(0), matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrixcp[i, j] = matrix[i, j] * scalar;
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

        public static double[,] getMatrixWithRootExtractedElements(double[,] matrix)
        {
            double[,] matrixcp = new double[matrix.GetLength(0), matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrixcp[i, j] = Math.Sqrt(matrix[i, j]);
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
        public static double[,] getMatrixWithElementsEqualsToZero10(double[,] matrix)
        {
            double[,] matrixcp = new double[matrix.GetLength(0), matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] == 0)
                        matrixcp[i, j] = 1;
                    else
                        matrixcp[i, j] = 0;
                }
            }
            return matrixcp;
        }

        /**
         * returns matrix where element is 1 if it is not equals zero and element is 0 if equals
         * (~= 0 in matlab)
        **/
        public static double[,] getMatrixWithElementsNotEqualsToZero10(double[,] matrix)
        {
            double[,] matrixcp = new double[matrix.GetLength(0), matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] == 0)
                        matrixcp[i, j] = 0;
                    else
                        matrixcp[i, j] = 1;
                }
            }
            return matrixcp;
        }

        public static double[,] getMatrixWithMultipliedElements(double[,] m1, double[,] m2)
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
                        matrixcp[i, j] = m1[i, j] * m2[i, j];
                    }
                }
            }
            return matrixcp;
        }

        public static double[,] getMatrixWithDividedElements(double[,] m1, double[,] m2)
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
                        if (m2[i, j] != 0)
                            matrixcp[i, j] = m1[i, j] / m2[i, j];
                        else
                            matrixcp[i, j] = double.NaN; // is that ok?
                    }
                }
            }
            return matrixcp;
        }

        public static double[,] getSquareMatrixOfSize_FilledBy_(int size, double scalar)
        {
            double[,] matrixcp = new double[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    matrixcp[i, j] = scalar;
                }
            }
            return matrixcp;
        }

        public static double[] getMeanColor(RGBLayers InitialImage, int pX, int pY, double bs2, double bsQuad, int m, int n) // will return [R, G, B] - one pixel
        {
            int xo = pX; 
            int yo = pY;
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
                    if (Xl > 0 && Xl <= m && Yl > 0 && Yl <= n) // если квадрат не вылез за рамки холста
                    {
                        if ((Xl - xo) * (Xl - xo) + (Yl - yo) * (Yl - yo) < bsQuad) // если расстояние до точки удовлетворяет уравнению круга
                        {
                            sumR += InitialImage.layers[(int)Xl, (int)Yl, 0];
                            sumG += InitialImage.layers[(int)Xl, (int)Yl, 1];
                            sumB += InitialImage.layers[(int)Xl, (int)Yl, 2];
                            ncol++;
                        }
                    }
                }
            }
            // вычислим цвет средний по области
            sumR /= ncol;
            sumG /= ncol;
            sumB /= ncol;

            return new double[] { sumR, sumG, sumB };
        }
    }
}