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
            choosePicture();
            prepareToSetSettings();
        }

        private void choosePicture()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Picture|*.jpg;*.png;*.bmp";

            if (fileDialog.ShowDialog() == true)
                image = new BitmapImage(new Uri(fileDialog.FileName));
        }

        private void prepareToSetSettings()
        {
            outputImage.Source = image;
        }
    }
}
