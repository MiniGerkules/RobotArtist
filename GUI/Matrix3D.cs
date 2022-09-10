namespace GUI {
    internal class Matrix3D {
        private Matrix[] matrix;

        public Matrix this[int layer] {
            get => matrix[layer];
            set => matrix[layer] = value;
        }

        public Matrix3D(int rows, int columns, int layers) {
            matrix = new Matrix[layers];

            for (int i = 0; i < layers; ++i)
                matrix[i] = new(rows, columns);
        }
    }
}
