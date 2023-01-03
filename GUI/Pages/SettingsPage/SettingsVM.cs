using System.Collections.ObjectModel;

namespace GUI.Pages.SettingsPage {
    public class SettingsVM {
        public ObservableCollection<ItemModel> Items { get; } = new();
    }
}
