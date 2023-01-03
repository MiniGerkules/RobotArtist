using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GUI.PLT {
    public class Images {
        public BitmapSource MainImage { get; private set; }
        public BitmapSource StrokesStructure { get; private set; }

        public Images(BitmapSource mainImage, BitmapSource stokesStructure) {
            MainImage = mainImage;
            StrokesStructure = stokesStructure;
        }

        public void Freeze() {
            if (MainImage.CanFreeze) MainImage.Freeze();
            if (StrokesStructure.CanFreeze) StrokesStructure.Freeze();
        }

        public void Rotate(uint angle) {
            MainImage = SetAngle(MainImage, angle);
            StrokesStructure = SetAngle(StrokesStructure, angle);
            Freeze();
        }

        private static BitmapSource SetAngle(BitmapSource image, in uint angle) {
            RotateTransform rotate = new(angle);
            TransformedBitmap transformed = new(image, rotate);
            if (transformed.CanFreeze) transformed.Freeze();

            return transformed;
        }
    }
}
