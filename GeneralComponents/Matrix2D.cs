using NPOI.HSSF.Record;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

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

        public Matrix2D(List<double> vector) {
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

        public Matrix2D(List<List<double>> matrix) {
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
                        result[j, i*Columns + k] = this[j, k];

            return result;
        }

        public Matrix2D RepeatRows(int numOfRepeat) {
            Matrix2D result = new(Rows * numOfRepeat, Columns);

            for (int i = 0; i < numOfRepeat; ++i)
                for (int j = 0; j < Rows; ++j)
                    for (int k = 0; k < Columns; ++k)
                        result[i*Rows + j, k] = this[j, k];

            return result;
        }

        public int[] GetIndexesForSorted() {
            double[] firstColumn = new double[Rows];
            int[] indexes = new int[Rows];

            for (int i = 0; i < Rows; ++i) {
                firstColumn[i] = this[i, 0];
                indexes[i] = i;
            }

            Array.Sort(firstColumn, indexes);

            return indexes;
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

        public Matrix2D Pow(Vector powers) {
            Matrix2D matrix = new(powers);
            
            if (powers.IsRow)
                matrix = matrix.RepeatRows(Rows);
            else
                matrix = matrix.RepeatColumns(Columns);

            return ByElem(this, matrix, Math.Pow);
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

        public static explicit operator double(Matrix2D matrix) {
            if (matrix.Rows != 1 || matrix.Columns != 1)
                throw new ArgumentException("Matrix dimension is not 1x1!");
            return matrix[0];
        }

        public Matrix2D Square() {
            return DoAction(Helpers.Square);
        }
        public Matrix2D Sqrt()
        {
            return DoAction(Helpers.Sqrt);
        }

        public Matrix2D DoAction(Func<double, double> action) {
            Matrix2D result = new(Rows, Columns);

            for (int i = 0; i < Rows; ++i)
                for (int j = 0; j < Columns; ++j)
                    result[i, j] = action(this[i, j]);

            return result;
        }

        /// <summary>
        /// The method defines the minus operation
        /// </summary>
        /// <param name="first"> First matrix </param>
        /// <param name="second"> Second matrix </param>
        /// <returns> The result of minux operation </returns>
        public static Matrix2D operator -(Matrix2D first, Matrix2D second) => ByElem(first, second, Helpers.Minus);

        /// <summary>
        /// The method defines the plus operation
        /// </summary>
        /// <param name="first"> First matrix </param>
        /// <param name="second"> Second matrix </param>
        /// <returns> The result of plus operation </returns>
        public static Matrix2D operator +(Matrix2D first, Matrix2D second) => ByElem(first, second, Helpers.Plus);

        /// <summary>
        /// The method defines the element by element multiply operation
        /// </summary>
        /// <param name="first"> First matrix </param>
        /// <param name="second"> Second matrix </param>
        /// <returns> The result of element by element multiply operation </returns>
        public static Matrix2D operator ^(Matrix2D first, Matrix2D second) => ByElem(first, second, Helpers.Multiply);

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
        public static Matrix2D operator /(Matrix2D first, Matrix2D second)
        {
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

        public static Matrix2D MulByRows(Matrix2D matrix) {
            Matrix2D product = new(matrix.Rows, 1);

            for (int i = 0; i < matrix.Rows; ++i) {
                product[i] = 1;
                for (int j = 0; j < matrix.Columns; ++j)
                    product[i] *= matrix[i, j];
            }

            return product;
        }

        private static Matrix2D ByElem(Matrix2D first, Matrix2D second,
            Func<double, double, double> action) {
            if (first.Rows != second.Rows || first.Columns != second.Columns)
                throw new ArgumentException("Dimensions of matrixes is different!");

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
    }
}
