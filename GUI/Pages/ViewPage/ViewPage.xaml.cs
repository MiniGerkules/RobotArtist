using System;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

using GUI.PLT;
using GUI.Settings;

namespace GUI.Pages.ViewPage {
    public partial class ViewPage : UserControl, IPage, IImgFileContainer {
        public delegate void ChooseClick(string fileToChoose);
        public delegate void CloseClick(string fileToClose);

        private readonly ViewVM viewVM = new();

        private readonly MenuItem linkedMenuItem;

        private Algorithm.Tracer tracer;
        private readonly PLTDecoder decoder;
        private readonly PLTImgBuilder builder;

        MenuItem IPage.LinkedMenuItem => linkedMenuItem;
        Grid IPage.Container => mainGrid;

        private readonly Dictionary<string, PLTPicture> files = new();
        private string? pathToActiveFile = null;

        public ViewPage(MenuItem linkedMenuItem, PLTDecoder decoder,
                        PLTImgBuilder builder, Algorithm.Tracer tracer) {
            InitializeComponent();
            DataContext = viewVM;

            this.linkedMenuItem = linkedMenuItem;
            this.decoder = decoder;
            this.builder = builder;
            this.tracer = tracer;
        }

        public bool Contains(string fileName) => files.ContainsKey(fileName);

        public async void Add(string fileName) {
            if (!Contains(fileName)) {
                if (fileName.EndsWith(".plt")) {
                    var picture = await PLTFileHandler(fileName);

                    OpenedFile file = new(fileName, ChangeFile, CloseFile);
                    viewVM.Items.Add(file);

                    files[fileName] = picture;
                    UpdateOutputImage(fileName);
                } else {
                    ImageFileHandler(fileName);
                }   
            }
        }

        public void RemoveActive() {
            if (pathToActiveFile != null) {
                CloseFile(pathToActiveFile);
            }
        }

        public PLTPicture? GetActive() {
            return pathToActiveFile != null ? files[pathToActiveFile] : null;
        }

        public string? GetPathToActive() {
            return pathToActiveFile;
        }

        public async Task ApplySettingsForActive(AlgorithmSettings newSettings) {
            files[pathToActiveFile!] = await Task.Run(
                () => builder.Rebuild(newSettings, files[pathToActiveFile!])
            );
            UpdateActiveImage();
        }

        private async Task<PLTPicture> PLTFileHandler(string fileName) {
            var decoded = await Task.Run(() => decoder.Decode(fileName));
            var picture = await Task.Run(() => builder.Build(decoded));

            return picture;
        }

        private void ImageFileHandler(string fileName) {
            BitmapImage image = new(new Uri(fileName));
            // all parameters below should be given by user!!! BEFORE Algorithm starts!
            var plt = tracer.Trace(image, 
                new Algorithm.Settings(
                    new Algorithm.GUITrace(
                        colorsAmount: 255, 
                        brushWidthMM: 2, 
                        canvasWidthMM: 10, // REQUIRED FROM USER!
                        canvasHeightMM: 10), // REQUIRED FROM USER!
                    amountOfTotalIters: 3,
                    doBlur: false,
                    goNormal: true,
                    canvasColorFault: 2,
                    itersAmountWithSmallOverlap: 1,
                    minLenFactor: null, 
                    maxLenFactor: 30,
                    minInitOverlapRatio: 0.6,
                    maxInitOverlapRatio: 0.8,
                    pixTol: 9,
                    pixTolAverage: 100,
                    pixTolAccept: 4,
                    useColor8Paints: false)); // REQUIRED FROM USER! // if true i don't understand how
        }

        private void RotateImage(object sender, RoutedEventArgs e) {
            files[pathToActiveFile!].Rotate();
            UpdateActiveImage();
        }
        
        private async void RepaintImage(object sender, RoutedEventArgs e) {
            files[pathToActiveFile!] = await Task.Run(
                () => builder.Rebuild(files[pathToActiveFile!])
            );
            UpdateActiveImage();
        }

        private void ChangeFile(OpenedFile toChange) {
            if (toChange.PathToFile == pathToActiveFile) return;
            else UpdateOutputImage(toChange.PathToFile);
        }

        private void CloseFile(OpenedFile toClose) {
            CloseFile(toClose.PathToFile);
        }

        private void CloseFile(string toClose) {
            files.Remove(toClose);
            var toRemove = viewVM.Items.First(elem => elem.PathToFile == toClose);
            viewVM.Items.Remove(toRemove);

            if (toClose == pathToActiveFile)
                WasClosedActiveFile();
        }

        private void WasClosedActiveFile() {
            if (viewVM.Items.Count != 0) {
                UpdateOutputImage(viewVM.Items[0].PathToFile);
            } else {
                pathToActiveFile = null;
                (this as IPage).SetInactive();
            }
        }

        private void UpdateOutputImage(string toReplaceActive) {
            pathToActiveFile = toReplaceActive;
            UpdateActiveImage();
        }

        private void UpdateActiveImage() {
            viewVM.Image = files[pathToActiveFile!].Rendered.MainImage;
        }
    }
}
