using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
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

        public double Width { get; private set; }
        public double Height { get; private set; }

        public Picture()
        {
            RenderedPicture = null;
            Strokes = new();
        }

        public void ProcessStrokes(List<Stroke> strokes, double width, double height)
        {
            Width = width;
            Height = height;
            DrawingVisual image = BuildImage(strokes);
            RenderBitmap(image);
            strokes.Clear();
        }

        public void Redraw(ImmutableDictionary<PossibleSettings, double> newSettings)
        {
            UpdateSettings(newSettings);

            DrawingVisual image = new();
            DrawingContext context = image.RenderOpen();
            SetBackground(context, Brushes.White);

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

        public ImmutableDictionary<PossibleSettings, double> GetActualSettings()
        {
            return new Dictionary<PossibleSettings, double>()
            {
                { PossibleSettings.brushWidth, penThikness }
            }.ToImmutableDictionary();
        }

        private void SetBackground(DrawingContext context, Brush brush)
        {
            Rect background = new(-penThikness/2, -penThikness/2, Width + penThikness, Height + penThikness);
            context.DrawRectangle(brush, null, background);
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
            DrawingContext context = image.RenderOpen();

            SetBackground(context, Brushes.White);
            UpdateColor(out Pen pen, out var geometry, strokes[0].StroceColor, out var lastColor);
            for (int i = 0; i < strokes.Count; ++i)
            {
                if (strokes[i].StroceColor != lastColor)
                {
                    context.DrawGeometry(null, pen, geometry);
                    Strokes.Add((geometry, pen));
                    UpdateColor(out pen, out geometry, strokes[i].StroceColor, out lastColor);
                }

                geometry.Children.Add(GetLineGeometry(strokes[i]));
            }

            context.DrawGeometry(null, pen, geometry);
            Strokes.Add((geometry, pen));
            
            context.Close();
            return image;
        }

        private void UpdateColor(out Pen pen, out GeometryGroup geometry,
                                 CMYBWColor newColor, out CMYBWColor oldColor)
        {
            UpdatePenAndGeometry(out pen, out geometry, (Brush)newColor);
            oldColor = newColor;
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
