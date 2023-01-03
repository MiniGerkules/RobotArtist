using System;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace GUI.Pages.ViewPage {
    internal class ViewVM : NotifierOfPropertyChange {
        public ObservableCollection<OpenedFile> Items { get; } = new();
        
        private ImageSource? imageSource = null;
        public ImageSource? Image {
            get => imageSource;
            set {
                imageSource = value;
                NotifyPropertyChanged(nameof(Image));
            }
        }
    }
}
