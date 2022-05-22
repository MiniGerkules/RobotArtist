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

        private readonly Dictionary<FileName, RotatedBitmap> files = new();
        private FileName activeFile = null;

        private readonly PLTDecoder pltDecoder = new();
        private uint penThikness = 4;

        public MainWindow()
        {
            InitializeComponent();

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
                PaintBitmap();
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
            PaintStrokes(strokes, pltDecoder.MaxX, pltDecoder.MaxY);
            strokes.Clear();
        }

        private void ImageFileHandler(string fileName)
        {
            throw new NotImplementedException();
        }

        private void PaintStrokes(List<Stroke> strokes, uint maxX, uint maxY)
        {
            DrawingVisual image = BuildImage(strokes);

            Rect bounds = image.ContentBounds;
            RenderTargetBitmap renderedImage = new((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Default);
            renderedImage.Render(image);
            files[activeFile] = new(renderedImage);

            PaintBitmap();
        }

        private DrawingVisual BuildImage(List<Stroke> strokes)
        {
            DrawingVisual image = new();
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
                    UpdatePenAndGeometry(out pen, out geometry, newBrush);
                }

                line = GetLineGeometry(strokes[i]);
                geometry.Children.Add(line);
            }

            drawingContext.DrawGeometry(null, pen, geometry);
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

        private void PaintBitmap()
        {
            //ImageBrush imageBrush = new(bitmap);
            //imageBrush.Stretch = Stretch.Uniform;
            //outputImage.Background = imageBrush;

            RotateTransform rotate = new(files[activeFile].Angle);
            TransformedBitmap tb = new(files[activeFile].Bitmap, rotate);

            outputImage.Source = tb;
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
            PaintBitmap();
        }

        private void RepaintImage(object sender, RoutedEventArgs e)
        {
            PLTFileHandler(activeFile.FullName);
        }
    }
}
