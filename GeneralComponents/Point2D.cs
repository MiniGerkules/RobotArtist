using System;

namespace GeneralComponents {
    public struct Point2D {
        public uint X { get; private set; }
        public uint Y { get; private set; }

        public Point2D(uint x, uint y) {
            X = x;
            Y = y;
        }

        public Point2D(uint[] coords) : this(coords[0], coords[1]) {
        }

        public void Deconstruct(out uint x, out uint y) {
            x = X;
            y = Y;
        }

        public Point2D Divide(uint divider) {
            if (divider == 0)
                throw new ArgumentException("The divider is 0!");

            X /= divider;
            Y /= divider;

            return this;
        }

        public Point2D Divide(double divider) {
            if (divider == 0)
                throw new ArgumentException("The divider is 0!");

            X = (uint)(X / divider);
            Y = (uint)(Y / divider);

            return this;
        }

        public Point2D Plus(uint xBias, uint yBias) {
            X += xBias;
            Y += yBias;

            return this;
        }

        public Point2D Plus(Point2D other) {
            X += other.X;
            Y += other.Y;

            return this;
        }
    }
}
