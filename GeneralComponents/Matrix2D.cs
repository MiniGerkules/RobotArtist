using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace GeneralComponents {
    public class Matrix2D {
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        private readonly Vector[] matrix;

        public double this[int row, int column] {
            get {
                if (matrix[0].IsRow)
                    return matrix[row][column];
                else
                    return matrix[column][row];
            }
            set {
                if (matrix[0].IsRow)
                    matrix[row][column] = value;
                else
                    matrix[column][row] = value;
            }
        }

        public double this[int index] {
            get {
                if (Rows == 1)
                    return matrix[0][index];
                else if (Columns == 1)
                    return matrix[index][0];
                else
                    throw new IndexOutOfRangeException("Index out of range!");
            }

            set {
                if (Rows == 1)
                    matrix[0][index] = value;
                else if (Columns == 1)
                    matrix[index][0] = value;
                else
                    throw new IndexOutOfRangeException("Index out of range!");
            }
        }

        public Matrix2D(int rows, int columns) {
            if (rows == 0 || columns == 0)
                throw new ArgumentException("Can't create an empty matrix!");

            Rows = rows;
            Columns = columns;
            matrix = new Vector[rows];
            for (int i = 0; i < rows; ++i)
                matrix[i] = new(columns);
        }

        public Matrix2D(int rows, int columns, double initVal) : this(rows, columns) {
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                    this[i, j] = initVal;
        }

        public Matrix2D(List<double> vector) : this(vector.ToImmutableList()) { }

        public Matrix2D(ImmutableList<double> vector) {
            if (vector.Count == 0)
                throw new ArgumentException("Pass a empty list to construct " +
                    "a new matrix!");

            Rows = 1;
            Columns = vector.Count;
            matrix = new Vector[] { new(vector) };
        }

        public Matrix2D(Vector vector) {
            if (vector.IsRow) {
                Rows = 1;
                Columns = vector.Size;
            } else {
                Rows = vector.Size;
                Columns = 1;
            }

            matrix = new Vector[] { new(vector) };
        }

        public Matrix2D(List<List<double>> matrix) : this(matrix.ToImmutable()) { }

        public Matrix2D(ImmutableList<ImmutableList<double>> matrix) {
            if (matrix.Count == 0 || matrix[0].Count == 0)
                throw new ArgumentException("Pass a empty list to construct " +
                    "a new matrix!");

            Rows = matrix.Count;
            Columns = matrix[0].Count;
            this.matrix = new Vector[Rows];

            for (int i = 0; i < Rows; ++i)
                this.matrix[i] = new(matrix[i]);
        }

        public Matrix2D(Matrix2D matrix) {
            Rows = matrix.Rows;
            Columns = matrix.Columns;
            this.matrix = new Vector[Rows];

            for (int i = 0; i < Rows; ++i)
                this.matrix[i] = new(matrix.matrix[i]);
        }

        public Matrix2D(double[,] matrix2)
        {
            Rows = matrix2.GetLength(0);
            Columns = matrix2.GetLength(1);
            this.matrix = new Vector[Rows];
            for (int i = 0; i < Rows; ++i)
            {
                this.matrix[i] = new Vector(Columns);
                for (int j = 0; j < Columns; j++)
                    this.matrix[i][j] = matrix2[i, j];
            }
        }

        public Matrix2D(int[,] matrix2)
        {
            Rows = matrix2.GetLength(0);
            Columns = matrix2.GetLength(1);
            this.matrix = new Vector[Rows];
            for (int i = 0; i < Rows; ++i)
            {
                this.matrix[i] = new Vector(Columns);
                for (int j = 0; j < Columns; j++)
                    this.matrix[i][j] = matrix2[i, j];
            }
        }

        public static void Copy(Matrix2D first, Matrix2D second)
        {
            int amountOfRows = Math.Min(first.Rows, second.Rows);
            for (int i = 0; i < amountOfRows; i++)
                Vector.Copy(first.matrix[i], second.matrix[i]);
        }

        public static Matrix2D Eye(int dimension)
        {
            Matrix2D eye = new Matrix2D(dimension, dimension);

            for (int i = 0; i < dimension; i++)
                eye[i, i] = 1;

            return eye;
        }

        public Vector GetRow(int rowIndex) {
            double[] vector = new double[Columns];
            for (int i = 0; i < Columns; ++i)
                vector[i] = this[rowIndex, i];

            return new(vector);
        }

        public Vector GetColumn(int columnIndex) {
            double[] vector = new double[Rows];
            for (int i = 0; i < Rows; ++i)
                vector[i] = this[i, columnIndex];

            return new(vector, false);
        }

        public void SwapRows(int index1, int index2) {
            if (!(0 < index1 && index1 < Rows && 0 < index2 && index2 < Rows))
                throw new IndexOutOfRangeException("The indexes is incorrect! " +
                    "They must be positive and less then number of matrix rows!");

            for (int i = 0; i < Columns; ++i)
                (this[index1, i], this[index2, i]) = (this[index2, i], this[index1, i]);
        }

        public void SwapColumns(int index1, int index2) {
            if (!(0 < index1 && index1 < Columns && 0 < index2 && index2 < Columns))
                throw new IndexOutOfRangeException("The indexes is incorrect! " +
                    "They must be positive and less then number of matrix rows!");

            for (int i = 0; i < Rows; ++i)
                (this[i, index1], this[i, index2]) = (this[i, index2], this[i, index1]);
        }

        public Matrix2D RepeatColumns(int numOfRepeat) {
            Matrix2D result = new(Rows, Columns * numOfRepeat);

            for (int i = 0; i < numOfRepeat; ++i)
                for (int j = 0; j < Rows; ++j)
                    for (int k = 0; k < Columns; ++k)
                        result[j, i * Columns + k] = this[j, k];

            return result;
        }

        public Matrix2D RepeatRows(int numOfRepeat) {
            Matrix2D result = new(Rows * numOfRepeat, Columns);

            for (int i = 0; i < numOfRepeat; ++i)
                for (int j = 0; j < Rows; ++j)
                    for (int k = 0; k < Columns; ++k)
                        result[i * Rows + j, k] = this[j, k];

            return result;
        }

        public double GetMaxValue()
        {
            List<List<double>> listMatrix = this.ToList();
            double max = this.matrix[0][0];
            for (int i = 0; i < Rows; i++)
            {
                max = (listMatrix[i].Max() > max) ? listMatrix[i].Max() : max;
            }
            return max;
        }

        public int[] GetIndexesForSorted() {
            List<KeyValuePair<int, double>> bla = new();

            int ComparePair(KeyValuePair<int, double> first, KeyValuePair<int, double> second)
            {
                if (first.Value > second.Value)
                    return 1;
                if (first.Value < second.Value)
                    return -1;
                if (first.Value == second.Value)
                {
                    if (first.Key < second.Key)
                        return -1;
                    if (first.Key > second.Key)
                        return 1;
                    if (first.Key == second.Key)
                        return 0;
                }
                return 0;
            }

            for (int i = 0; i < Rows; ++i) {
                bla.Add(new KeyValuePair<int, double>(i, this[i, 0]));
            }
            bla.Sort(ComparePair);
            
            return bla.Select(pair => pair.Key).ToArray();
        }

        public void MakeUnit(int startRow) {
            if (startRow > Rows)
                throw new IndexOutOfRangeException("Can't make unit matrix! " +
                    "The index of the start row is greater than rows number.");

            int column = 0;
            for (int row = startRow; row < Rows && column < Columns; ++row, ++column)
                this[row, column] = 1;
        }

        public Matrix2D Transpose() {
            Matrix2D result = new(this);

            (result.Rows, result.Columns) = (result.Columns, result.Rows);
            foreach (var vector in result.matrix)
                vector.Transpose();

            return result;
        }

        public Matrix2D GetByIndexes(int[] indexes) {
            Matrix2D matrix = new(indexes.Length, Columns);

            for (int i = 0; i < indexes.Length; ++i)
                for (int j = 0; j < Columns; ++j)
                    matrix[i, j] = this[indexes[i], j];

            return matrix;
        }

        public Matrix2D MakeDiag() {
            if (!(Rows > 1 && Columns == 1 || Rows == 1 && Columns > 1))
                throw new ArgumentException("Can't make a diagonal matrix! The matrix must be a vector!");

            int dim = Math.Max(Rows, Columns);
            Matrix2D result = new(dim, dim, 0);

            for (int i = 0; i < dim; ++i)
                result[i, i] = this[i];

            return result;
        }

        public double GetSum() {
            double result = 0;

            for (int i = 0; i < Rows; ++i) 
                result += matrix[i].Sum();
            
            return result;
        }

        public Matrix2D Pow(Vector powers) {
            Matrix2D matrix = new(powers);

            if (powers.IsRow)
                matrix = matrix.RepeatRows(Rows);
            else
                matrix = matrix.RepeatColumns(Columns);

            return ByElem(this, matrix, Math.Pow);
        }

        public Matrix2D Abs()
        {
            return ByElem(this, Math.Abs);
        }

        public Matrix2D Pow(Matrix2D powers) {
            if (powers.Columns == 1 && powers.Rows == Rows) {
                powers = powers.RepeatColumns(Columns);
                return ByElem(this, powers, Math.Pow);
            } else if (powers.Rows == 1 && powers.Columns == Columns) {
                powers = powers.RepeatRows(Rows);
                return ByElem(this, powers, Math.Pow);
            } else if (powers.Columns == Columns && powers.Rows == Rows) {
                return ByElem(this, powers, Math.Pow);
            } else {
                throw new ArgumentException("The matrix dimensions isn't same!");
            }
        }

        public void FillByZeros()
        {
            for (int i = 0; i < Rows; i++)
                matrix[i].FillByZeros();
        }

        public static explicit operator double(Matrix2D matrix) {
            if (matrix.Rows != 1 || matrix.Columns != 1)
                throw new ArgumentException("Matrix dimension is not 1x1!");
            return matrix[0];
        }

        public Matrix2D Square() => DoAction(MathFunctions.Square);
        public Matrix2D Sqrt() => DoAction(MathFunctions.Sqrt);
        public Matrix2D Exp() => DoAction(MathFunctions.Exp);

        public Matrix2D DoAction(Func<double, double> action) {
            Matrix2D result = new(Rows, Columns);

            for (int i = 0; i < Rows; ++i)
                for (int j = 0; j < Columns; ++j)
                    result[i, j] = action(this[i, j]);

            return result;
        }

        ///<summary>
        /// MESHGRID   Cartesian rectangular grid in 2-D or 3-D
        /// [X, Y] = MESHGRID(x, y) returns 2-D grid coordinates based on the
        /// coordinates contained in vectors x and y. X is a matrix where each row
        /// is a copy of x, and Y is a matrix where each column is a copy of y. The
        /// grid represented by the coordinates X and Y has length(y) rows and
        /// length(x) columns.
        ///</summary>
        public static (Matrix2D x, Matrix2D y) Meshgrid(Vector x, Vector y)
        {
            Matrix2D xx = new Matrix2D(y.Size, x.Size);
            Matrix2D yy = new Matrix2D(y.Size, x.Size);

            if (x.Size != 0 && y.Size != 0)
            {
                if (!x.IsRow) x.Transpose();
                if (y.IsRow) y.Transpose();
                xx = x.Repeat(y.Size);
                yy = y.Repeat(x.Size);
            }
            return (xx, yy);
        }

        /// <summary>
        /// The method defines the matrix filled by 1 and 0
        /// 1 is placed on the element's position that is less or equal to scalar
        /// 0 is placed on the element's position that is greater than the scalar
        /// </summary>
        /// <param name="scalar"> the scalar to compare with </param>
        /// <returns> Matrix2D filled by 1 and 0 of the size of data matrix </returns>
        public Matrix2D IsLessThanScalar(double scalar)
        {
            Matrix2D result = new Matrix2D(Rows, Columns);
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                    result[i, j] = (matrix[i][j] <= 1) ? 1 : 0;
            }
            return result;
        }

        /// <summary>
        /// The method defines the matrix filled by 1 and 0
        /// 1 is placed on the element's position that equals zero
        /// 0 is placed on the element's position that doesn't equal zero
        /// </summary>
        /// <returns> Matrix2D filled by 1 and 0 of the size of data matrix </returns>
        public Matrix2D IsEqualsZero()
        {
            Matrix2D matrixcp = new Matrix2D(Rows, Columns);
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (this[i, j] == 0)
                        matrixcp[i, j] = 1;
                    else
                        matrixcp[i, j] = 0;
                }
            }
            return matrixcp;
        }

        /// <summary>
        /// The method defines the unary minus operation
        /// </summary>
        /// <param name="first"> First matrix </param>
        /// <returns> The result of unary minus operation </returns>
        public static Matrix2D operator -(Matrix2D first) => ByElem(first, MathFunctions.UnaryMinus);


        /// <summary>
        /// The method defines the minus operation
        /// </summary>
        /// <param name="first"> First matrix </param>
        /// <param name="second"> Second matrix </param>
        /// <returns> The result of minux operation </returns>
        public static Matrix2D operator -(Matrix2D first, Matrix2D second) => ByElem(first, second, MathFunctions.Minus);

        /// <summary>
        /// The method defines the minus operation of number - matrix
        /// where number is an initial value for first matrix
        /// </summary>
        /// <param name="first"> A number that fills first matrix </param>
        /// <param name="second"> Second matrix </param>
        /// <returns> The result of minus operation </returns>
        public static Matrix2D operator -(double first, Matrix2D second)
        {
            Matrix2D result = new Matrix2D(second.Rows, second.Columns, first);
            return (result - second);
        }

        /// <summary>
        /// The method defines the plus operation
        /// </summary>
        /// <param name="first"> First matrix </param>
        /// <param name="second"> Second matrix </param>
        /// <returns> The result of plus operation </returns>
        public static Matrix2D operator +(Matrix2D first, Matrix2D second) => ByElem(first, second, MathFunctions.Plus);

        /// <summary>
        /// The method defines the plus operation for matrix (first matrix)
        /// and number where number is a value that fills second matrix
        /// </summary>
        /// <param name="matrix"> matrix </param>
        /// <param name="number"> number </param>
        /// <returns> The result of plus operation </returns>
        public static Matrix2D operator +(Matrix2D matrix, double number)
        {
            Matrix2D result = new(matrix.Rows, matrix.Columns, number);
            result += matrix;
            return result;
        }

        /// <summary>
        /// The method defines the element by element multiply operation
        /// </summary>
        /// <param name="first"> First matrix </param>
        /// <param name="second"> Second matrix </param>
        /// <returns> The result of element by element multiply operation </returns>
        public static Matrix2D operator ^(Matrix2D first, Matrix2D second) => ByElem(first, second, MathFunctions.Multiply);

        /// <summary>
        /// The method defines the multyply between a matrix and a number
        /// </summary>
        /// <param name="matrix"> The matrix </param>
        /// <param name="number"> The number to multiply the matrix by </param>
        /// <returns></returns>
        public static Matrix2D operator *(Matrix2D matrix, double number) {
            Matrix2D result = new(matrix.Rows, matrix.Columns);

            for (int i = 0; i < result.Rows; ++i)
                for (int j = 0; j < result.Columns; ++j)
                    result[i, j] = number * matrix[i, j];

            return result;
        }

        /// <summary>
        /// The method defines the multyply between a matrix and a vector
        /// </summary>
        /// <param name="matrix"> The matrix </param>
        /// <param name="number"> The number to multiply the matrix by </param>
        /// <returns></returns>
        public static Matrix2D operator *(Matrix2D matrix, Vector vector) {
            if (!vector.IsRow && matrix.Columns != vector.Size)
                throw new ArgumentException("The number of rows first " +
                    "matrix don't equal the number of columns second matrix!");
            else if (vector.IsRow && matrix.Columns != 1)
                throw new ArgumentException("The number of rows first " +
                    "matrix don't equal the number of columns second matrix!");

            Matrix2D result;
            int rows = matrix.Rows;

            if (vector.IsRow) {
                int columns = vector.Size;
                result = new(rows, columns);

                for (int i = 0; i < rows; ++i)
                    for (int j = 0; j < columns; ++j)
                        result[i, j] = matrix[i, j] * vector[j];
            } else {
                int jMax = matrix.Columns;
                result = new(rows, 1, 0);

                for (int i = 0; i < rows; ++i)
                    for (int j = 0; j < jMax; ++j)
                        result[i] += matrix[i, j] * vector[j];
            }

            return result;
        }

        /// <summary>
        /// The method defines the matrix multiply operation
        /// </summary>
        /// <param name="first"> First matrix </param>
        /// <param name="second"> Second matrix </param>
        /// <returns> The result of matrix multiply operation </returns>
        public static Matrix2D operator *(Matrix2D first, Matrix2D second) {
            if (first.Columns != second.Rows)
                throw new ArgumentException("The number of rows first " +
                    "matrix don't equal the number of columns second matrix!");

            Matrix2D result = new(first.Rows, second.Columns, 0);
            for (int i = 0; i < first.Rows; ++i)
                for (int j = 0; j < second.Columns; ++j)
                    for (int k = 0; k < second.Rows; ++k)
                        result[i, j] += first[i, k] * second[k, j];

            return result;
        }

        /// <summary>
        /// The method defines the matrix element - by - element division
        ///  (sizes of matrixes should match)
        /// </summary>
        /// <param name="first"> First matrix </param>
        /// <param name="second"> Second matrix </param>
        /// <returns> ResultMatrix[i,j] = Matrix1[i,j] / Matrix2[i, j] </returns>
        public static Matrix2D operator /(Matrix2D first, Matrix2D second) {
            if (first.Rows != second.Rows || first.Columns != second.Columns)
                throw new ArgumentException("Matrixes sizes don't match!");

            Matrix2D result = new(first.Rows, second.Columns);
            for (int i = 0; i < first.Rows; ++i)
                for (int j = 0; j < second.Columns; ++j)
                    result[i, j] = first[i, j] / second[i, j];

            return result;
        }

        public static Matrix2D operator /(double num, Matrix2D matrix) {
            Matrix2D result = new(matrix.Rows, matrix.Columns);

            for (int i = 0; i < matrix.Rows; ++i)
                for (int j = 0; j < matrix.Columns; ++j)
                    result[i, j] = num / matrix[i, j];

            return result;
        }

        /// <summary>
        /// The method defines division matrix by number
        /// </summary>
        /// <param name="matrix"> The matrix </param>
        /// <param name="number"> The number to divide by </param>
        /// <returns></returns>
        public static Matrix2D operator /(Matrix2D matrix, double number) {
            return matrix * (1 / number);
        }

        public static Matrix2D MulByRows(Matrix2D matrix) {
            Matrix2D product = new(matrix.Rows, 1);

            for (int i = 0; i < matrix.Rows; ++i) {
                product[i] = 1;
                for (int j = 0; j < matrix.Columns; ++j)
                    product[i] *= matrix[i, j];
            }

            return product;
        }

        private static Matrix2D ByElem(Matrix2D first,
           Func<double, double> action)
        {
            Matrix2D result = new(first.Rows, first.Columns);
            for (int i = 0; i < first.Rows; ++i)
                for (int j = 0; j < first.Columns; ++j)
                    result[i, j] = action(first[i, j]);

            return result;
        }

        private static Matrix2D ByElem(Matrix2D first, Matrix2D second,
            Func<double, double, double> action) {
            if (first.Rows != second.Rows || first.Columns != second.Columns)
                throw new ArgumentException("Dimensions of matrixes are different!");

            Matrix2D result = new(first.Rows, first.Columns);
            for (int i = 0; i < first.Rows; ++i)
                for (int j = 0; j < first.Columns; ++j)
                    result[i, j] = action(first[i, j], second[i, j]);

            return result;
        }

        public List<List<double>> ToList() {
            List<List<double>> list = new(Rows);

            for (int i = 0; i < Rows; ++i) {
                list.Add(new(Columns));
                for (int j = 0; j < Columns; ++j)
                    list[i].Add(this[i, j]);
            }

            return list;
        }

        public int[] GetIndexesOfMinElementsInRows()
        {
            int[] indexes = new int[Rows];
            for (int i = 0; i < Rows; i++)
                indexes[i] = this.GetRow(i).IndexOfMinElement();
            return indexes;
        }
    }
}
