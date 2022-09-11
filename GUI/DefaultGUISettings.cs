using System.Windows.Media;

namespace GUI {
    internal class DefaultGUISettings {
        //public static readonly SolidColorBrush inactive = new(Color.FromRgb(77, 147, 161));
        public static readonly SolidColorBrush menuColor = new(Color.FromRgb(136, 212, 227));
        public static readonly SolidColorBrush buttonColor = new(Color.FromRgb(255, 255, 255));
        public static readonly SolidColorBrush activeButton = new(Color.FromRgb(189, 242, 252));
        public static readonly SolidColorBrush inactiveButton = menuColor;

        public static readonly int FontSize = 16;
    }
}
