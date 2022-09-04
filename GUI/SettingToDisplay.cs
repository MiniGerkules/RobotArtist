using System.Reflection;
using System.Windows.Controls;

namespace GUI {
    internal class SettingToDisplay : TextBlock {
        public PropertyInfo Setting { get; private set; }

        public SettingToDisplay(PropertyInfo setting) {
            Setting = setting;
        }
    }
}
