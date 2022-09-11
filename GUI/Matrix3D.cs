namespace GUI {
    internal class Matrix3D {
        private Matrix2D[] matrix;

        public Matrix2D this[int layer] {
            get => matrix[layer];
            set => matrix[layer] = value;
        }

        public Matrix3D(int rows, int columns, int layers) {
            matrix = new Matrix2D[layers];

            for (int i = 0; i < layers; ++i)
                matrix[i] = new(rows, columns);
        }
    }
}
