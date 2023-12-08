using GeneralComponents;
using System;

namespace Algorithm.Functions
{
    internal static class Filtration
    {
        /// <summary>
        /// The method creates an filter matrix of the data radius and type ("disk" or "gaussian")
        /// (hSize and Sigma are parameters fot type "gaussian", hSize is the size
        /// of the filter and Sigma - is a standart deviation)
        /// </summary>
        /// <param name="radius"> Radius of the filter </param>
        /// <param name="type"> The type of the filter: "disk" or "gaussian" </param>
        /// <param name="hSize"> Width of the image </param>
        /// <param name="sigma"> Canvas width in the millimeters </param>
        /// <returns> A filter matrix of data radius, type and other parameters </returns>
        public static Matrix2D Fspecial(int radius, string type = "disk", int[] hSize = null, double sigma = 0.5)
        {
            Matrix2D answer = new Matrix2D(new double[,] { { 1 } });

            if (type == "disk") // % H = FSPECIAL('disk',RADIUS) returns a circular averaging filter
                                // % (pillbox)within the square matrix of side 2 * RADIUS + 1.
                                // % The default RADIUS is 5.
            {
                if (radius < 0)
                    throw new ArgumentException("Fspecial: RADIUS for disk must be a real scalar");
                else if (radius == 0)
                    return answer;
                int ax = radius; // index of the "X-axis" and "Y-axis"
                double corner = Math.Floor(radius / Math.Sqrt(2) + 0.5) - 0.5; // corner corresponding to 45 degrees
                double rsq = radius * radius;
                // First set values for Points completely covered by the disk
                int dimension = 2 * radius + 1;
                Matrix2D X = new Matrix2D(dimension, dimension);
                Matrix2D Y = new Matrix2D(dimension, dimension);
                for (int i = 0; i < dimension; i++)
                {
                    for (int j = 0; j < dimension; j++)
                    {
                        X[i, j] = j - radius;
                        Y[i, j] = i - radius;
                    }
                }
                Matrix2D rhi = (X.Abs() + 0.5).Square() + (Y.Abs() + 0.5).Square();
                answer = rhi.IsLessThanScalar(rsq);
                Vector xx = Vector.linspace(0.5, radius - 0.5, radius);
                Vector ii = (rsq - xx.Square()).SqrtFromAbs(); // intersection Points for sqrt (r^2 - X^2)
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
                    double rx = 0.5; // X on the right of the integrable region
                    bool ybreak = false; // did we change our Y last time
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
            else if (type == "gaussian") // % H = FSPECIAL('gaussian',HSIZE,SIGMA) returns a rotationally
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
                Vector x = Vector.CreateInMinusPlusRange(siz[1]);
                Vector y = Vector.CreateInMinusPlusRange(siz[0]);
                var xy = Matrix2D.Meshgrid(x, y);
                Matrix2D xOnx = (new Matrix2D(xy.x)) ^ (new Matrix2D(xy.x));
                Matrix2D yOny = (new Matrix2D(xy.y)) ^ (new Matrix2D(xy.y));
                Matrix2D arg = (-xOnx - yOny) / (2d * sigma * sigma);
                Matrix2D h = arg.Exp();
                double max = h.GetMaxValue();
                for (int i = 0; i < h.Rows; i++)
                    for (int j = 0; j < h.Columns; j++)
                        if (h[i, j] < max * Constants.Eps)
                            h[i, j] = 0;
                double sumh = h.GetSum();
                if (sumh != 0)
                    h /= sumh;
                answer = h;
            }
            return answer;
        }

        /// <summary>
        /// The method calculates a matrix = 2D-convolution of the data and kernel matricies.
        /// Supported modes are "same", "full" and "replicate".
        /// "same" mode will cut the result matrix to the sizes of the data matrix
        /// "full" mode will return matrix of the size 
        /// (dataMsize + kernelMsize - 1) X (dataNsize + kernelNsize - 1)
        /// where data matrix is dataMsize X dataNsize and kernel is kernelMsize X kernelNsize
        /// "replicate" mode is the same as "same" but the bounds of the matrix are taken like
        /// nearest numbers
        /// </summary>
        /// <param name="data"> Data matrix </param>
        /// <param name="kernel"> Kernel matrix </param>
        /// <param name="mode"> "same" or "full" or "replicate" mode </param>
        /// <returns> The result matrix of 2D-convolution of data and kernel matricies </returns>
        public static Matrix2D Conv2(Matrix2D data, Matrix2D kernel, string mode = "same") // mode: only "full", "same", "replicate" options
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
            else if (mode == "same") // imfilter(E,h) = Conv2(E,h,'same')
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

                                answer[i, j] += data[indexI, indexJ] * kernel[m, n];

                            }
                        }

                    }
                }
            }
            return answer;
        }

        /// <summary>
        /// The method first convolves each column of A with the vector H1,
        /// and then it convolves each row of the result with the vector H2
        /// only "full" mode 
        /// </summary>
        /// <param name="H1"> First vector </param>
        /// <param name="H2"> Second vector </param>
        /// <param name="A"> Matrix </param>
        /// <param name="mode"> Only "full" mode </param>
        /// <returns> The result matrix of 2D-convolution of data matrix and two vectors </returns>
        public static Matrix2D Conv2(Matrix2D H1, Matrix2D H2, Matrix2D A, string mode = "full")
        {
            // mc = max([ma+n1-1,ma,n1]) and nc = max([na+n2-1,na,n2]).
            int sizeM = Math.Max(A.Rows + H1.Rows - 1, Math.Max(A.Rows, H1.Rows));
            int sizeN = Math.Max(A.Columns + H2.Rows - 1, Math.Max(A.Columns, H1.Rows));
            Matrix2D answer = new Matrix2D(sizeM, sizeN);
            if (mode == "full")
            {
                Matrix2D column = new Matrix2D(H1.Columns * H1.Rows, 1);
                Matrix2D row = new Matrix2D(1, H2.Columns * H2.Rows);
                for (int i = 0; i < H1.Rows; i++)
                {
                    for (int j = 0; j < H1.Columns; j++)
                    {
                        column[i * H1.Rows + j] = H1[i, j];
                    }
                }
                for (int i = 0; i < H2.Rows; i++)
                {
                    for (int j = 0; j < H2.Columns; j++)
                    {
                        row[i * H2.Rows + j] = H2[i, j];
                    }
                }
                answer = CutToSize(Conv2(column * row, A, mode), sizeM, sizeN);
            }
            return answer;
        }

        /// <summary>
        /// The method calculates a matrix = 2D-convolution of the layers of rgb and kernel matricies.
        /// Supported modes are "same", "full" and "replicate".
        /// "same" mode will cut the result matrix to the sizes of the data matrix
        /// "full" mode will return matrix of the size 
        /// (dataMsize + kernelMsize - 1) X (dataNsize + kernelNsize - 1)
        /// where data matrix is dataMsize X dataNsize and kernel is kernelMsize X kernelNsize
        /// "replicate" mode is the same as "same" but the bounds of the matrix are taken like
        /// nearest numbers
        /// </summary>
        /// <param name="rgb"> Data matricies: one for red layer, one for green and one for blue </param>
        /// <param name="kernel"> Kernel matrix </param>
        /// <param name="mode"> Modes "same" or "full" or "replicate" </param>
        /// <returns> The result matrix of 2D-convolution of data rgb layers and kernel matricies </returns>
        public static Matrix3D Conv2(Matrix3D rgb, Matrix2D kernel, string mode = "same") // mode: only "full" and "same" options
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
            else if (mode == "same") // imfilter(E,h) = Conv2(E,h,'same')
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

                                    answer[i, j, k] += rgb[indexI, indexJ, k] * kernel[m, n];

                                }
                            }

                        }
                    }
                }
            }
            return answer;
        }

        /// <summary>
        /// Cuts data matrix to the size rows x columns
        /// </summary>
        /// <param name="matrix"> Matrix to cut </param>
        /// <param name="rows"> Rows amount in result matrix </param>
        /// <param name="columns"> Columns amount in result matrix </param>
        /// <returns> Returns the matrix of the size rows x columns </returns>
        static Matrix2D CutToSize(Matrix2D matrix, int rows, int columns)
        {
            int countRowsToCutDown = (matrix.Rows - rows) / 2;
            int countRowsToCutUp = matrix.Rows - rows - countRowsToCutDown;
            int countColumnsToCutRight = (matrix.Columns - columns) / 2;
            int countColumnsToCutLeft = matrix.Columns - columns - countColumnsToCutRight;
            Matrix2D answer = new Matrix2D(rows, columns);
            for (int i = countRowsToCutUp; i < matrix.Rows - countRowsToCutDown; i++)
                for (int j = countColumnsToCutLeft; j < matrix.Columns - countColumnsToCutRight; j++)
                    answer[i - countRowsToCutUp, j - countColumnsToCutLeft] = matrix[i, j];
            return answer;
        }
    }
}
