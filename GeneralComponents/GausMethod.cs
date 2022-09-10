using System;

namespace GeneralComponents
{
    internal class GausMethod
    {
        public static Matrix Solve(Matrix coefs, Matrix answers)
        {
            if (answers.Columns != 1 || answers.Rows != coefs.Rows)
                throw new ArgumentException("Can't solve a linear system! Matrix dimensions isn't same!");

            if (coefs.Rows != coefs.Columns)
                throw new ArgumentException("Method solve only a square matrix!");

            Matrix coefsCopy = new(coefs);
            Matrix answersCopy = new(answers);

            // Tikhonov regularization
            Matrix regularization = new(coefs.Rows, 1, 1e-10);
            regularization = regularization.MakeDiag();
            coefsCopy = coefsCopy + regularization;

            for (int i = 0; i < coefsCopy.Rows; ++i)
            {
                if (IsFullNulls(coefsCopy, i))
                    throw new ArgumentException("The system haven't one solution!");

                if (Equals(coefsCopy[i, i], 0))
                    if (!TrySwap(coefsCopy, answersCopy, i, i))
                        continue;

                for (int j = i + 1; j < coefs.Rows; ++j)
                {
                    double toMul = coefsCopy[j, i] / coefsCopy[i, i];
                    coefsCopy[j, i] = 0;
                    answersCopy[j] -= answersCopy[i] * toMul;

                    for (int k = i + 1; k < coefsCopy.Columns; ++k)
                        coefsCopy[j, k] -= coefsCopy[i, k] * toMul;
                }
            }

            Matrix result = new(coefs.Columns, 1);
            for (int i = result.Rows - 1; i >= 0; i--)
            {
                if (Equals(coefsCopy[i, i], 0))
                    throw new ArgumentException("The system haven't one solution!");

                double divisible = answersCopy[i];
                for (int j = i + 1; j < coefs.Columns; ++j)
                    divisible -= result[j] * coefsCopy[i, j];

                result[i] = divisible / coefsCopy[i, i];
            }

            return result;
        }

        private static bool IsFullNulls(Matrix matrix, int curRow)
        {
            for (int i = 0; i < matrix.Columns; ++i)
                if (matrix[curRow, i] != 0)
                    return false;

            return true;
        }

        private static bool TrySwap(Matrix coefs, Matrix answers, int startRow, int column)
        {
            for (int i = startRow; i < coefs.Rows; ++i)
            {
                if (Equals(coefs[i, column], 0))
                    continue;

                for (int j = 0; j < coefs.Columns; ++j)
                    (coefs[i, j], coefs[startRow, j]) = (coefs[startRow, j], coefs[i, j]);
                (answers[i], answers[startRow]) = (answers[startRow], answers[i]);

                return true;
            }

            return false;
        }
    }
}
