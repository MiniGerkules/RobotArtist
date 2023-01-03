using System.Reflection;
using GUI.Settings;

namespace GUI.Pages.SettingsPage {
    public class ItemModel {
        public PropertyInfo Property { get; }
        public string Decs => AlgorithmSettings.GetPropertyDesc(Property);
        public string Value { get; set; }

        public ItemModel(PropertyInfo propertyInfo, string value) {
            Property = propertyInfo;
            Value = value;
        }
    }
}
