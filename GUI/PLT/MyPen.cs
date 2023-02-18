using System.Windows.Media;

namespace GUI.PLT {
    internal class MyPen {
        private Brush brush = Brushes.White;
        private double thickness = 3;

        public Pen Pen { get; private set; }
        public Brush Brush {
            get => brush;
            set {
                brush = value;
                UpdatePen();
            } 
        }
        public double Thickness {
            get => thickness;
            set {
                thickness = value;
                UpdatePen();
            }
        }

        public MyPen() { Pen = new(Brush, Thickness); }

        public MyPen(Brush brush, uint thickness, double scale) {
            Brush = brush;
            Thickness = thickness;
            Pen = new(Brush, Thickness);
        }

        private void UpdatePen() {
            Pen = new() {
                Thickness = Thickness,
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                Brush = Brush,
            };
    }
    }
}
