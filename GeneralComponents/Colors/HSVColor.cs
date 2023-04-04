using System;
using System.Windows.Media;

namespace GeneralComponents.Colors {
    public class HSVColor : IColor {
        private readonly double hue;            // From 0 to 1
        private readonly double saturation;     // From 0 to 1
        private readonly double value;          // From 0 to 1

        public HSVColor(double hue, double saturation, double value) {
            this.hue = hue;
            this.saturation = saturation;
            this.value = value;
        }

        public HSVColor(double[] hsv) {
            if (hsv == null || hsv.Length != 3)
                throw new ArgumentException("");

            hue = hsv[0];
            saturation = hsv[1];
            value = hsv[2];
        }

        public Color GetRealColor() {
            return ToRGB().GetRealColor();
        }

        public Color GetArtificialColor() {
            return ToRGB().GetArtificialColor();
        }

        public RGBColor ToRGB() {
            // https://en.wikipedia.org/wiki/HSL_and_HSV#HSV_to_RGB
            double hueInDegrees = hue * 360;
            double C = saturation * value; // цветность
            double H = hueInDegrees / 60;
            double X = C * (1 - Math.Abs(H % 2 - 1));

            double r1, g1, b1;
            if (0 <= H && H < 1)
                (r1, g1, b1) = (C, X, 0);
            else if (1 <= H && H < 2)
                (r1, g1, b1) = (X, C, 0);
            else if (2 <= H && H < 3)
                (r1, g1, b1) = (0, C, X);
            else if (3 <= H && H < 4)
                (r1, g1, b1) = (0, X, C);
            else if (4 <= H && H < 5)
                (r1, g1, b1) = (X, 0, C);
            else /* 5 <= H && H < 6 */
                (r1, g1, b1) = (C, 0, X);

            double m = value - C;
            byte red = (byte)((r1 + m) * 255);
            byte green = (byte)((g1 + m) * 255);
            byte blue = (byte)((b1 + m) * 255);

            return new RGBColor(red, green, blue);
        }
    }
}
