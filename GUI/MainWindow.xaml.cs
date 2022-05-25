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

        public static readonly SolidColorBrush active = new(Color.FromRgb(189, 242, 252));
        public static readonly SolidColorBrush inactive = new(Color.FromRgb(77, 147, 161));
        public static readonly SolidColorBrush menuColor = new(Color.FromRgb(136, 212, 227));
        public static readonly SolidColorBrush buttonColor = new(Color.FromRgb(255, 255, 255));

        private readonly Dictionary<FileName, Picture> files = new();
        private FileName activeFile = null;

        private readonly PLTDecoder pltDecoder = new();
        private readonly Settings GUISettings;

        public MainWindow()
        {
            InitializeComponent();

            GUISettings = new(settingsFields);
            GUISettings.applySettings += ApplySettings;
            viewGrid.Visibility = Visibility.Visible;
            settingsGrid.Visibility = Visibility.Collapsed;
            mainMenu.Background = menuColor;

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

            ChangeActive(true);
            if (fileDialog.FileName.EndsWith(".plt"))
                PLTFileHandler(fileDialog.FileName);
            else
                ImageFileHandler(fileDialog.FileName);

            DisplayActiveBitmap(viewImage);
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
            file.Background = buttonColor;
            file.Click += ChangeFile;
            openedFiles.Children.Add(file);

            activeFile = new(fullFilePath, shortName);
            files.Add(activeFile, new());

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
                DisplayActiveBitmap(viewImage);
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
            files[activeFile].ProcessStrokes(strokes);
        }

        private void ImageFileHandler(string fileName)
        {
            throw new NotImplementedException();
        }

        private void DisplayActiveBitmap(Image image)
        {
            //ImageBrush imageBrush = new(bitmap);
            //imageBrush.Stretch = Stretch.Uniform;
            //outputImage.Background = imageBrush;

            image.Source = files[activeFile].RenderedPicture;
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
            DisplayActiveBitmap(viewImage);
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
            DisplayActiveBitmap(viewImage);
        }
        
        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            if (activeFile == null || (settingsButton.Background as SolidColorBrush).Color == active.Color)
                return;

            ChangeActive(false);
            DisplayActiveBitmap(settingsImage);
            GUISettings.DisplaySettings(
                new()
                {
                    { PossibleSettings.itersMinOverlap, ("Number of iterations", 1) },
                    { PossibleSettings.minOverlap, ("Minimum overlap coefficient", 0.6) },
                    { PossibleSettings.maxOverlap, ("Maximum overlap coefficient", 1) },
                    { PossibleSettings.pixTol, ("Possible color deviation at the end", 6) },
                    { PossibleSettings.pixTol2, ("Possible color deviation on average", 100) },
                    { PossibleSettings.pixTolBest, ("The error of taking a smear", 4) },
                    { PossibleSettings.maxLen, ("Maximum stroke length", files[activeFile].penThikness * 10) },
                    { PossibleSettings.brushWidth, ("Brush thickness", files[activeFile].penThikness) },
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

        private void ApplySettings(ImmutableDictionary<PossibleSettings, double> settedSettings, bool changed)
        {
            if (changed)
            {
                files[activeFile].Redraw(settedSettings);
                DisplayActiveBitmap(settingsImage);
            }
        }
    }
}
