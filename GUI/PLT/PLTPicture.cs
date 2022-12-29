using System.Windows.Media;
using System.Windows.Media.Imaging;

using GUI.Settings;

namespace GUI.PLT {
    internal class PLTPicture {
        public BitmapSource RenderedPicture { get; private set; }
        public AlgorithmSettings Settings { get; private set; }

        public PLTDecoderRes PLTDecoded { get; }
        public double Width => PLTDecoded.Width;
        public double Height => PLTDecoded.Height;

        private uint angleOfRotation = 0;

        public PLTPicture(in AlgorithmSettings settings, in BitmapSource renderedPicture,
                          in PLTDecoderRes pltDecoded) {
            PLTDecoded = pltDecoded;
            RenderedPicture = renderedPicture;
            Settings = settings;
        }

        public void RestoreRotationAngle(in PLTPicture picture) {
            SetAngle(picture.angleOfRotation);
        }

        public void Rotate() {
            angleOfRotation = (angleOfRotation + 90) % 360;
            SetAngle(90);
        }

        private void SetAngle(uint angle) {
            RotateTransform rotate = new(angle);
            TransformedBitmap tb = new(RenderedPicture, rotate);
            RenderedPicture = tb;

            if (RenderedPicture.CanFreeze)
                RenderedPicture.Freeze();
        }
    }
}
