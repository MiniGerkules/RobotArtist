using System;

namespace GUI {
    public class WindowSize {
        private readonly double windowWidth;
        private readonly double windowHeight;

        public WindowSize(double windowWidth, double windowHeight) {
            (this.windowWidth, this.windowHeight) = (windowWidth, windowHeight);
        }

        public double CountScaling(double guiObjWidth, double guiObjHeight) {
            double widthRatio = windowWidth / guiObjWidth;
            double heightRatio = windowHeight / guiObjHeight;

            return Math.Min(widthRatio, heightRatio);
        }
    }
}