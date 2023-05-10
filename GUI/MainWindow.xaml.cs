using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

using Microsoft.Win32;

using GUI.PLT;
using GUI.Settings;

using GUI.Pages;
using GUI.Pages.ViewPage;
using GUI.Pages.SettingsPage;

namespace GUI {
    public partial class MainWindow : Window {
        //private static readonly string pathToDatabase = @"resources/ModelTable600_initial.xls";
        private static readonly string pathToDatabase = @"resources/ModelTable600.xls";
        /// <summary> Key -- path to plt file, value -- path to saved image </summary>
        private readonly Dictionary<string, string> savedFiles = new();

        private readonly PLTDecoder pltDecoder;
        private readonly PLTImgBuilder pltImgBuilder = new();
        private readonly Algorithm.Tracer tracer;

        private readonly IImgFileContainer filesContainer;
        private readonly Dictionary<MenuItem, IPage> pages;
        private MenuItem? activePage = null;

        public MainWindow() {
            InitializeComponent();

            DatabaseLoader.LoadDatabase(pathToDatabase);
            tracer = new Algorithm.Tracer(DatabaseLoader.Database);
            pltDecoder = new(pltImgBuilder.Settings.DefaultBrushWidth);

            filesContainer = new ViewPage(viewButton, pltDecoder, pltImgBuilder, tracer);
            footer.DataContext = new BuildingImgProcessVM(pltDecoder, pltImgBuilder);

            pages = new() {
                { viewButton, (filesContainer as IPage)! },
                { stroKesStructButton, new StrokesStructurePage(stroKesStructButton,
                                                                filesContainer.GetActive) },
                { settingsButton, new SettingsPage(settingsButton, filesContainer.GetActive,
                                                   ErrorDisplayer, filesContainer.ApplySettingsForActive) },
                { infoButton, new InformationPage(infoButton, filesContainer.GetActive) },
            };

            mainMenu.Background = DefaultGUISettings.menuColor;
            foreach (var (_, page) in pages)
                pagePlaceholder.Children.Add(page as UserControl);
            SetAllPagesInactive();

            CommandBinding commandBinding = new(ApplicationCommands.Open, OpenFile);
            CommandBindings.Add(commandBinding);
            openButton.CommandBindings.Add(commandBinding);
        }

        private void ErrorDisplayer(string errorMsg) {
            MessageBox.Show(errorMsg, "ERROR!", MessageBoxButton.OK);
        }

        private void WindowLoaded(object sender, RoutedEventArgs e) {
            if (!DatabaseLoader.IsLoaded()) {
                ErrorDisplayer("Can't to upload a file with Color data. Check " +
                               "the presence of the database file in the directory.");
                Close();
            }
        }

        private void OpenFile(object sender, ExecutedRoutedEventArgs e) {
            OpenFileDialog fileDialog = new() {
                Filter = "Picture or PLT-file|*.jpg;*.png;*.bmp;*.plt",
            };

            if (fileDialog.ShowDialog() == false) {
                ErrorDisplayer("Can't open file!");
            } else {
                try {
                    filesContainer.Add(fileDialog.FileName);
                    ChangeActivePage(viewButton);
                } catch (Exception error) {
                    ErrorDisplayer(error.Message);
                }
            }
        }

        private void MenuButtonClick(object sender, RoutedEventArgs e) {
            if (!IsAnyPageActive() || sender is not MenuItem ||
                    ReferenceEquals(sender, activePage))
                return;

            ChangeActivePage((MenuItem)sender);
        }

        private void EditCurSettings(object sender, RoutedEventArgs e) {
            MenuButtonClick(settingsButton, e);
        }

        private void SaveCurrentSettings(object sender, RoutedEventArgs e) {
            SettingsWriter.WriteSettings(pltImgBuilder.Settings);
        }

        private void LoadSettingsFromFile(object sender, RoutedEventArgs e) {
            try {
                pltImgBuilder.Settings = SettingsReader.ReadSettings();
            } catch (Exception error) {
                ErrorDisplayer(error.Message);
            }
        }

        private void SaveSettingsAsDefault(object sender, RoutedEventArgs e) {
            SettingsWriter.WriteSettingsToDefaultConf(pltImgBuilder.Settings);
        }

        private void ResetSettings(object sender, RoutedEventArgs e) {
            pltImgBuilder.Settings = SettingsReader.ReadDefaultSettings();
        }

        private bool IsAnyPageActive() {
            foreach (var (_, page) in pages) {
                if (page.IsActive()) return true;
            }

            return false;
        }

        private void ChangeActivePage(MenuItem activeTab) {
            SetAllPagesInactive();
            activePage = activeTab;
            pages[activeTab].SetActive();
        }

        private void SetAllPagesInactive() {
            activePage = null;
            foreach (var (_, page) in pages)
                page.SetInactive();
        }

        private void CloseActiveFile(object sender, RoutedEventArgs e) {
            filesContainer.RemoveActive();
        }

        private void CloseApp(object sender, RoutedEventArgs e) {
            Close();
        }

        private void SaveFileClick(object sender, RoutedEventArgs e) {
            var active = filesContainer.GetActive();
            if (active == null) return;

            string pathToActive = filesContainer.GetPathToActive()!;
            if (savedFiles.ContainsKey(pathToActive)) {
                if (pathToActive.EndsWith(".plt"))
                    SaveImageToFile(active.Rendered.MainImage, savedFiles[pathToActive]);
                else
                    SavePLTToFile(active.Rendered.MainImage, savedFiles[pathToActive]);
            } else {
                SaveFileAsClick(sender, e);
            }
        }

        private void SaveFileAsClick(object sender, RoutedEventArgs e) {
            var active = filesContainer.GetActive();
            if (active == null)
                return;

            var pathToActive = filesContainer.GetPathToActive()!;
            SaveAs(pathToActive, active);
        }

        private void SaveAs(string pathToActive, PLTPicture active) {
            SaveFileDialog dlg = new() {
                FileName = Path.GetFileNameWithoutExtension(pathToActive),
            };

            Action<BitmapSource, string> call;
            if (pathToActive.EndsWith(".plt")) {
                dlg.DefaultExt = ".png";
                dlg.Filter = "Picture|*.jpg;*.png;*.bmp";
                call = SaveImageToFile;
            } else {
                dlg.DefaultExt = ".plt";
                dlg.Filter = "PLT-file|*.plt";
                call = SavePLTToFile;
            }

            if (dlg.ShowDialog() != true) {
                ErrorDisplayer("Couldn't select the path to save the file!");
            } else {
                savedFiles[pathToActive] = dlg.FileName;
                call(active.Rendered.MainImage, dlg.FileName);
            }
        }

        private void SaveImageToFile(BitmapSource image, string filePath) {
            BitmapEncoder encoder = filePath[^3..] switch {
                "png" => new PngBitmapEncoder(),
                "bmp" => new BmpBitmapEncoder(),
                _ => new JpegBitmapEncoder()
            };

            ScaleTransform mirrow = new(1, -1);
            TransformedBitmap tb = new(image.Clone(), mirrow);
            encoder.Frames.Add(BitmapFrame.Create(tb));
            using FileStream stream = new(filePath, FileMode.Create);
            encoder.Save(stream);
        }

        private void SavePLTToFile(BitmapSource image, string filePath) {
            throw new NotImplementedException();
        }
    }
}
