using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

using GUI.PLT;
using GUI.Colors;
using GUI.Settings;

namespace GUI {
    internal class Picture {
        public BitmapSource RenderedPicture { get; private set; } = null;
        public AlgorithmSettings Settings { get; private set; }

        public double Width { get; private set; }
        public double Height { get; private set; }

        private readonly object mutex = new();
        private readonly PLTPicture pltPicture;

        private ushort angleOfRotation = 0;

        public Picture(AlgorithmSettings settings, PLTPicture picture) {
            Settings = settings;
            pltPicture = picture;
        }

        public void ProcessStrokes() {
            DrawingVisual image = BuildImage();
            RenderBitmap(image);

            if (RenderedPicture.CanFreeze)
                RenderedPicture.Freeze();
        }

        public void Redraw(AlgorithmSettings newSettings) {
            Settings = newSettings;

            var image = BuildImage();
            RenderBitmap(image);
            RestoreRotationAngle(angleOfRotation);

            if (RenderedPicture.CanFreeze)
                RenderedPicture.Freeze();
        }

        public void Rotate() {
            angleOfRotation = (ushort)((angleOfRotation + 90) % 360);
            RestoreRotationAngle(90);
        }

        private void SetBackground(DrawingContext context, Brush brush) {
            double brushWidth = Settings.BrushWidth;
            Rect background = new(-brushWidth / 2, -brushWidth / 2,
                                  Width + brushWidth, Height + brushWidth);
            context.DrawRectangle(brush, null, background);
        }

        private void RestoreRotationAngle(ushort angle) {
            RotateTransform rotate = new(angle);
            TransformedBitmap tb = new(RenderedPicture, rotate);
            RenderedPicture = tb;
        }

        private void RenderBitmap(DrawingVisual image) {
            //Rect bounds = image.ContentBounds;
            //RenderTargetBitmap renderedImage = new((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Default);
            int width = (int)SystemParameters.PrimaryScreenWidth;
            int height = (int)SystemParameters.PrimaryScreenHeight;
            RenderTargetBitmap renderedImage = new(width, height, 96, 96, PixelFormats.Default);

            renderedImage.Render(image);
            RenderedPicture = renderedImage;
        }

        private DrawingVisual BuildImage() {
            DrawingVisual image = new();
            DrawingContext context = image.RenderOpen();
            SetBackground(context, Brushes.White);
            UpdateColor(out var pen, out var geometry, pltPicture.Strokes[0].StroceColor, out var lastColor);

            lock (mutex) {
                for (int i = 0; i < pltPicture.Strokes.Count; ++i) {
                    var stroke = pltPicture.Strokes[i];
                    if (pltPicture.Strokes[i].StroceColor != lastColor) {
                        context.DrawGeometry(null, pen, geometry);
                        UpdateColor(out pen, out geometry, stroke.StroceColor, out lastColor);
                    }

                    geometry.Children.Add(GetLineGeometry(stroke));
                }

                context.DrawGeometry(null, pen, geometry);
            }

            context.Close();
            return image;
        }

        private void UpdateColor(out Pen pen, out GeometryGroup geometry,
                                 PLTColor newColor, out PLTColor oldColor) {
            UpdatePenAndGeometry(out pen, out geometry, new SolidColorBrush(newColor.ToColor()));
            oldColor = newColor;
        }

        private void UpdatePenAndGeometry(out Pen pen, out GeometryGroup geometry, Brush newColor) {
            geometry = new();
            pen = new() {
                Thickness = Settings.BrushWidth,
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                Brush = newColor
            };
        }

        private LineGeometry GetLineGeometry(Stroke stoke) {
            Point start = new(stoke.Start.X, stoke.Start.Y);
            Point end = new(stoke.End.X, stoke.End.Y);

            return new(start, end);
        }
    }
}
