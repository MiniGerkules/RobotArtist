﻿using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using GUI.Colors;
using GUI.Settings;

namespace GUI.PLT {
    public class PLTImgBuilder : NotifierOfPropertyChange {
        private class BuildingImages {
            public DrawingVisual MainImage { get; } = new();
            public DrawingVisual StrokesStructure { get; } = new();
            public double Scale { get; }

            private readonly DrawingContext mainContext;
            private readonly DrawingContext strokesContext;

            private GeometryGroup geometry = new();
            private IColor? curColor = null;
            private readonly MyPen penWithRealColor = new();
            private readonly MyPen penForStrokesStruct = new();

            public BuildingImages(double scale) {
                MainImage = new();
                StrokesStructure = new();

                mainContext = MainImage.RenderOpen();
                strokesContext = StrokesStructure.RenderOpen();

                Scale = scale;
            }

            internal void Close() {
                mainContext.Close();
                strokesContext.Close();
            }

            internal void SetBackground(SolidColorBrush brush, double width, double height) {
                Rect background = new(0, 0, width*Scale, height*Scale);

                mainContext.DrawRectangle(brush, null, background);
                strokesContext.DrawRectangle(brush, null, background);
            }

            internal bool NeedUpdate(IColor color, double brushWidth) {
                return curColor != color || penWithRealColor.Thickness != brushWidth * Scale;
            }

            internal void UpdatePen(IColor color, double brushWidth) {
                geometry = new();

                if (curColor != color) {
                    curColor = color;
                    penWithRealColor.Brush = new SolidColorBrush(color.GetRealColor());
                    penForStrokesStruct.Brush = new SolidColorBrush(color.GetArtificialColor());
                }
                if (penWithRealColor.Thickness != brushWidth * Scale) {
                    penWithRealColor.Thickness = brushWidth * Scale;
                    penForStrokesStruct.Thickness = brushWidth * Scale;
                }
            }

            internal void AddToGeometry(Stroke stroke) {
                Point start = new(stroke.Start.X*Scale, stroke.Start.Y*Scale);
                Point end = new(stroke.End.X*Scale, stroke.End.Y*Scale);
                geometry.Children.Add(new LineGeometry(start, end));
            }

            internal void SaveGeometry() {
                mainContext.DrawGeometry(null, penWithRealColor.Pen, geometry);
                strokesContext.DrawGeometry(null, penForStrokesStruct.Pen, geometry);
            }
        }

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
        public void ResetCurPercent() { curPercent = 0; }

        public PLTPicture Build(in PLTDecoderRes picture) {
            var images = CreateImagesOf(Settings, picture);
            images.Freeze();
            return new(Settings, images, picture);
        }

        public PLTPicture Rebuild(in PLTPicture picture) {
            return Rebuild(Settings, picture);
        }

        public PLTPicture Rebuild(in AlgorithmSettings settings, in PLTPicture picture) {
            var images = CreateImagesOf(settings, picture.PLTDecoded);
            var newPicture = new PLTPicture(settings, images, picture.PLTDecoded);
            newPicture.RestoreRotationAngle(picture);

            return newPicture;
        }

        private Images CreateImagesOf(in AlgorithmSettings settings,
                                      in PLTDecoderRes decoded) {
            CMYBWColor.NumOfNeibForRegression = settings.NumOfNeibForHSVReg;
            double scale = windowSize.CountScaling(decoded.Width, decoded.Height);
            var builded = ProcessStrokes(settings, decoded, scale);

            var main = RenderBitmap(builded.MainImage, decoded, scale);
            CurPercent += maxPercentForRendering / 2;
            var strokes = RenderBitmap(builded.StrokesStructure, decoded, scale);
            CurPercent = maxPercentForBuilding + maxPercentForRendering;

            return new Images(main, strokes);
        }

        private static BitmapSource RenderBitmap(in DrawingVisual image, in PLTDecoderRes decoded,
                                                 in double scale) {
            RenderTargetBitmap renderedImage = new(
                (int)(decoded.Width * scale), (int)(decoded.Height * scale),
                96, 96, PixelFormats.Default
            );

            renderedImage.Render(image);
            return renderedImage;
        }

        private BuildingImages ProcessStrokes(in AlgorithmSettings settings,
                                              in PLTDecoderRes picture,
                                              in double scale) {
            BuildingImages builded = new(scale);
            builded.SetBackground(Brushes.White, picture.Width, picture.Height);
            builded.UpdatePen(picture.Strokes[0].StroceColor, picture.Strokes[0].BrushWidth);

            for (int i = 0; i < picture.Strokes.Count; ++i) {
                CurPercent = i*maxPercentForBuilding / picture.Strokes.Count;
                var stroke = picture.Strokes[i];

                if (builded.NeedUpdate(stroke.StroceColor, stroke.BrushWidth)) {
                    builded.SaveGeometry();
                    builded.UpdatePen(stroke.StroceColor, stroke.BrushWidth);
                }

                builded.AddToGeometry(stroke);
            }

            builded.SaveGeometry();
            builded.Close();

            CurPercent = maxPercentForBuilding;
            return builded;
        }
    }
}
