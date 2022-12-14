using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

using GUI.PLT;
using GUI.Settings;
using System.Threading.Tasks;

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
        private AlgorithmSettings curSettings = null;

        private static readonly string pathToDatabase = @"resources/ModelTable600_initial.xls";
        private ActiveGrid activeGrid = ActiveGrid.NoActive;

        public MainWindow() {
            InitializeComponent();
            progressBar.DataContext = pltDecoder;

            mainMenu.Background = DefaultGUISettings.menuColor;
            SetInactive();

            settingsManager = new(ApplySettings);

            CommandBinding commandBinding = new(ApplicationCommands.Open, OpenFile);
            CommandBindings.Add(commandBinding);
            openButton.CommandBindings.Add(commandBinding);
        }

        private void WindowLoaded(object sender, RoutedEventArgs e) {
            DatabaseLoader.LoadDatabase(pathToDatabase);
            if (!DatabaseLoader.IsLoaded()) {
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

        private async void ProcessFile(string fileName) {
            if (IsFileAlreadyOpened(fileName))
                return;

            try {
                if (fileName.EndsWith(".plt")) await PLTFileHandler(fileName);
                else await ImageFileHandler(fileName);
            } catch (ArgumentException Error) {
                MessageBox.Show($"Can't process file!\n{Error.Message}",
                                "Error!", MessageBoxButton.OK);
                return;
            }

            AddNewOpenedFile(fileName);
            SwitchToAnotherTab(ActiveGrid.ViewGrid, viewImage);
        }

        private bool IsFileAlreadyOpened(string file) {
            if (files.ContainsKey(file)) {
                UpdateOutputImage(file);
                return true;
            }

            return false;
        }

        private void AddNewOpenedFile(in string fileName) {
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
            pathToActiveFile = fileName;
            SwitchToAnotherTab(ActiveGrid.ViewGrid, viewImage);
        }

        private async Task PLTFileHandler(string fileName) {
            status.Text = "Process PLT file";
            var pltPicture = Task.Run(() => pltDecoder.Decode(fileName));

            curSettings ??= SettingsReader.ReadDefaultSettings();
            Picture picture = new(await pltPicture, curSettings);

            status.Text = "Render image from PLT file";
            await Task.Run(() => picture.ProcessStrokes());

            pathToActiveFile = new(fileName);
            files[pathToActiveFile] = picture;
            status.Text = "";
        }

        private async Task ImageFileHandler(string fileName) {
            BitmapImage image = new(new Uri(fileName));
            tracer = new(image, new(new()), DatabaseLoader.Database);
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
            if (pathToActiveFile == null || activeGrid == ActiveGrid.ViewGrid)
                return;

            SwitchToAnotherTab(ActiveGrid.ViewGrid, viewImage);
        }

        private void EditCurPicSettings(object sender, RoutedEventArgs e) {
            if (pathToActiveFile == null || activeGrid == ActiveGrid.SettingsGrid)
                return;

            SwitchToAnotherTab(ActiveGrid.SettingsGrid, settingsImage);
            settingsManager.DisplaySettings(settingsFields, files[pathToActiveFile].Settings);
        }

        private void InfoClick(object sender, RoutedEventArgs e) {
            if (pathToActiveFile == null || activeGrid == ActiveGrid.InfoGreed)
                return;

            activeGrid = ActiveGrid.InfoGreed;
            ChangeActive();
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

        private void SwitchToAnotherTab(ActiveGrid active, Image image) {
            activeGrid = active;
            ChangeActive();
            DisplayActiveBitmap(image);
        }

        private void SaveCurrentSettings(object sender, RoutedEventArgs e) {
            if (curSettings == null) return;
            SettingsWriter.WriteSettings(curSettings);
        }

        private void LoadSettingsFromFile(object sender, RoutedEventArgs e) {
            var newSettings = SettingsReader.ReadSettings();

            if (newSettings != null)
                curSettings = newSettings;
            else
                MessageBox.Show("You didn't choose the file!", "Error!", MessageBoxButton.OK);
        }

        private void SaveSettingsAsDefault(object sender, RoutedEventArgs e) {
            if (curSettings != null)
                SettingsWriter.WriteSettingsToDefaultConf(curSettings);
        }

        private void ResetSettings(object sender, RoutedEventArgs e) {
            curSettings = SettingsReader.ReadDefaultSettings();
        }

        private void ChangeActive() {
            switch (activeGrid) {
                case ActiveGrid.ViewGrid:
                    viewGrid.Visibility = Visibility.Visible;
                    settingsGrid.Visibility = Visibility.Collapsed;
                    infoGrid.Visibility = Visibility.Collapsed;
                    viewButton.Background = DefaultGUISettings.activeButton;
                    settingsButton.Background = DefaultGUISettings.inactiveButton;
                    infoButton.Background = DefaultGUISettings.inactiveButton;
                    break;
                case ActiveGrid.SettingsGrid:
                    viewGrid.Visibility = Visibility.Collapsed;
                    settingsGrid.Visibility = Visibility.Visible;
                    infoGrid.Visibility = Visibility.Collapsed;
                    viewButton.Background = DefaultGUISettings.inactiveButton;
                    settingsButton.Background = DefaultGUISettings.activeButton;
                    infoButton.Background = DefaultGUISettings.inactiveButton;
                    break;
                case ActiveGrid.InfoGreed:
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

        private async void ApplySettings(AlgorithmSettings settedSettings, bool changed) {
            if (changed) {
                await Task.Run(() => files[pathToActiveFile].Redraw(settedSettings));
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
