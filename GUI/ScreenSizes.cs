using System;
using System.Windows;

namespace GUI {
    public class ScreenSizes {
        private readonly double width;
        private readonly double height;

        public ScreenSizes() {
            width = SystemParameters.PrimaryScreenWidth;
            height = SystemParameters.PrimaryScreenHeight;
        }

        public double CountScaling(double guiObjWidth, double guiObjHeight) {
            double widthRatio = width / guiObjWidth;
            double heightRatio = height / guiObjHeight;

            return Math.Min(widthRatio, heightRatio);
        }
    }
}