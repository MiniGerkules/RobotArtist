using System;
using System.Windows;
using Microsoft.Win32;
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

        private BitmapImage image;
        public MainWindow()
        {
            InitializeComponent();
            ChoosePicture();
            PrepareToSetSettings();
        }

        private void ChoosePicture()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Picture|*.jpg;*.png;*.bmp";

            if (fileDialog.ShowDialog() == true)
                image = new BitmapImage(new Uri(fileDialog.FileName));
        }

        private void PrepareToSetSettings()
        {
            outputImage.Source = image;
            // Unpack object settings into the fields
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

            // Resume work
        }

        /// <summary>
        /// Event handler for the process of adding new gradients to the map
        /// </summary>
        /// <param name="sender"> The object sending new gradients </param>
        /// <param name="gradients"> New gradients to add to the map. </param>
        private void AddNewGradients(object sender, List<Vector> gradients)
        {
            if (gradients == null || gradients.Count == 0)
                return;

            // Resume work
        }
    }
}
