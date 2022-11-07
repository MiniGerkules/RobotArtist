using System;
using System.Collections.Generic;

namespace GeneralComponents {
    public class Matrix3D {
        public int Layers { get => matrix.Length; }
        private Matrix2D[] matrix;

        public double this[int row, int column, int layer] {
            get {
                return matrix[layer][row, column];
            }
            set {
                matrix[layer][row, column] = value;
            }
        }

        public Matrix2D this[int layer] {
            get => matrix[layer];
            set => matrix[layer] = value;
        }

        public Matrix3D(int rows, int columns, int layers = 3) {
            matrix = new Matrix2D[layers];
            for (int i = 0; i < layers; ++i)
                matrix[i] = new(rows, columns);
        }

        public Matrix3D(int rows, int columns, int layers, double initVal) : this(rows, columns, layers) {
            for (int k = 0; k < layers; ++k)
                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < columns; j++)
                        this[i, j, k] = initVal;
        }

        public Matrix3D(List<List<List<double>>> matrix3D) // layers, columns, rows
        {
            if (matrix3D.Count == 0 || matrix3D[0].Count == 0 || matrix3D[0][0].Count == 0)
                throw new ArgumentException("Pass a empty list to construct " +
                    "a new matrix!");
            matrix = new Matrix2D[matrix3D.Count];
            for (int i = 0; i < matrix3D.Count; i++) {
                matrix[i] = new Matrix2D(matrix3D[i]);
            }
        }

        public static Matrix3D operator -(Matrix3D first, Matrix3D second) => ByElem(first, second, MathFunctions.Minus);

        private static Matrix3D ByElem(Matrix3D first, Matrix3D second,
                                       Func<double, double, double> action) {
            if (first.Layers != second.Layers || first[0].Rows != second[0].Rows
                || first[0].Columns != second[0].Columns)
                throw new ArgumentException("Dimensions of matrixes is different!");

            Matrix3D result = new(first[0].Rows, first[0].Columns);
            for (int k = 0; k < first.Layers; ++k)
                for (int i = 0; i < first[0].Rows; ++i)
                    for (int j = 0; j < first[0].Columns; ++j)
                        result[i, j, k] = action(first[i, j, k], second[i, j, k]);

            return result;
        }

        public Matrix2D rgb2gray() {
            int rows = this[0].Rows;
            int columns = this[0].Columns;
            Matrix2D result = new Matrix2D(rows, columns, 0);
            for (int k = 0; k < Layers; k++) {
                for (int x = 0; x < rows; x++) {
                    for (int y = 0; y < columns; y++) {
                        // count weighted sum: R * 0.2989 + G * 0.5870 + B * 0.1140
                        result[x, y] = Math.Round((this[x, y, 0] * 0.2989)
                            + (this[x, y, 1] * 0.5870) + (this[x, y, 2] * 0.1140));
                    }
                }
            }
            return result;
        }

        public Matrix2D mean()
        {
            Matrix2D answer = new Matrix2D(matrix[0].Rows, matrix[0].Columns);
            if (this.Layers > 1)
            {
                for (int i = 0; i < matrix[0].Rows; i++)
                {
                    for (int j = 0; j < matrix[0].Columns; j++)
                    {
                        for (int k = 0; k < Layers; k++)
                            answer[i, j] += matrix[k][i, j];
                        answer[i, j] /= 3d;
                    }
                }
            }
            else
                answer = matrix[0];
            return answer;
        }

    }
}
