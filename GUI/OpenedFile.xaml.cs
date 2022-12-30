using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace GUI {
    public partial class OpenedFile : UserControl {
        public delegate void CloseClick(OpenedFile toClose);
        public delegate void ChangeClick(OpenedFile toClose);

        public string PathToFile { get; }

        private readonly ChangeClick chooseClick;
        private readonly CloseClick closeClick;

        public OpenedFile(string pathToFile, ChangeClick chooseClick, CloseClick closeClick) {
            InitializeComponent();
            PathToFile = pathToFile;
            this.chooseClick = chooseClick;
            this.closeClick = closeClick;

            choose.Content = Path.GetFileName(pathToFile);
            choose.Background = DefaultGUISettings.buttonColor;
            choose.FontSize = DefaultGUISettings.FontSize;
        }

        private void Choose_Click(object sender, RoutedEventArgs e) {
            chooseClick(this);
        }

        private void Close_Click(object sender, RoutedEventArgs e) {
            closeClick(this);
        }
    }
}
