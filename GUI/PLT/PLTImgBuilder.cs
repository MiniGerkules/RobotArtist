using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using GUI.Colors;
using GUI.Settings;

namespace GUI.PLT {
    internal class PLTImgBuilder : NotifierOfPropertyChange {
        private static readonly ScreenSizes windowSize = new();

        private static readonly int maxPercentForBuilding = 90;
        private static readonly int maxPercentForRendering = 10;
        public static int MaxPercent => maxPercentForBuilding + maxPercentForRendering;
        
        public AlgorithmSettings Settings { get; set; } = SettingsReader.ReadDefaultSettings();

        private int curPercent = 0;
        public int CurPercent {
            get => curPercent;
            private set {
                if (0 <= value && value <= MaxPercent) {
                    curPercent = value;
                    NotifyPropertyChanged(nameof(CurPercent));
                }
            }
        }

        public PLTPicture Build(PLTDecoderRes picture) {
            var bitmap = CreateBitmapOf(Settings, picture);
            if (bitmap.CanFreeze)
                bitmap.Freeze();

            return new(Settings, bitmap, picture);
        }

        public PLTPicture Rebuild(in AlgorithmSettings settings, in PLTPicture picture) {
            var bitmap = CreateBitmapOf(settings, picture.PLTDecoded);
            var newPicture = new PLTPicture(settings, bitmap, picture.PLTDecoded);
            newPicture.RestoreRotationAngle(picture);

            return newPicture;
        }

        public PLTPicture Rebuild(in PLTPicture picture) {
            return Rebuild(Settings, picture);
        }

        private BitmapSource CreateBitmapOf(in AlgorithmSettings settings, in PLTDecoderRes picture) {
            var image = ProcessStrokes(settings, picture);
            double scale = windowSize.CountScaling(picture.Width, picture.Height);
            return RenderBitmap(image, picture.Width, picture.Height, scale);
        }

        private BitmapSource RenderBitmap(DrawingVisual image, double width,
                                                 double height, double scalingFactor) {
            RenderTargetBitmap renderedImage = new(
                (int)(width * scalingFactor), (int)(height * scalingFactor),
                96, 96, PixelFormats.Default
            );
            renderedImage.Render(image);

            CurPercent += maxPercentForRendering;
            return renderedImage;
        }

        private DrawingVisual ProcessStrokes(in AlgorithmSettings settings,
                                             in PLTDecoderRes picture) {
            DrawingVisual image = new();
            DrawingContext context = image.RenderOpen();

            double scale = windowSize.CountScaling(picture.Width, picture.Height);
            SetBackground(settings, context, Brushes.White, picture.Width, picture.Height, scale);
            UpdateColor(settings, out var pen, out var geometry, picture.Strokes[0].StroceColor,
                        out var lastColor, scale);

            for (int i = 0; i < picture.Strokes.Count; ++i) {
                CurPercent = i*maxPercentForBuilding / picture.Strokes.Count;

                var stroke = picture.Strokes[i];
                if (picture.Strokes[i].StroceColor != lastColor) {
                    context.DrawGeometry(null, pen, geometry);
                    UpdateColor(settings, out pen, out geometry, stroke.StroceColor,
                                out lastColor, scale);
                }

                geometry.Children.Add(GetLineGeometry(stroke, scale));
            }

            context.DrawGeometry(null, pen, geometry);
            context.Close();

            CurPercent = maxPercentForBuilding;
            return image;
        }

        private static void SetBackground(in AlgorithmSettings settings, DrawingContext context,
                                          in Brush brush, in double width, in double height,
                                          in double scale) {
            double brushWidth = settings.BrushWidth * scale;
            Rect background = new(-brushWidth / 2, -brushWidth / 2,
                                  width*scale + brushWidth, height*scale + brushWidth);
            context.DrawRectangle(brush, null, background);
        }

        private static void UpdateColor(in AlgorithmSettings settings, out Pen pen,
                                        out GeometryGroup geometry, in PLTColor newColor,
                                        out PLTColor oldColor, in double scale) {
            UpdatePenAndGeometry(settings, out pen, out geometry,
                                 new SolidColorBrush(newColor.ToColor()), scale);
            oldColor = newColor;
        }

        private static void UpdatePenAndGeometry(in AlgorithmSettings settings, out Pen pen,
                                                 out GeometryGroup geometry, in Brush newColor,
                                                 in double scale) {
            geometry = new();
            pen = new() {
                Thickness = settings.BrushWidth * scale,
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                Brush = newColor
            };
        }

        private static LineGeometry GetLineGeometry(in Stroke stoke, in double scale) {
            Point start = new(stoke.Start.X * scale, stoke.Start.Y * scale);
            Point end = new(stoke.End.X * scale, stoke.End.Y * scale);

            return new(start, end);
        }
    }
}
