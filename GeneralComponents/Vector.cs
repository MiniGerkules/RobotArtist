using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

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
    }
}
