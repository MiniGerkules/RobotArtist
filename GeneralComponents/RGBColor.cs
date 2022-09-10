using System.Linq;
using System;
using System.Windows.Media;

namespace GeneralComponents
{
    internal class RGBColor : PLTColor
    {
        private byte red, green, blue;

        public RGBColor(string[] colors)
        {
            (red, green, blue) = colors.Select(elem => byte.Parse(elem)).ToArray() switch
            {
                var arr when (arr.Length == 4) => (arr[1], arr[2], arr[3]),
                _ => throw new ArgumentException("There isn't correct number of the colors!" +
                                                 $"Get {colors.Length}, expected 4!")
            };
        }

        public RGBColor(byte red, byte green, byte blue)
        {
            (this.red, this.green, this.blue) = (red, green, blue);
        }

        public override Color ToColor()
        {
            return Color.FromRgb(red, green, blue);
        }
    }
}
