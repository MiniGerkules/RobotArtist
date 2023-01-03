using System.Windows;
using System.Windows.Controls;

namespace GUI.Pages {
    internal interface IPage {
        protected MenuItem LinkedMenuItem { get; }
        protected Grid Container { get; }

        public bool IsLinked(object obj) {
            return obj is MenuItem menuItem && ReferenceEquals(LinkedMenuItem, menuItem);
        }

        public bool IsActive() {
            return Container.Visibility == Visibility.Visible &&
                    LinkedMenuItem.Background == DefaultGUISettings.activeButton;
        }

        public void SetActive() {
            Container.Visibility = Visibility.Visible;
            LinkedMenuItem.Background = DefaultGUISettings.activeButton;
        }

        public void SetInactive() {
            Container.Visibility = Visibility.Collapsed;
            LinkedMenuItem.Background = DefaultGUISettings.inactiveButton;
        }
    }
}
