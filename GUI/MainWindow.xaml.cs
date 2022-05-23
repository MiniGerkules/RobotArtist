using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Collections.Immutable;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Algorithm.Settings settings;
        private Algorithm.Tracer tracer;

        private readonly List<Vector> gradients = new();
        private readonly List<Vector> strokes = new();

        private static readonly SolidColorBrush active = Brushes.Green;
        private static readonly SolidColorBrush inactive = Brushes.LightBlue;

        private readonly Dictionary<FileName, RotatedBitmap> files = new();
        private FileName activeFile = null;

        private readonly PLTDecoder pltDecoder = new();
        private readonly List<(GeometryGroup, Pen)> picture = new();

        private readonly Settings GUISettings;

        private double penThikness = 4;

        public MainWindow()
        {
            InitializeComponent();

            GUISettings = new(settingsFields);
            GUISettings.applySettings += ApplySettings;
            viewGrid.Visibility = Visibility.Visible;
            settingsGrid.Visibility = Visibility.Collapsed;

            CommandBinding commandBinding = new(ApplicationCommands.Open, OpenFile);
            CommandBindings.Add(commandBinding);
            openButton.CommandBindings.Add(commandBinding);
        }

        private void OpenFile(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new();
            fileDialog.Filter = "Picture or PLT|*.jpg;*.png;*.bmp;*.plt";

            if (fileDialog.ShowDialog() == false)
            {
                MessageBox.Show("Can't open file!");
                return;
            }

            if (!AddNewOpenedFile(fileDialog.FileName))
                return;

            viewButton.Background = active;
            if (fileDialog.FileName.EndsWith(".plt"))
                PLTFileHandler(fileDialog.FileName);
            else
                ImageFileHandler(fileDialog.FileName);
        }

        private bool AddNewOpenedFile(string fullFilePath)
        {
            if (files.FirstOrDefault(elem => elem.Key.FullName == fullFilePath, new(null, null)).Key != null)
            {
                UpdateOutputImage(fullFilePath);
                return false;
            }

            string shortName = Helpers.GetFileName(fullFilePath);
            OpenedFile file = new(fullFilePath, shortName);
            file.Click += ChangeFile;
            openedFiles.Children.Add(file);

            activeFile = new(fullFilePath, shortName);
            files.Add(activeFile, null);

            return true;
        }

        private void ChangeFile(object sender, EventArgs e)
        {
            OpenedFile clicked = (OpenedFile)sender;
            if (clicked.FullName == activeFile.FullName)
                return;

            UpdateOutputImage((string)clicked.FullName);
        }

        private void UpdateOutputImage(string newActiveFileFullName)
        {
            try
            {
                activeFile = files.First(elem => elem.Key.FullName == newActiveFileFullName).Key;
                PaintActiveBitmap(outputImage);
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException($"The user don't open file {newActiveFileFullName}");
            }
        }

        private void PLTFileHandler(string fileName)
        {
            string text = File.ReadAllText(fileName);
            List<Stroke> strokes = pltDecoder.Decode(text);
            PaintStrokes(strokes);
            strokes.Clear();
        }

        private void ImageFileHandler(string fileName)
        {
            throw new NotImplementedException();
        }

        private void PaintStrokes(List<Stroke> strokes)
        {
            DrawingVisual image = BuildImage(strokes);
            RenderActiveBitmap(image);
            PaintActiveBitmap(outputImage);
        }

        private void RenderActiveBitmap(DrawingVisual image)
        {
            Rect bounds = image.ContentBounds;
            RenderTargetBitmap renderedImage = new((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Default);
            renderedImage.Render(image);
            files[activeFile] = new(renderedImage);
        }

        private DrawingVisual BuildImage(List<Stroke> strokes)
        {
            DrawingVisual image = new();
            picture.Capacity = strokes.Count;
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
                    picture.Add((geometry, pen));
                    UpdatePenAndGeometry(out pen, out geometry, newBrush);
                }

                line = GetLineGeometry(strokes[i]);
                geometry.Children.Add(line);
            }

            drawingContext.DrawGeometry(null, pen, geometry);
            picture.Add((geometry, pen));
            drawingContext.Close();
            return image;
        }

        private LineGeometry GetLineGeometry(Stroke stoke)
        {
            Point start = new(stoke.Start.X, stoke.Start.Y);
            Point end = new(stoke.End.X, stoke.End.Y);

            return new(start, end);
        }

        private void UpdatePenAndGeometry(out Pen pen, out GeometryGroup geometry, Brush brush)
        {
            geometry = new();
            pen = new()
            {
                Thickness = penThikness,
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                Brush = brush
            };
        }

        private void PaintActiveBitmap(Image image)
        {
            //ImageBrush imageBrush = new(bitmap);
            //imageBrush.Stretch = Stretch.Uniform;
            //outputImage.Background = imageBrush;

            RotateTransform rotate = new(files[activeFile].Angle);
            TransformedBitmap tb = new(files[activeFile].Bitmap, rotate);

            image.Source = tb;
        }

        /// <summary>
        /// Event handler for the process of adding new strokes to the map
        /// </summary>
        /// <param name="sender"> The object sending new strokes </param>
        /// <param name="vectors"> New strokes to add to the map </param>
        private void AddNewVectors(object sender, List<Vector> vectors)
        {
            if (vectors == null || vectors.Count == 0)
                return;

            strokes.AddRange(vectors);
            // Paint new vectors
        }

        /// <summary>
        /// Event handler for the process of adding new gradients to the map
        /// </summary>
        /// <param name="sender"> The object sending new gradients </param>
        /// <param name="vectors"> New gradients to add to the map </param>
        private void AddNewGradients(object sender, List<Vector> vectors)
        {
            if (vectors == null || vectors.Count == 0)
                return;

            gradients.AddRange(vectors);
            // Paint new vectors
        }

        private void RotateImage(object sender, RoutedEventArgs e)
        {
            //var (centerX, centerY) = Helpers.GetCenter(outputImage.ActualWidth, outputImage.ActualHeight);
            files[activeFile].Rotate();
            PaintActiveBitmap(outputImage);
        }

        private void RepaintImage(object sender, RoutedEventArgs e)
        {
            PLTFileHandler(activeFile.FullName);
        }

        private void ViewClick(object sender, RoutedEventArgs e)
        {
            if (activeFile == null || (viewButton.Background as SolidColorBrush).Color == active.Color)
                return;

            ChangeActive(true);
            PictureSettingsChanged(outputImage);
        }
        
        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            if (activeFile == null || (settingsButton.Background as SolidColorBrush).Color == active.Color)
                return;

            ChangeActive(false);
            PaintActiveBitmap(previewImage);
            GUISettings.DisplaySettings(
                new()
                {
                    { PossibleSettings.itersMinOverlap, ("Количество итераций с малым перекрытием", 1) },
                    { PossibleSettings.minOverlap, ("Минимальный начальный коэффициент перекрытия", 0.6) },
                    { PossibleSettings.maxOverlap, ("Максимальный начальный коэффициент перекрытия", 1) },
                    { PossibleSettings.pixTol, ("Возможное отклонение цвета от исходника на конце", 6) },
                    { PossibleSettings.pixTol2, ("Возможное отклонение цвета от исходника в среднем", 100) },
                    { PossibleSettings.pixTolBest, ("Погрешность, при которой мазок безоговорочно принят", 4) },
                    { PossibleSettings.maxLen, ("Максимальная длина мазка", penThikness * 10) },
                    { PossibleSettings.brushWidth, ("Толщина кисти", penThikness) },
                }
            );
        }

        private void ChangeActive(bool view)
        {
            if (view)
            {
                settingsGrid.Visibility = Visibility.Collapsed;
                viewGrid.Visibility = Visibility.Visible;
                viewButton.Background = active;
                settingsButton.Background = inactive;
            }
            else
            {
                settingsGrid.Visibility = Visibility.Visible;
                viewGrid.Visibility = Visibility.Collapsed;
                viewButton.Background = inactive;
                settingsButton.Background = active;
            }
        }

        private void PictureSettingsChanged(Image imageToOut)
        {
            DrawingVisual image = new();
            DrawingContext context = image.RenderOpen();

            foreach (var (geometry, pen) in picture)
            {
                pen.Thickness = penThikness;
                context.DrawGeometry(null, pen, geometry);
            }

            context.Close();
            RenderActiveBitmap(image);
            PaintActiveBitmap(imageToOut);
        }

        private void ApplySettings(ImmutableDictionary<PossibleSettings, double> settedSettings)
        {
            penThikness = settedSettings[PossibleSettings.brushWidth];
            PictureSettingsChanged(previewImage);
        }
    }
}
