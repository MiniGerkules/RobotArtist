﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GUI
{
    public partial class OpenedFile : UserControl
    {
        public string PathToFile { get; private set; }
        private RoutedEventHandler chooseClick;
        private RoutedEventHandler closeClick;

        public OpenedFile(string pathToFile, RoutedEventHandler chooseClick, RoutedEventHandler closeClick)
        {
            InitializeComponent();
            PathToFile = pathToFile;
            this.chooseClick = chooseClick;
            this.closeClick = closeClick;

            choose.Content = Helpers.GetFileName(pathToFile);
            choose.Background = DefaultSettings.buttonColor;
            choose.FontSize = DefaultSettings.FontSize;
        }

        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            chooseClick(this, e);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            closeClick(this, e);
        }
    }
}