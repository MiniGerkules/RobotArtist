using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Windows.Media;

namespace GeneralComponents {
    public class Vector {
        private readonly double[] vector;
        public int Size { get => vector.Length; }
        public bool IsRow { get; private set; }

        public double this[int index] {
            get => vector[index];
            set => vector[index] = value;
        }

        public Vector(int size, bool isRow = true) {
            if (size == 0)
                throw new ArgumentException("Can't create an empty vector!");

            vector = new double[size];
            IsRow = isRow;
        }

        public Vector(double[] vector, bool isRow = true) {
            if (vector.Length == 0)
                throw new ArgumentException("Can't create an empty vector!");

            this.vector = new double[vector.Length];
            IsRow = isRow;
            Array.Copy(vector, this.vector, vector.Length);
        }

        public Vector(List<double> vector, bool isRow = true) : this(vector.ToImmutableList(), isRow) { }

        public Vector(ImmutableList<double> vector, bool isRow = true) {
            if (vector.Count == 0)
                throw new ArgumentException("Can't create an empty vector!");

            this.vector = new double[vector.Count];
            vector.CopyTo(this.vector);
            IsRow = isRow;
        }

        public Vector(Vector other) : this(other.vector) {
            IsRow = other.IsRow;
        }

        public void Transpose() {
            IsRow = !IsRow;
        }

        public int IndexOfMinElement()
        {
            return Array.IndexOf(vector, vector.Min());
        }

        public void FillByZeros()
        {
            for (int i = 0; i < vector.Length; i++)
                vector[i] = 0;
        }

        public static void Copy(Vector first, Vector second)
        {
            Array.Copy(first.vector, second.vector, Math.Min(first.Size, second.Size));
        }

        public double Sum() => vector.Sum();

        public Vector Square() => DoAction(MathFunctions.Square);

        public Vector SqrtFromAbs() => DoAction(MathFunctions.SqrtFromAbs);

        public Vector Pow(Vector powers) => ByElem(this, powers, Math.Pow);

        public static Vector linspace(double start, double end, int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("amount of dots should be a positive integer number!");
            Vector result = new Vector(amount, true);
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

        /// <summary>
        /// The method repeats data vector numOfRepeat times
        /// if vector is a row, than vector will consist of numOfRepeat rows
        /// if vector is a column, than vector will consist of numOfRepeat columns
        /// </summary>
        /// <param name="numOfRepeat"> number of repeating vector </param>
        /// <returns> The vector of numOfRepeat data vectors </returns>
        public Matrix2D Repeat(int numOfRepeat)
        {
            Matrix2D result = new Matrix2D(numOfRepeat, Size);
            for (int i = 0; i < numOfRepeat; i++)
                for (int j = 0; j < Size; j++)
                    result[i, j] = vector[j];
            if (!IsRow)
                result = result.Transpose();
            return result;
        }

        /// <summary>
        /// The method creates a vector with elements from -boarder with step = 1
        /// of the size of ceiled doubled boarder + 1
        /// example 1: boarder = 3 => returns Vector [-3 -2 -1  0  1  2  3]
        /// example 2: boarder = 1.5 => returns Vector [-1.5 -0.5  0.5  1.5]
        /// </summary>
        /// <param name="boarder"> numbers starts from -boarder </param>
        /// <returns> vector filled by numbers from -boarder to specified length with step = 1 </returns>
        public static Vector CreateInMinusPlusRange(double boarder)
        {
            Vector result = new Vector((int)(boarder * 2) + 1, true);
            for (int i = 0; i <= boarder * 2; i++)
                result[i] = i - boarder;
            return result;
        }

        /// <summary>
        /// The method gets the product of vector's elements
        /// </summary>
        /// <returns> The product of vector's elements </returns>
        public double Product()
        {
            double result = 1;

            for (int i = 0; i < Size; ++i)
                result *= this[i];

            return result;
        }

        /// <summary>
        /// The method defines the minus operation number - vector
        /// where number is initial value for first vector
        /// </summary>
        /// <param name="first"> number to fill first vector by </param>
        /// <param name="second"> Second vector </param>
        /// <returns> The result of minus operation </returns>
        public static Vector operator -(double first, Vector second) => ByElem(first, second, MathFunctions.Minus);

        public Vector DoAction(Func<double, double> action)
        {
            Vector result = new(Size, IsRow);

            for (int i = 0; i < Size; ++i)
                    result[i] = action(this[i]);

            return result;
        } 

        private static Vector ByElem(Vector first, Vector second,
            Func<double, double, double> action)
        {
            if (first.Size != second.Size || first.IsRow != second.IsRow)
                throw new ArgumentException("Vectors are incompatible!");

            Vector result = new(first.Size, first.IsRow);
            for (int i = 0; i < first.Size; ++i)
                    result[i] = action(first[i], second[i]);

            return result;
        }

        private static Vector ByElem(double scalar, Vector second,
            Func<double, double, double> action)
        {

            Vector result = new(second.Size, second.IsRow);
            for (int i = 0; i < second.Size; ++i)
                result[i] = action(scalar, second[i]);

            return result;
        }
    }
}
