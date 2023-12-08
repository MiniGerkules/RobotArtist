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
                PLTPicture picture;

                if (fileName.EndsWith(".plt")) {
                    picture = await PLTFileHandler(fileName);
                } else {
                    picture = await ImageFileHandler(fileName);
                }

                OpenedFile file = new(fileName, ChangeFile, CloseFile);
                viewVM.Items.Add(file);

                files[fileName] = picture;
                UpdateOutputImage(fileName);
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

        private async Task<PLTPicture> ImageFileHandler(string fileName) {
            var settings = builder.Settings;
            var plt = await Task.Run(() =>
                tracer.Trace(fileName,
                    new Algorithm.Settings(
                        new Algorithm.GUITrace(
                            colorsAmount: settings.ColorsAmount,
                            brushWidthMM: settings.DefaultBrushWidth,
                            canvasWidthMM: settings.DefaultWidthOfGenImg,
                            canvasHeightMM: settings.DefaultHeightOfGenImg
                        ),
                        amountOfTotalIters: settings.AmountOfTotalIters,
                        doBlur: settings.DoBlur,
                        goNormal: settings.GoNormal,
                        canvasColorFault: settings.CanvasColorFault,
                        itersAmountWithSmallOverlap: settings.ItersMinOverlap,
                        minLenFactor: settings.MinLenFactor,
                        maxLenFactor: (int)settings.MaxLenFactor,
                        minInitOverlapRatio: settings.MinOverlap,
                        maxInitOverlapRatio: settings.MaxOverlap,
                        pixTol: settings.PixTol,
                        pixTolAverage: settings.PixTol2,
                        pixTolAccept: settings.PixTolBest,
                        useColor8Paints: settings.UseColor8Paints
                    )
                )
            );

            return await Task.Run(() => builder.Build(plt));
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
