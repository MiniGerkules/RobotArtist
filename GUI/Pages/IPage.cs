using System.Windows;
using System.Windows.Controls;

namespace GUI.Pages {
    internal interface IPage {
        protected MenuItem LinkedMenuItem { get; }
        protected Grid Container { get; }

        bool IsLinked(object obj) {
            return obj is MenuItem menuItem && ReferenceEquals(LinkedMenuItem, menuItem);
        }

        bool IsActive() {
            return Container.Visibility == Visibility.Visible &&
                    LinkedMenuItem.Background == DefaultGUISettings.activeButton;
        }

        void SetActive() {
            ShowMainParts();
        }

        void ShowMainParts() {
            Container.Visibility = Visibility.Visible;
            LinkedMenuItem.Background = DefaultGUISettings.activeButton;
        }

        void SetInactive() {
            HideMainParts();
        }

        void HideMainParts() {
            Container.Visibility = Visibility.Collapsed;
            LinkedMenuItem.Background = DefaultGUISettings.inactiveButton;
        }
    }
}
