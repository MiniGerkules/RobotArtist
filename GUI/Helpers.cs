using System.Windows;
using System.Windows.Controls;

namespace GUI {
    internal static class Helpers {
        public static string GetFileName(string fullFilePath) {
            string fileName = new(fullFilePath);

            int index = fileName.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
            if (index != -1)
                fileName = fileName[(index + 1)..];

            return fileName;
        }

        public static string GetFileNameWithoutExt(string shortFileName) {
            string fileName = new(shortFileName);

            int index = fileName.LastIndexOf('.');
            if (index != -1)
                fileName = fileName[..index];

            return fileName;
        }

        public static TextBlock CreateTextBlock(string text, HorizontalAlignment alignment, Thickness margin) {
            TextBlock label = new();
            label.Text = text;
            label.FontSize = DefaultGUISettings.FontSize;
            label.TextWrapping = TextWrapping.Wrap;
            label.HorizontalAlignment = alignment;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.Margin = margin;

            return label;
        }
    }
}
