using System;
using System.Collections.Generic;

namespace GeneralComponents
{
    public class Matrix
    {
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        private readonly double[,] matrix;

        public double this[int row, int column]
        {
            get => matrix[row, column];
            set => matrix[row, column] = value;
        }

        public double this[int index]
        {
            get
            {
                if (Rows == 1)
                    return matrix[0, index];
                else if (Columns == 1)
                    return matrix[index, 0];
                else
                    throw new IndexOutOfRangeException("Index out of range!");
            }

            set
            {
                if (Rows == 1)
                    matrix[0, index] = value;
                else if (Columns == 1)
                    matrix[index, 0] = value;
                else
                    throw new IndexOutOfRangeException("Index out of range!");
            }
        }

        public Matrix(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            matrix = new double[rows, columns];
        }

        public Matrix(int rows, int columns, double initVal) : this(rows, columns)
        {
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                    matrix[i, j] = initVal;
        }

        public Matrix(List<double> vector)
        {
            if (vector.Count == 0)
                throw new ArgumentException("Pass a empty list to construct " +
                    "a new matrix!");

            Rows = 1;
            Columns = vector.Count;
            matrix = new double[Rows, Columns];

            for (int j = 0; j < Columns; ++j)
                matrix[0, j] = vector[j];
        }

        public Matrix(List<List<double>> matrix)
        {
            if (matrix.Count == 0)
                throw new ArgumentException("Pass a empty list to construct " +
                    "a new matrix!");

            Rows = matrix.Count;
            Columns = matrix[0].Count;
            this.matrix = new double[Rows, Columns];

            for (int i = 0; i < Rows; ++i)
                for (int j = 0; j < Columns; ++j)
                    this.matrix[i, j] = matrix[i][j];
        }

        public Matrix(Matrix matrix)
        {
            Rows = matrix.Rows;
            Columns = matrix.Columns;
            this.matrix = new double[Rows, Columns];
            Array.Copy(matrix.matrix, this.matrix, Rows * Columns);
        }

        public Matrix GetRow(int rowIndex)
        {
            Matrix row = new(1, Columns);

            for (int i = 0; i < Columns; ++i)
                row[i] = matrix[rowIndex, i];

            return row;
        }

        public Matrix GetColumn(int columnIndex)
        {
            Matrix column = new(Rows, 1);

            for (int i = 0; i < Rows; ++i)
                column[i] = matrix[i, columnIndex];

            return column;
        }

        public void SwapRows(int index1, int index2)
        {
            if (!(0 < index1 && index1 < Rows && 0 < index2 && index2 < Rows))
                throw new IndexOutOfRangeException("The indexes is incorrect! " +
                    "They must be positive and less then number of matrix rows!");

            double[] temp = new double[Columns];
            for (int i = 0; i < Columns; ++i)
                temp[i] = matrix[index1, i];

            for (int i = 0; i < Columns; ++i)
                matrix[index1, i] = matrix[index2, i];

            for (int i = 0; i < Columns; ++i)
                matrix[index2, i] = temp[i];
        }

        public void SwapColumns(int index1, int index2)
        {
            if (!(0 < index1 && index1 < Columns && 0 < index2 && index2 < Columns))
                throw new IndexOutOfRangeException("The indexes is incorrect! " +
                    "They must be positive and less then number of matrix rows!");

            double[] temp = new double[Rows];
            for (int i = 0; i < Rows; ++i)
                temp[i] = matrix[i, index1];

            for (int i = 0; i < Rows; ++i)
                matrix[i, index1] = matrix[i, index2];

            for (int i = 0; i < Rows; ++i)
                matrix[i, index2] = temp[i];
        }

        public static Matrix Eye(int dimension)
        {
            Matrix eye = new Matrix(dimension, dimension);
            for (int i = 0; i < dimension; i++)
                eye[i, i] = 1;
            return eye;
        }

        public Matrix RepeatColumns(int numOfRepeat)
        {
            Matrix result = new(Rows, Columns * numOfRepeat);

            for (int k = 0; k < numOfRepeat; ++k)
                for (int i = 0; i < Rows; ++i)
                    for (int j = 0; j < Columns; ++j)
                        result[i, k*Columns + j] = matrix[i, j];

            return result;
        }

        public Matrix RepeatRows(int numOfRepeat)
        {
            Matrix result = new(Rows * numOfRepeat, Columns);

            for (int k = 0; k < numOfRepeat; ++k)
                for (int i = 0; i < Rows; ++i)
                    for (int j = 0; j < Columns; ++j)
                        result[k * Rows + i, j] = matrix[i, j];

            return result;
        }

        public int[] GetIndexesForSorted()
        {
            int[] indexes = new int[Rows];
            double[] firstColumn = new double[Rows];

            for (int i = 0; i < Rows; ++i)
            {
                indexes[i] = i;
                firstColumn[i] = matrix[i, 0];
            }

            Array.Sort(firstColumn, indexes);

            return indexes;
        }

        public void MakeUnit(int startRow)
        {
            if (startRow > Rows)
                throw new IndexOutOfRangeException("Can't make unit matrix! " +
                    "The index of the start row is greater than rows number.");

            int column = 0;
            for (int i = startRow; i < Rows && column < Columns; ++i)
                matrix[i, column++] = 1;
        }

        public Matrix Transpose()
        {
            Matrix result = new(Columns, Rows);

            for (int i = 0; i < Rows; ++i)
                for (int j = 0; j < Columns; ++j)
                    result[j, i] = matrix[i, j];

            return result;
        }

        public double GetSum()
        {
            double result = 0;
            for (int i = 0; i < Rows; ++i)
                for (int j = 0; j < Columns; ++j)
                    result += matrix[i, j];
            return result;
        }

        public Matrix GetByIndexes(int[] indexes)
        {
            Matrix matrix = new(indexes.Length, Columns);

            for (int i = 0; i < indexes.Length; ++i)
                for (int j = 0; j < Columns; ++j)
                    matrix[i, j] = this.matrix[indexes[i], j];

            return matrix;
        }

        public Matrix MakeDiag()
        {
            if (!(Rows > 1 && Columns == 1 || Rows == 1 && Columns > 1))
                throw new ArgumentException("Can't make a diagonal matrix! The matrix must be a vector!");

            int dim = Math.Max(Rows, Columns);
            Matrix result = new(dim, dim, 0);

            for (int i = 0; i < dim; ++i)
                result[i, i] = this[i];

            return result;
        }

        public Matrix Pow(Matrix powers)
        {
            if (powers.Columns == 1 && powers.Rows == Rows)
            {
                powers = powers.RepeatColumns(Columns);
                return ByElem(this, powers, Math.Pow);
            }
            else if (powers.Rows == 1 && powers.Columns == Columns)
            {
                powers = powers.RepeatRows(Rows);
                return ByElem(this, powers, Math.Pow);
            }
            else if (powers.Columns == Columns && powers.Rows == Rows)
            {
                return ByElem(this, powers, Math.Pow);
            }
            else
            {
                throw new ArgumentException("The matrix dimensions isn't same!");
            }
        }

        public static explicit operator double(Matrix matrix)
        {
            if (matrix.Rows != 1 || matrix.Columns != 1)
                throw new ArgumentException("Matrix dimension is not 1x1!");    
            return matrix[0];
        } 

        public Matrix Square()
        {
            return DoAction(Helpers.Square);
        }

        public Matrix DoAction(Func<double, double> action)
        {
            Matrix result = new(Rows, Columns);

            for (int i = 0; i < Rows; ++i)
                for (int j = 0; j < Columns; ++j)
                    result[i, j] = action(matrix[i, j]);

            return result;
        }

        /// <summary>
        /// The method defines the minus operation
        /// </summary>
        /// <param name="first"> First matrix </param>
        /// <param name="second"> Second matrix </param>
        /// <returns> The result of minux operation </returns>
        public static Matrix operator -(Matrix first, Matrix second) => ByElem(first, second, Helpers.Minus);

        /// <summary>
        /// The method defines the plus operation
        /// </summary>
        /// <param name="first"> First matrix </param>
        /// <param name="second"> Second matrix </param>
        /// <returns> The result of plus operation </returns>
        public static Matrix operator +(Matrix first, Matrix second) => ByElem(first, second, Helpers.Plus);

        /// <summary>
        /// The method defines the element by element multiply operation
        /// </summary>
        /// <param name="first"> First matrix </param>
        /// <param name="second"> Second matrix </param>
        /// <returns> The result of element by element multiply operation </returns>
        public static Matrix operator ^(Matrix first, Matrix second) => ByElem(first, second, Helpers.Multiply);

        /// <summary>
        /// The method defines the multyply between a matrix and a number
        /// </summary>
        /// <param name="matrix"> The matrix </param>
        /// <param name="number"> The number to multiply the matrix by </param>
        /// <returns></returns>
        public static Matrix operator *(Matrix matrix, double number)
        {
            Matrix result = new(matrix.Rows, matrix.Columns);

            for (int i = 0; i < result.Rows; ++i)
                for (int j = 0; j < result.Columns; ++j)
                    result[i, j] = number * matrix[i, j];

            return result;
        }

        /// <summary>
        /// The method defines division matrix by number
        /// </summary>
        /// <param name="matrix"> The matrix </param>
        /// <param name="number"> The number to divide by </param>
        /// <returns></returns>
        public static Matrix operator /(Matrix matrix, double number)
        {
            return matrix * (1 / number);
        }

        /// <summary>
        /// The method defines the matrix multiply operation
        /// </summary>
        /// <param name="first"> First matrix </param>
        /// <param name="second"> Second matrix </param>
        /// <returns> The result of matrix multiply operation </returns>
        public static Matrix operator *(Matrix first, Matrix second)
        {
            if (first.Columns != second.Rows)
                throw new ArgumentException("The number of rows first " +
                    "matrix don't equal the number of columns second matrix!");

            Matrix result = new(first.Rows, second.Columns, 0);
            for (int i = 0; i < first.Rows; ++i)
                for (int j = 0; j < second.Columns; ++j)
                    for (int k = 0; k < second.Rows; ++k)
                        result[i, j] += first[i, k] * second[k, j];

            return result;
        }

        public static Matrix operator /(double num, Matrix matrix)
        {
            Matrix result = new(matrix.Rows, matrix.Columns);

            for (int i = 0; i < matrix.Rows; ++i)
                for (int j = 0; j < matrix.Columns; ++j)
                    result[i, j] = num / matrix[i, j];

            return result;
        }

        public static Matrix MulByRows(Matrix matrix)
        {
            Matrix product = new(matrix.Rows, 1);

            for (int i = 0; i < matrix.Rows; ++i)
            {
                product[i] = 1;
                for (int j = 0; j < matrix.Columns; ++j)
                    product[i] *= matrix[i, j];
            }

            return product;
        }

        private static Matrix ByElem(Matrix first, Matrix second,
            Func<double, double, double> action)
        {
            if (first.Rows != second.Rows || first.Columns != second.Columns)
                throw new ArgumentException("Dimensions of matrixes is different!");

            Matrix result = new(first.Rows, first.Columns);
            for (int i = 0; i < first.Rows; ++i)
                for (int j = 0; j < first.Columns; ++j)
                    result[i, j] = action(first[i, j], second[i, j]);

            return result;
        }

        public List<List<double>> ToList()
        {
            List<List<double>> list = new(Rows);

            for (int i = 0; i < Rows; ++i)
            {
                list.Add(new(Columns));
                for (int j = 0; j < Columns; ++j)
                    list[i].Add(matrix[i, j]);
            }

            return list;
        }
    }
}
