using System.Windows.Media;
using System.Windows.Media.Imaging;

using GUI.Settings;

namespace GUI.PLT {
    public class PLTPicture {
        public Images Rendered { get; }
        public AlgorithmSettings Settings { get; private set; }

        public PLTDecoderRes PLTDecoded { get; }
        public double Width => PLTDecoded.Width;
        public double Height => PLTDecoded.Height;

        private uint angleOfRotation = 0;

        public PLTPicture(in AlgorithmSettings settings, in Images rendered,
                          in PLTDecoderRes pltDecoded) {
            Settings = settings;
            PLTDecoded = pltDecoded;
            Rendered = rendered;
        }

        public void RestoreRotationAngle(PLTPicture picture) {
            angleOfRotation = picture.angleOfRotation;
            Rendered.Rotate(angleOfRotation);
        }

        public void Rotate() {
            angleOfRotation = (angleOfRotation + 90) % 360;
            Rendered.Rotate(angleOfRotation);
        }
    }
}
