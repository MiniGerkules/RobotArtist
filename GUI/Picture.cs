using System.Windows.Media;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Collections.Immutable;

namespace GUI
{
    internal class Picture
    {
        public List<(GeometryGroup, Pen)> Strokes { get; private set; }
        public BitmapSource RenderedPicture { get; private set; }
        public double penThikness { get; private set; } = 4;
        private ushort angle = 0;

        public Picture()
        {
            RenderedPicture = null;
            Strokes = new();
        }

        public void ProcessStrokes(List<Stroke> strokes)
        {
            DrawingVisual image = BuildImage(strokes);
            RenderBitmap(image);
            strokes.Clear();
        }

        public void Redraw(ImmutableDictionary<PossibleSettings, double> newSettings)
        {
            UpdateSettings(newSettings);

            DrawingVisual image = new();
            DrawingContext context = image.RenderOpen();
            foreach (var (geometry, pen) in Strokes)
            {
                pen.Thickness = penThikness;
                context.DrawGeometry(null, pen, geometry);
            }

            context.Close();
            RenderBitmap(image);
            RestoreRotationAngle(angle);
        }

        public void Rotate()
        {
            angle = (ushort)((angle + 90) % 360);
            RestoreRotationAngle(90);
        }

        private void RestoreRotationAngle(ushort angle)
        {
            RotateTransform rotate = new(angle);
            TransformedBitmap tb = new(RenderedPicture, rotate);
            RenderedPicture = tb;
        }

        private void UpdateSettings(ImmutableDictionary<PossibleSettings, double> newSettings)
        {
            penThikness = newSettings[PossibleSettings.brushWidth];
        }

        private void RenderBitmap(DrawingVisual image)
        {
            Rect bounds = image.ContentBounds;
            RenderTargetBitmap renderedImage = new((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Default);
            renderedImage.Render(image);
            RenderedPicture = renderedImage;
        }

        private DrawingVisual BuildImage(List<Stroke> strokes)
        {
            DrawingVisual image = new();
            Strokes.Capacity = strokes.Count;
            DrawingContext drawingContext = image.RenderOpen();

            LineGeometry line = GetLineGeometry(strokes[0]);
            UpdatePenAndGeometry(out Pen pen, out GeometryGroup geometry, (Brush)(BWColor)strokes[0].StroceColor);
            geometry.Children.Add(line);

            for (int i = 1; i < strokes.Count; ++i)
            {
                SolidColorBrush newBrush = (Brush)(BWColor)strokes[i].StroceColor as SolidColorBrush;
                if (newBrush.Color != (pen.Brush as SolidColorBrush).Color)
                {
                    drawingContext.DrawGeometry(null, pen, geometry);
                    Strokes.Add((geometry, pen));
                    UpdatePenAndGeometry(out pen, out geometry, newBrush);
                }

                line = GetLineGeometry(strokes[i]);
                geometry.Children.Add(line);
            }

            drawingContext.DrawGeometry(null, pen, geometry);
            Strokes.Add((geometry, pen));
            drawingContext.Close();
            return image;
        }

        private void UpdatePenAndGeometry(out Pen pen, out GeometryGroup geometry, Brush newColor)
        {
            geometry = new();
            pen = new()
            {
                Thickness = penThikness,
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                Brush = newColor
            };
        }

        private LineGeometry GetLineGeometry(Stroke stoke)
        {
            Point start = new(stoke.Start.X, stoke.Start.Y);
            Point end = new(stoke.End.X, stoke.End.Y);

            return new(start, end);
        }
    }
}
