using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Linq;
using GeneralComponents;

namespace Algorithm
{
    public static class Functions
    {

        //public enum MIXTYPE { MY1, MY2, CY, CM }

        public static double[] GaussSolution(double[,] matrix, double[] solution)
        {
            int rowsAmount = matrix.GetLength(0);
            int columnsAmount = matrix.GetLength(1);

            if (rowsAmount != columnsAmount)
                throw new ArgumentException("Matrix should be square!");
            
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

        public static void getYcellWcell(List<List<List<double>>> colorDB, out List<List<List<double>>> Ycell, out List<List<List<double>>> Wcell)
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

        public static double EuclideanDistance(double[] first, double[] second)
        {
            double distance = 0;
            int length = first.Length;
            if (length != second.Length)
                throw new ArgumentException("dimensions of vectors should match!");
            for (int i = 0; i < length; i++)
                distance += (first[i] - second[i]) * (first[i] - second[i]);
            distance = Math.Sqrt(distance);
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

        /**
         * 
         * 
         * 
         * 
         */
        internal static void PredictProportions(out double[,] proportions, out GeneralComponents.ColorMixType[] mixTypes, out double[,] hsvNewColor, double[,] hsvColor, List<List<List<double>>> Ycell = null, List<List<List<double>>> Wcell = null, int sheetsAmount = 4)
        {
            
            int K = 22;
            double tolerance = 0.1; // tolerance of color error in rgb

            List<List<double>> Tbl = new List<List<double>>();
            List<int> Clss = new List<int>(); // один большой столбец индексов

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

                int[] range = new int[Nset];

                for (int k = 0; k < Nset; k++)
                    range[k] = i;

                Clss.AddRange(range);
            }

            //now, create classification table and class list
            int N = Tbl.Count;
            int hsvPixelsAmount = hsvColor.GetLength(0);
            hsvNewColor = hsvColor.Clone() as double[,];
            proportions = new double[hsvPixelsAmount, 3];
            mixTypes = new GeneralComponents.ColorMixType[hsvPixelsAmount];
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
                    double[] dist = new double[hsvPixelsAmount]; // distances - taken as maximum range
                    for (int n = 0; n < hsvPixelsAmount; n++)
                    {
                        dist[n] = Math.Sqrt(3); // непонятно, зачем нужна эта строчка
                        dist[n] = EuclideanDistance(new double[] { hsvColor[i, 0], hsvColor[i, 1], hsvColor[i, 2] }, slicepts[n].ToArray());
                    }
                    // I is a sorted indicies array (take first Kp elements)
                    int[] indexes = dist.Select((value, index) => new
                    {
                        value = value,
                        index = index
                    })
                        .OrderBy(pair => pair.value)  // Order by values
                        .Take(Kp)                     // take only Kp first of them
                        .Select(pair => pair.index)   // select original indicies
                        .ToArray();

                    double s = 0, v = 0;
                    for (int index = 0; index < Kp; index++)
                    {
                        s += slicepts[indexes[index]][1];
                        v += slicepts[indexes[index]][2];
                    }
                    s /= Kp;
                    v /= Kp;
                    hsvNewColor[i, 1] = s;
                    hsvNewColor[i, 2] = v;
                }

                // then, replace the current color with the accessible one
                double[] hsvColorCurrent = new double[]
                    { hsvNewColor[i, 0], hsvNewColor[i, 1], hsvNewColor[i, 2] };
                // make prediction from the closest point
                double[] dst = new double[N]; // distances - taken as maximum range
                for (int n = 0; n < N; n++)
                {
                    dst[n] = Math.Sqrt(3); // непонятно, зачем нужна эта строчка
                    dst[n] = EuclideanDistance(hsvColorCurrent, Tbl[n].ToArray());
                }
                // I is a sorted indicies array (take first Kp elements)
                int[] I = dst.Select((value, index) => new
                {
                    value = value,
                    index = index
                })
                    .OrderBy(pair => pair.value)  // Order by values
                    .Select(pair => pair.index)   // select original indicies
                    .ToArray();
                int clsvect = Clss[I[i]];
                mixTypes[i] = (GeneralComponents.ColorMixType)clsvect;
                // then, find second possible class
                bool flag = true; // 1
                int ctr = 1; // index
                GeneralComponents.ColorMixType class2 = mixTypes[i];
                while (flag) 
                {
                    if ((GeneralComponents.ColorMixType)Clss[I[ctr]] != mixTypes[i])
                    {
                        flag = false;
                        class2 = (GeneralComponents.ColorMixType)Clss[I[ctr]]; // this class will be tested if cls(i) is wrong
                    }
                    else
                        ctr++;
                }
                flag = true;
                ctr = 0;
                double[] props0 = new double[3]; // initial proportions?
                double[] propscur = new double[3]; // current proportions
                double err0 = 0;

                while (flag)
                {
                    List<List<double>> Y = Ycell[(int)mixTypes[i]]; // Y for hsv from i sheet of data table
                    List<List<double>> W = Wcell[(int)mixTypes[i]]; // W for proportions from i sheet
                    int NY = Y.Count; // amount of rows in Y
                    double[] dist = new double[NY]; // distances - taken as maximum range
                    for (int n = 0; n < NY; n++)
                    {
                        dist[n] = Math.Sqrt(3); // непонятно, зачем нужна эта строчка
                        dist[n] = EuclideanDistance(hsvColorCurrent, Y[n].ToArray());
                    }

                    int[] indexes = dist.Select((value, index) => new
                    {
                        value = value,
                        index = index
                    })
                    .OrderBy(pair => pair.value)  // Order by values
                    .Take(K)                      // take first K elements
                    .Select(pair => pair.index)   // select original indicies
                    .ToArray();

                    for (int j = 0; j < 3; j++)
                    {

                        var X = Y.Select((value, index) => new
                        {
                            value = value,
                            index = index
                        })
                            .Where(pair => indexes.Contains(pair.index))
                            .Select(pair => pair.value.ToArray())
                            .ToArray();

                        double[,] E = new double[K, 4]; // evaluated polynomial
                        List<double[]> T = new List<double[]> { 
                            new double[] { 0, 0, 0 },
                            new double[] { 1, 0, 0 },
                            new double[] { 0, 1, 0 },
                            new double[] { 0, 0, 1 }
                        }; // monomial orders
                        double[][] h = Eye(4);
                        
                        for (int k = 0; k < 4; k++)
                        {
                            double[] product = prod2(matrixToMatrixDegrees(X, repmat(T[k], K)));
                            for (int r = 0; r < K; r++)
                            {
                                for (int c = 0; c < E.GetLength(1); c++)
                                {
                                    E[r, c] += product[r] * h[k][c]; 
                                }
                            }
                        }

                        var Wj = W.Select((value, index) => new
                        {
                            value = value,
                            index = index
                        })
                            .Where(pair => indexes.Contains(pair.index))
                            .Select(pair => pair.value.ToArray()[j])
                            .ToArray();

                        // h = (E'*E)\(E' * W(I(1:K), j)); % OLS
                        double[,] Etransposed = getTransposed(E);
                        double[] solution = array2dimsToarray1dim(
                            getMultiplied(Etransposed, arrayToArray2dimsColumn(Wj)));
                        
                        h = array1DToArray2DColumn(
                            GaussSolution(
                                getMultiplied(Etransposed, E), solution));


                        // predict proportion

                        propscur[j] = 0;
                        for (int k = 0; k < 4; k++)
                        {
                            propscur[j] += h[k][0] * prod2(matrixToMatrixDegrees(hsvColorCurrent, T[k]));
                        }
                    }

                    // get into ranges [0,1]
                    propscur = saturation(propscur);
                    // test inversion
                    double[][] hsvinv = prop2hsv(array1DToArray2DRow(propscur), mixTypes, Wcell, Ycell); // is first right?
                    double[] rgbcur = hsv2rgb(hsvColorCurrent);
                    double[] rgbinv = hsv2rgb(hsvinv[0]); // is that right? should be...

                    double err = EuclideanDistance(rgbcur, rgbinv);

                    if (ctr < 2)
                    {
                        // if the values are calculated for the first time
                        if (err > tol)
                        {
                            ctr++;
                            mixTypes[i] = class2;
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
                            mixTypes[i] = (GeneralComponents.ColorMixType)clsvect;
                        }
                        // anyway, go out of the loop
                        flag = false; // go out of the loop
                    }
                }
                for (int k = 0; k < propscur.Length; k++)
                { 
                    proportions[i, k] = propscur[k];
                    hsvArray[i, k] = hsvNewColor[i, k]; // тоже дичь бесконечная, как же все запутано с этими размерами массивов
                }
            }
            hsvNewColor = hsvArray;
        }


        internal static double[][] prop2hsv(double[][] proportions, GeneralComponents.ColorMixType[] mixTypes, List<List<List<double>>> Ycell = null, List<List<List<double>>> Wcell = null, int sheetsAmount = 4)
        {
            int K = 150;
            if ((Ycell == null) && (Wcell == null))
            {
                // считать функцией из хелпера?
            }

            // далее Ycell и Wcell гарантированно не null
            int Npts = proportions.GetLength(0);//size(props, 1);
           // double[,] hsvcol = new double[Npts, 3];

            double[][] hsvcol = new double[Npts][];
            for (int i = 0; i < Npts; i++) 
                hsvcol[i] = new double[3];

            for (int i = 0; i < Npts; i++)
            {
                List<List<double>> Y = new List<List<double>>();
                List<List<double>> W = new List<List<double>>();
                // take the corresponding sets
                if ((int)(mixTypes[i]) <= 2)
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
                    Y = Ycell[(int)mixTypes[i]]; // Y for proportions
                    W = Wcell[(int)mixTypes[i]]; // W for HSV
                }

                int NY = Y.Count;
                double[] dst = new double[NY];
                for (int k = 0; k < NY; k++)
                {
                    dst[k] = EuclideanDistance(proportions[i], Y[k].ToArray());
                }

                int[] indexes = dst.Select((value, index) => new
                {
                    value = value,
                    index = index
                })
                    .OrderBy(pair => pair.value)  // Order by values
                    .Take(K)                      // take first K elements
                    .Select(pair => pair.index)   // select original indicies
                    .ToArray();

                double[] hsvcolcur = new double[3];
                int N = 4;

                List<double[]> T = new List<double[]> {
                            new double[] { 0, 0, 0 },
                            new double[] { 1, 0, 0 },
                            new double[] { 0, 1, 0 },
                            new double[] { 0, 0, 1 }
                        }; // deglexord(0,1,3);
                // take first K points
                K = Math.Min(NY, K); // decrease K if needed

                var X = Y.Select((value, index) => new
                {
                    value = value,
                    index = index
                })
                            .Where(pair => indexes.Contains(pair.index))
                            .Select(pair => pair.value.ToArray())
                            .ToArray();

                double[,] E = new double[K, N]; // evaluated polynomial
                double[][] h = Eye(N);
                for(int k = 0; k < N; k++)
                {
                    double[] product = prod2(matrixToMatrixDegrees(X, repmat(T[k], K)));
                    for (int r = 0; r < K; r++)
                    {
                        for (int c = 0; c < E.GetLength(1); c++)
                        {
                            E[r, c] += product[r] * h[k][c];
                        }
                    }
                }

                for (int j = 0; j < 3; j++)
                {
                    var Wj = W.Select((value, index) => new
                    {
                        value = value,
                        index = index
                    })
                            .Where(pair => indexes.Contains(pair.index))
                            .Select(pair => pair.value.ToArray()[j])
                            .ToArray();
                    // h = (E'*E)\(E'*V); %OLS
                    double[,] Etransposed = getTransposed(E);
                    
                    double[] h2 = GaussSolution(getMultiplied(Etransposed, E), 
                        array2dimsToarray1dim(getMultiplied(Etransposed, arrayToArray2dimsColumn(Wj))));

                    // predict proportion
                    
                    hsvcolcur[j] = 0;
                    double[] x = proportions[i];
                    
                    for (int k = 0; k < N; k++)
                    {
                        double product = prod2(matrixToMatrixDegrees(x, T[k]));

                        for (int c = 0; c < h[k].Length; c++)
                        {
                            hsvcolcur[j] += product * h[k][c];
                        }

                    }

                    if (hsvcolcur[0] < 0)
                        hsvcolcur[0]++; // use this because models predict with shift
                    // get into ranges [0,1]
                    hsvcol[i] = saturation(hsvcolcur);
                }
            }
            return hsvcol;
        }

        public static double[] array2dimsToarray1dim(double[,] array)
        {
            int rowsAmount = array.GetLength(0);
            int columnsAmount = array.GetLength(1);
            double[] array1D = new double[rowsAmount * columnsAmount];
            for (int i = 0; i < rowsAmount; i++)
            {
                for (int j = 0; j < columnsAmount; j++)
                {
                    array1D[i * columnsAmount + j] = array[i, j];
                }
            }
            return array1D;
        }

        public static double[][] array1DToArray2DColumn(double[] array)
        {
            double[][] array2D = new double[array.Length][];
            for (int i = 0; i < array.Length; i++)
            {
                array2D[i] = new double[1];
                array2D[i][0] = array[i];
            }
            return array2D;
        }

        public static double[][] array1DToArray2DRow(double[] array)
        {
            double[][] array2D = new double[1][];
            array2D[0] = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array2D[0][i] = array[i];
            }
            return array2D;
        }

        public static double[,] getTransposed(double[,] matrix)
        {
            int rowsAmount = matrix.GetLength(0);
            int columnsAmount = matrix.GetLength(1);
            double[,] transposed = new double[columnsAmount, rowsAmount]; 
            for (int i = 0; i < rowsAmount; i++)
            {
                for (int j = 0; j < columnsAmount; j++)
                {
                    transposed[j, i] = matrix[i, j];
                }
            }
            return transposed;
        }

        /**
         * converts array [,] with two dimensions to array [][] 2D array
        */
        public static double[][] array2dimsTo2DArray(double[,] array) 
        {
            int rowsAmount = array.GetLength(0);
            int columnsAmount = array.GetLength(1);
            double[][] array2D = new double[rowsAmount][];
            for (int i = 0; i < rowsAmount; i++)
            {
                array2D[i] = new double[columnsAmount];
                for (int j = 0; j < columnsAmount; j++)
                {
                    array2D[i][j] = array[i, j]; 
                }
            }
            return array2D;
        }

        /**
         * converts array [] with to array [ , 1] column
        */
        public static double[,] arrayToArray2dimsColumn(double[] array)
        {
            double[,] column = new double[array.Length, 1];
            for (int i = 0; i < array.Length; i++)
                column[i, 0] = array[i];
            return column;
        }

        public static double[,] getMultiplied(double[,] first, double[,] second)
        {
            int rowsAmount = first.GetLength(0);
            int columnsAmount = second.GetLength(1);
            int commonSize = first.GetLength(1);
            if (commonSize != second.GetLength(0))
                throw new ArgumentException("matrix sizes didn't match to produce a multiplication");
            double[,] multiplied = new double[rowsAmount, columnsAmount];
            for (int i = 0; i < rowsAmount; i++)
            {
                for (int j = 0; j < columnsAmount; j++)
                {
                    for (int k = 0; k < commonSize; k++)
                        multiplied[i, j] += first[i, k] * second[k, j];
                }
            }
            return multiplied;
        }

        public static double[] prod2(double[][] matrix) // prod(matrix, 2) 
        {
            int length = matrix.GetLength(0);
            double[] answer = new double[length];
            for (int i = 0; i < length; i++) 
            {
                answer[i] = 1;
                for (int j = 0; j < matrix[i].GetLength(0); j++)
                {
                    answer[i] *= matrix[i][j];
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
            return new double[] { h, s, v};
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

            static int hueIsLess (double hue, double number)
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

                rgbPixel[i] += factor[i] * ( 6d * hueIsLess(hueForRgb[i], 1/6d) * hueForRgb[i] 
                    + (hueIsNotLess(hueForRgb[i], 1 / 6d) & hueIsLess(hueForRgb[i], 1 / 2d))
                    + (hueIsNotLess(hueForRgb[i], 1 / 2d) & hueIsLess(hueForRgb[i], 2 / 3d)) * (4d - 6d * hueForRgb[i]) );
            }
            return rgbPixel;
        }

        public static double mod(double dividend, double divisor)
        {
            if (divisor == 0)
                throw new ArgumentException("divisor is zero");
            return (dividend - divisor * Math.Floor(dividend / divisor));
        }

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

        public static double[] getMatrixWithMultipliedElements(double[] m1, double[] m2)
        {
            int size = m1.GetLength(0);
            double[] matrixcp = new double[size];
            for (int i = 0; i < size; i++)
            {
                matrixcp[i] = m1[i] * m2[i];
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
        public static double[][] matrixToMatrixDegrees(double[][] data, double[,] degrees)
        {
            int sizeM = data.GetLength(0);
            int sizeN = data[0].GetLength(0);
            if (sizeM != degrees.GetLength(0) || sizeN != degrees.GetLength(1))
                throw new ArgumentException("matrix dimensions should match!");
            double[][] answer = new double[sizeM] [];
            for (int i = 0;i < sizeM; i++)
            {
                answer[i] = new double[sizeN];
                for (int j = 0; j < sizeN; j++)
                {
                    answer[i][j] = Math.Pow(data[i][j], degrees[i, j]);
                }
            }
            return answer;
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
                    if (Xl > 0 && Xl < m && Yl > 0 && Yl < n) // если квадрат не вылез за рамки холста, было <= m, <= n
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