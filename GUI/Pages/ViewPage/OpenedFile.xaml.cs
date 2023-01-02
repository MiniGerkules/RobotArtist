using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace GUI.Pages.ViewPage {
    public partial class OpenedFile : UserControl {
        public delegate void ChooseClick(OpenedFile fileToChoose);
        public delegate void CloseClick(OpenedFile fileToClose);

        private readonly ChooseClick chooseClick;
        private readonly CloseClick closeClick;

        public string PathToFile { get; }

        public OpenedFile(string pathToFile, ChooseClick chooseClick,
                          CloseClick closeClick) {
            InitializeComponent();
            PathToFile = pathToFile;
            choose.Content = Path.GetFileName(pathToFile);

            this.chooseClick = chooseClick;
            this.closeClick = closeClick;
        }

        private void Choose_Click(object sender, RoutedEventArgs e) {
            chooseClick(this);
        }

        private void Close_Click(object sender, RoutedEventArgs e) {
            closeClick(this);
        }
    }
}
