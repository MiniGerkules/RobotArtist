using System;
using System.IO;
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

        //public static readonly SolidColorBrush inactive = new(Color.FromRgb(77, 147, 161));
        public static readonly SolidColorBrush menuColor = new(Color.FromRgb(136, 212, 227));
        public static readonly SolidColorBrush buttonColor = new(Color.FromRgb(255, 255, 255));
        public static readonly SolidColorBrush activeButton = new(Color.FromRgb(189, 242, 252));
        public static readonly SolidColorBrush inactiveButton = menuColor;

        private readonly Dictionary<FileName, Picture> files = new();
        private readonly Dictionary<FileName, UIElement> tabs = new();
        private readonly Dictionary<FileName, string> savedFiles = new();
        private FileName activeFile = null;

        private readonly PLTDecoder pltDecoder = new();
        private readonly Settings GUISettings;

        private readonly string pathToDatabase = @"C:\ModelTable600.xls";

        public MainWindow()
        {
            InitializeComponent();

            GUISettings = new(settingsFields);
            GUISettings.applySettings += ApplySettings;
            mainMenu.Background = menuColor;
            SetInactive();

            CommandBinding commandBinding = new(ApplicationCommands.Open, OpenFile);
            CommandBindings.Add(commandBinding);
            openButton.CommandBindings.Add(commandBinding);
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            Database.LoadDatabase(pathToDatabase);

            if (!Database.IsLoad())
            {
                MessageBox.Show("Can't to upload a file with color data. Check the presence " +
                    "of the database file in the directory.", "Error!", MessageBoxButton.OK);
                Close();
            }
        }

        private void OpenFile(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new();
            fileDialog.Filter = "Picture or PLT-file|*.jpg;*.png;*.bmp;*.plt";

            if (fileDialog.ShowDialog() == false)
                MessageBox.Show("Can't open file!", "Error!", MessageBoxButton.OK);
            else
                ProcessFile(fileDialog.FileName);
        }

        private void ProcessFile(string fileName)
        {
            FileName file = new(fileName);
            if (IsFileAlreadyOpened(file))
                return;

            //try
            //{
                if (fileName.EndsWith(".plt"))
                    PLTFileHandler(fileName);
                else
                    ImageFileHandler(fileName);
            //}
            //catch (ArgumentException error)
            //{
            //    MessageBox.Show($"Can't process file!\n{error.Message}",
            //                    "Error!", MessageBoxButton.OK);
            //    return;
            //}

            AddNewOpenedFile(file);
            ChangeActive(Active.ViewGrid);
            DisplayActiveBitmap(viewImage);
        }
        
        private bool IsFileAlreadyOpened(FileName file)
        {
            if (files.ContainsKey(file))
            {
                UpdateOutputImage(file);
                return true;
            }

            return false;
        }

        private void AddNewOpenedFile(FileName fileName)
        {
            OpenedFile file = new(fileName);
            file.Background = buttonColor;
            file.Click += ChangeFile;
            openedFiles.Children.Add(file);
            tabs[activeFile] = openedFiles.Children[^1];
        }

        private void ChangeFile(object sender, EventArgs e)
        {
            OpenedFile clicked = (OpenedFile)sender;
            if (clicked.FileName.FullName == activeFile.FullName)
                return;

            UpdateOutputImage(clicked.FileName);
        }

        private void UpdateOutputImage(FileName fileName)
        {
            ChangeActive(Active.ViewGrid);
            activeFile = fileName;
            DisplayActiveBitmap(viewImage);
        }

        private void PLTFileHandler(string fileName)
        {
            List<Stroke> strokes = pltDecoder.Decode(fileName);

            Picture picture = new();
            picture.ProcessStrokes(strokes, pltDecoder.MaxX, pltDecoder.MaxY);

            activeFile = new(fileName);
            files[activeFile] = picture;
        }

        private void ImageFileHandler(string fileName)
        {
            BitmapImage image = new(new Uri(fileName));
            tracer = new(new(image, new()));
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
            if (activeFile == null || (viewButton.Background as SolidColorBrush).Color == activeButton.Color)
                return;

            ChangeActive(Active.ViewGrid);
            DisplayActiveBitmap(viewImage);
        }
        
        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            if (activeFile == null || (settingsButton.Background as SolidColorBrush).Color == activeButton.Color)
                return;

            ChangeActive(Active.SettingsGrid);
            DisplayActiveBitmap(settingsImage);
            GUISettings.DisplaySettings(
                new()
                {
                    { PossibleSettings.itersMinOverlap, 1 },
                    { PossibleSettings.minOverlap, 0.6 },
                    { PossibleSettings.maxOverlap, 1 },
                    { PossibleSettings.pixTol, 6 },
                    { PossibleSettings.pixTol2, 100 },
                    { PossibleSettings.pixTolBest, 4 },
                    { PossibleSettings.maxLen, files[activeFile].penThikness * 10 },
                    { PossibleSettings.brushWidth, files[activeFile].penThikness },
                }
            );
        }

        private void InfoClick(object sender, RoutedEventArgs e)
        {
            if (activeFile == null || (infoButton.Background as SolidColorBrush).Color == activeButton.Color)
                return;

            ChangeActive(Active.InfoGreed);
            var settings = files[activeFile].GetActualSettings();
            List<UIElement> elements = new(settings.Count);

            string width = "Width (mm) = " + files[activeFile].Width.ToString();
            string height = "Height (mm) = " + files[activeFile].Height.ToString();
            elements.Add(Helpers.CreateTextBlock(width, HorizontalAlignment.Center, new()));
            elements.Add(Helpers.CreateTextBlock(height, HorizontalAlignment.Center, new()));

            foreach (var pair in settings)
            {
                string text = pair.Key.GetDescription() + " = " + pair.Value.ToString();
                elements.Add(Helpers.CreateTextBlock(text, HorizontalAlignment.Center, new()));
            }

            GridDisplayer displayer = new(infoGrid);
            displayer.Reset();
            displayer.DisplayElemByRow(elements);
        }

        private void ChangeActive(Active active)
        {
            switch (active)
            {
                case Active.ViewGrid:
                    viewGrid.Visibility = Visibility.Visible;
                    settingsGrid.Visibility = Visibility.Collapsed;
                    infoGrid.Visibility = Visibility.Collapsed;
                    viewButton.Background = activeButton;
                    settingsButton.Background = inactiveButton;
                    infoButton.Background = inactiveButton;
                    break;
                case Active.SettingsGrid:
                    viewGrid.Visibility = Visibility.Collapsed;
                    settingsGrid.Visibility = Visibility.Visible;
                    infoGrid.Visibility = Visibility.Collapsed;
                    viewButton.Background = inactiveButton;
                    settingsButton.Background = activeButton;
                    infoButton.Background = inactiveButton;
                    break;
                case Active.InfoGreed:
                    viewGrid.Visibility = Visibility.Collapsed;
                    settingsGrid.Visibility = Visibility.Collapsed;
                    infoGrid.Visibility = Visibility.Visible;
                    viewButton.Background = inactiveButton;
                    settingsButton.Background = inactiveButton;
                    infoButton.Background = activeButton;
                    break;
            }
        }

        private void SetInactive()
        {
            activeFile = null;
            
            viewGrid.Visibility = Visibility.Visible;
            settingsGrid.Visibility = Visibility.Collapsed;
            infoGrid.Visibility = Visibility.Collapsed;
            
            viewButton.Background = inactiveButton;
            settingsButton.Background = inactiveButton;
            infoButton.Background= inactiveButton;

            viewImage.Source = null;
            settingsImage.Source = null;
        }

        private void ApplySettings(ImmutableDictionary<PossibleSettings, double> settedSettings, bool changed)
        {
            if (changed)
            {
                files[activeFile].Redraw(settedSettings);
                DisplayActiveBitmap(settingsImage);
            }
        }

        private void CloseActiveFile(object sender, RoutedEventArgs e)
        {
            if (activeFile != null)
            {
                files.Remove(activeFile);
                openedFiles.Children.Remove(tabs[activeFile]);
                if (openedFiles.Children.Count != 0)
                {
                    activeFile = ((OpenedFile)openedFiles.Children[0]).FileName;
                    if ((viewButton.Background as SolidColorBrush).Color == activeButton.Color)
                        DisplayActiveBitmap(viewImage);
                    else
                        DisplayActiveBitmap(settingsImage);
                }
                else
                {
                    SetInactive();
                }
            }
        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveFileClick(object sender, RoutedEventArgs e)
        {
            if (activeFile == null)
                return;

            if (savedFiles.ContainsKey(activeFile))
            {
                if (activeFile.ShortName.EndsWith(".plt"))
                    SaveImageToFile(files[activeFile].RenderedPicture, savedFiles[activeFile]);
                else
                    SavePLTToFile(files[activeFile].RenderedPicture, savedFiles[activeFile]);
            }
            else
            {
                SaveFileAsClick(sender, e);
            }
        }

        private void SaveFileAsClick(object sender, RoutedEventArgs e)
        {
            if (activeFile == null)
                return;

            Action<BitmapSource, string> call;
            SaveFileDialog dlg = new();
            dlg.FileName = Helpers.GetFileNameWithoutExt(activeFile.ShortName);

            if (activeFile.ShortName.EndsWith(".plt"))
            {
                dlg.DefaultExt = ".png";
                dlg.Filter = "Picture|*.jpg;*.png;*.bmp";
                call = SaveImageToFile;
            }
            else
            {
                dlg.DefaultExt = ".plt";
                dlg.Filter = "PLT-file|*.plt";
                call = SavePLTToFile;
            }

            if (dlg.ShowDialog() != true)
                MessageBox.Show("Couldn't select the path to save the file!", "Error!", MessageBoxButton.OK);
            else
            {
                savedFiles[activeFile] = dlg.FileName;
                call(files[activeFile].RenderedPicture, dlg.FileName);
            }
        }

        private void SaveImageToFile(BitmapSource image, string filePath)
        {
            BitmapEncoder encoder = filePath[^3..] switch
            {
                "png" => new PngBitmapEncoder(),
                "bmp" => new BmpBitmapEncoder(),
                _ => new JpegBitmapEncoder()
            };

            ScaleTransform mirrow = new ScaleTransform(1, -1);
            TransformedBitmap tb = new TransformedBitmap(image.Clone(), mirrow);
            encoder.Frames.Add(BitmapFrame.Create(tb));
            using FileStream stream = new(filePath, FileMode.Create);
            encoder.Save(stream);
        }

        private void SavePLTToFile(BitmapSource image, string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
