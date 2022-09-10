namespace GUI {
    internal class Vector {
        private double[] vector;
        public bool IsRow { get; private set; }

        public double this[int index] {
            get => vector[index];
            set => vector[index] = value;
        }

        public Vector(int size, bool isRow = true) {
            vector = new double[size];
            IsRow = isRow;
        }

        public void Transpose() {
            IsRow = !IsRow;
        }
    }
}
