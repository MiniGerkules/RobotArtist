using System;
using System.Linq;
using System.Windows.Media;

namespace GUI.Colors {
    internal class RGBColor : IColor {
        private byte red, green, blue;

        public RGBColor(string[] colors) {
            (red, green, blue) = colors.Select(elem => byte.Parse(elem)).ToArray() switch {
                var arr when arr.Length == 4 => (arr[1], arr[2], arr[3]),
                _ => throw new ArgumentException("There isn't correct number of the colors!" +
                                                 $"Get {colors.Length}, expected 4!")
            };
        }

        public RGBColor(byte red, byte green, byte blue) {
            (this.red, this.green, this.blue) = (red, green, blue);
        }

        public Color GetRealColor() {
            return Color.FromRgb(red, green, blue);
        }

        public Color GetArtificialColor() {
            Random rand = new();
            var rgb = new byte[3];

            rand.NextBytes(rgb);
            return Color.FromRgb(rgb[0], rgb[1], rgb[2]);
        }
    }
}
