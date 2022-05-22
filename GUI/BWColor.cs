using System.Windows.Media;

namespace GUI
{
    /// <summary>
    /// The class describes the black and white color
    /// </summary>
    internal struct BWColor
    {
        private readonly byte brightness;

        public BWColor(byte brightness)
        {
            this.brightness = brightness;
        }

        public static explicit operator Brush(BWColor color)
        {
            Color gray = Color.FromRgb(color.brightness, color.brightness, color.brightness);
            return new SolidColorBrush(gray);
        }
    }
}
