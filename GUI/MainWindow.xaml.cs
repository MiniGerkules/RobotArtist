using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace GUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private Algorithm.Tracer tracer;

        private readonly Dictionary<string, Picture> files = new();
        private readonly Dictionary<string, UIElement> tabs = new();
        private readonly Dictionary<string, string> savedFiles = new();
        private string pathToActiveFile = null;

        private readonly PLTDecoder pltDecoder = new();
        private readonly SettingsManager settingsManager;

        private readonly string pathToDatabase = @"resources/ModelTable600.xls";

        public MainWindow() {
            InitializeComponent();

            mainMenu.Background = DefaultGUISettings.menuColor;
            SetInactive();

            settingsManager = new(ApplySettings);

            CommandBinding commandBinding = new(ApplicationCommands.Open, OpenFile);
            CommandBindings.Add(commandBinding);
            openButton.CommandBindings.Add(commandBinding);
        }

        private void WindowLoaded(object sender, RoutedEventArgs e) {
            Database.LoadDatabase(pathToDatabase);

            if (!Database.IsLoad()) {
                MessageBox.Show("Can't to upload a file with color data. Check the presence " +
                    "of the database file in the directory.", "Error!", MessageBoxButton.OK);
                Close();
            }
        }

        private void OpenFile(object sender, ExecutedRoutedEventArgs e) {
            OpenFileDialog fileDialog = new();
            fileDialog.Filter = "Picture or PLT-file|*.jpg;*.png;*.bmp;*.plt";

            if (fileDialog.ShowDialog() == false)
                MessageBox.Show("Can't open file!", "Error!", MessageBoxButton.OK);
            else
                ProcessFile(fileDialog.FileName);
        }

        private void ProcessFile(string fileName) {
            if (IsFileAlreadyOpened(fileName))
                return;

            //try
            //{
            if (fileName.EndsWith(".plt"))
                PLTFileHandler(fileName);
            else
                ImageFileHandler(fileName);
            //}
            //catch (ArgumentException Error)
            //{
            //    MessageBox.Show($"Can't process file!\n{Error.Message}",
            //                    "Error!", MessageBoxButton.OK);
            //    return;
            //}

            AddNewOpenedFile(fileName);
            ChangeActive(Active.ViewGrid);
            DisplayActiveBitmap(viewImage);
        }

        private bool IsFileAlreadyOpened(string file) {
            if (files.ContainsKey(file)) {
                UpdateOutputImage(file);
                return true;
            }

            return false;
        }

        private void AddNewOpenedFile(string fileName) {
            OpenedFile file = new(fileName, ChangeFile, CloseActiveFile);

            openedFiles.Children.Add(file);
            tabs[pathToActiveFile] = openedFiles.Children[^1];
        }

        private void ChangeFile(object sender, EventArgs e) {
            OpenedFile clicked = (OpenedFile)sender;
            if (clicked.PathToFile == pathToActiveFile)
                return;

            UpdateOutputImage(clicked.PathToFile);
        }

        private void UpdateOutputImage(string fileName) {
            ChangeActive(Active.ViewGrid);
            pathToActiveFile = fileName;
            DisplayActiveBitmap(viewImage);
        }

        private void PLTFileHandler(string fileName) {
            List<Stroke> strokes = pltDecoder.Decode(fileName);

            Picture picture = new(SettingsLoader.LoadSettings());
            picture.ProcessStrokes(strokes, pltDecoder.MaxX, pltDecoder.MaxY);

            pathToActiveFile = new(fileName);
            files[pathToActiveFile] = picture;
        }

        private void ImageFileHandler(string fileName) {
            BitmapImage image = new(new Uri(fileName));
            tracer = new(image, new(new()));
        }

        private void DisplayActiveBitmap(Image image) {
            //ImageBrush imageBrush = new(bitmap);
            //imageBrush.Stretch = Stretch.Uniform;
            //outputImage.Background = imageBrush;

            image.Source = files[pathToActiveFile].RenderedPicture;
        }

        private void RotateImage(object sender, RoutedEventArgs e) {
            //var (centerX, centerY) = Helpers.GetCenter(outputImage.ActualWidth, outputImage.ActualHeight);
            files[pathToActiveFile].Rotate();
            DisplayActiveBitmap(viewImage);
        }

        private void RepaintImage(object sender, RoutedEventArgs e) {
            PLTFileHandler(pathToActiveFile);
        }

        private void ViewClick(object sender, RoutedEventArgs e) {
            if (pathToActiveFile == null || (viewButton.Background as SolidColorBrush).Color == DefaultGUISettings.activeButton.Color)
                return;

            ChangeActive(Active.ViewGrid);
            DisplayActiveBitmap(viewImage);
        }

        private void SettingsClick(object sender, RoutedEventArgs e) {
            if (pathToActiveFile == null || (settingsButton.Background as SolidColorBrush).Color == DefaultGUISettings.activeButton.Color)
                return;

            ChangeActive(Active.SettingsGrid);
            DisplayActiveBitmap(settingsImage);
            settingsManager.DisplaySettings(settingsFields, files[pathToActiveFile].Settings);
        }

        private void InfoClick(object sender, RoutedEventArgs e) {
            if (pathToActiveFile == null || (infoButton.Background as SolidColorBrush).Color == DefaultGUISettings.activeButton.Color)
                return;

            ChangeActive(Active.InfoGreed);
            var settings = files[pathToActiveFile].Settings;
            List<UIElement> elements = new(settings.numOfSettings);

            string width = "Width (mm) = " + files[pathToActiveFile].Width.ToString();
            string height = "Height (mm) = " + files[pathToActiveFile].Height.ToString();
            elements.Add(Helpers.CreateTextBlock(width, HorizontalAlignment.Center, new()));
            elements.Add(Helpers.CreateTextBlock(height, HorizontalAlignment.Center, new()));

            foreach (var pair in settings) {
                string text = AlgorithmSettings.GetPropertyDesc(pair.Item1) + " = " + pair.Item2.ToString();
                elements.Add(Helpers.CreateTextBlock(text, HorizontalAlignment.Center, new()));
            }

            GridDisplayer displayer = new(infoGrid);
            displayer.Reset();
            displayer.DisplayElemByRow(elements);
        }

        private void ChangeActive(Active active) {
            switch (active) {
                case Active.ViewGrid:
                    viewGrid.Visibility = Visibility.Visible;
                    settingsGrid.Visibility = Visibility.Collapsed;
                    infoGrid.Visibility = Visibility.Collapsed;
                    viewButton.Background = DefaultGUISettings.activeButton;
                    settingsButton.Background = DefaultGUISettings.inactiveButton;
                    infoButton.Background = DefaultGUISettings.inactiveButton;
                    break;
                case Active.SettingsGrid:
                    viewGrid.Visibility = Visibility.Collapsed;
                    settingsGrid.Visibility = Visibility.Visible;
                    infoGrid.Visibility = Visibility.Collapsed;
                    viewButton.Background = DefaultGUISettings.inactiveButton;
                    settingsButton.Background = DefaultGUISettings.activeButton;
                    infoButton.Background = DefaultGUISettings.inactiveButton;
                    break;
                case Active.InfoGreed:
                    viewGrid.Visibility = Visibility.Collapsed;
                    settingsGrid.Visibility = Visibility.Collapsed;
                    infoGrid.Visibility = Visibility.Visible;
                    viewButton.Background = DefaultGUISettings.inactiveButton;
                    settingsButton.Background = DefaultGUISettings.inactiveButton;
                    infoButton.Background = DefaultGUISettings.activeButton;
                    break;
            }
        }

        private void SetInactive() {
            pathToActiveFile = null;

            viewGrid.Visibility = Visibility.Visible;
            settingsGrid.Visibility = Visibility.Collapsed;
            infoGrid.Visibility = Visibility.Collapsed;

            viewButton.Background = DefaultGUISettings.inactiveButton;
            settingsButton.Background = DefaultGUISettings.inactiveButton;
            infoButton.Background = DefaultGUISettings.inactiveButton;

            viewImage.Source = null;
            settingsImage.Source = null;
        }

        private void ApplySettings(AlgorithmSettings settedSettings, bool changed) {
            if (changed) {
                files[pathToActiveFile].Redraw(settedSettings);
                DisplayActiveBitmap(settingsImage);
            }
        }

        private void CloseActiveFile(object sender, RoutedEventArgs e) {
            if (pathToActiveFile != null) {
                files.Remove(pathToActiveFile);
                openedFiles.Children.Remove(tabs[pathToActiveFile]);
                if (openedFiles.Children.Count != 0) {
                    pathToActiveFile = ((OpenedFile)openedFiles.Children[0]).PathToFile;
                    if ((viewButton.Background as SolidColorBrush).Color == DefaultGUISettings.activeButton.Color)
                        DisplayActiveBitmap(viewImage);
                    else
                        DisplayActiveBitmap(settingsImage);
                } else {
                    SetInactive();
                }
            }
        }

        private void CloseApp(object sender, RoutedEventArgs e) {
            Close();
        }

        private void SaveFileClick(object sender, RoutedEventArgs e) {
            if (pathToActiveFile == null)
                return;

            if (savedFiles.ContainsKey(pathToActiveFile)) {
                if (pathToActiveFile.EndsWith(".plt"))
                    SaveImageToFile(files[pathToActiveFile].RenderedPicture, savedFiles[pathToActiveFile]);
                else
                    SavePLTToFile(files[pathToActiveFile].RenderedPicture, savedFiles[pathToActiveFile]);
            } else {
                SaveFileAsClick(sender, e);
            }
        }

        private void SaveFileAsClick(object sender, RoutedEventArgs e) {
            if (pathToActiveFile == null)
                return;

            Action<BitmapSource, string> call;
            SaveFileDialog dlg = new();
            string fileName = Helpers.GetFileName(pathToActiveFile);
            dlg.FileName = Helpers.GetFileNameWithoutExt(fileName);

            if (pathToActiveFile.EndsWith(".plt")) {
                dlg.DefaultExt = ".png";
                dlg.Filter = "Picture|*.jpg;*.png;*.bmp";
                call = SaveImageToFile;
            } else {
                dlg.DefaultExt = ".plt";
                dlg.Filter = "PLT-file|*.plt";
                call = SavePLTToFile;
            }

            if (dlg.ShowDialog() != true)
                MessageBox.Show("Couldn't select the path to save the file!", "Error!", MessageBoxButton.OK);
            else {
                savedFiles[pathToActiveFile] = dlg.FileName;
                call(files[pathToActiveFile].RenderedPicture, dlg.FileName);
            }
        }

        private void SaveImageToFile(BitmapSource image, string filePath) {
            BitmapEncoder encoder = filePath[^3..] switch {
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

        private void SavePLTToFile(BitmapSource image, string filePath) {
            throw new NotImplementedException();
        }
    }
}
