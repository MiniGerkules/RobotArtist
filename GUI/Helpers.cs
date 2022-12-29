using System.Windows;
using System.Windows.Controls;

namespace GUI {
    internal static class Helpers {
        public static TextBlock CreateTextBlock(string text, HorizontalAlignment alignment, Thickness margin) {
            TextBlock label = new() {
                Text = text,
                FontSize = DefaultGUISettings.FontSize,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = alignment,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = margin,
            };

            return label;
        }
    }
}
