using System;
using System.Windows;
using System.Windows.Controls;

namespace GUI
{
    internal static class Helpers
    {
        public static double GetMaxCoefficient(double maxXPic, double maxYPic, double maxXDisp, double maxYDisp)
        {
            double k1 = maxXPic / maxXDisp;
            double k2 = maxYPic / maxYDisp;

            return Math.Max(k1, k2);
        }

        public static (double, double) GetCenter(double width, double height)
        {
            return (width / 2, height / 2);
        }

        /// <summary>
        /// The method finds the canvas center. Takes into account the size of the placed picture
        /// </summary>
        /// <param name="canWidth"> canvas width </param>
        /// <param name="canHeight"> canvas height </param>
        /// <param name="picWidth"> picture width  </param>
        /// <param name="picHeight"> picture height </param>
        /// <returns> Point2D where the center is located </returns>
        public static Point2D GetCenter(double canWidth, double canHeight, double picWidth, double picHeight)
        {
            var (centerX, centerY) = GetCenter(canWidth - picWidth, canHeight - picHeight);

            return new((uint)centerX, (uint)centerY);
        }

        public static string GetFileName(string fullFilePath)
        {
            string fileName = new(fullFilePath);

            int index = fileName.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
            if (index != -1)
                fileName = fileName[(index + 1)..];

            return fileName;
        }

        public static string GetFileNameWithoutExt(string shortFileName)
        {
            string fileName = new(shortFileName);

            int index = fileName.LastIndexOf('.');
            if (index != -1)
                fileName = fileName[..index];

            return fileName;
        }

        public static TextBlock CreateTextBlock(string text, HorizontalAlignment alignment, Thickness margin)
        {
            TextBlock label = new();
            label.Text = text;
            label.FontSize = 16;
            label.TextWrapping = TextWrapping.Wrap;
            label.HorizontalAlignment = alignment;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.Margin = margin;

            return label;
        }
    }
}
