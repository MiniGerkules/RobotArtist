using System;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;

using GUI.PLT;
using GUI.Settings;

namespace GUI.Pages.SettingsPage {
    public partial class SettingsPage : UserControl, IPage {
        private readonly MenuItem menuItem;
        private readonly SettingsVM settingsVM = new();

        private readonly Func<PLTPicture?> getActive;
        private readonly Action<string> thereAreErrors;
        private readonly Func<AlgorithmSettings, Task> applyNewSettings;

        MenuItem IPage.LinkedMenuItem => menuItem;
        Grid IPage.Container => mainGrid;

        public SettingsPage(MenuItem menuItem, Func<PLTPicture?> getActive,
                            Action<string> thereAreErrors,
                            Func<AlgorithmSettings, Task> applyNewSettings) {
            InitializeComponent();
            DataContext = settingsVM;
            this.menuItem = menuItem;

            this.getActive = getActive;
            this.thereAreErrors = thereAreErrors;
            this.applyNewSettings = applyNewSettings;
        }

        public void SetActive() {
            var active = getActive();
            if (active == null) return;

            (this as IPage).ShowMainParts();

            settingsVM.Items.Clear();
            foreach (var (property, value) in active.Settings) {
                if (value == null) continue;
                settingsVM.Items.Add(new(property, value.ToString()!));
            }

            settingsImage.Source = active.Rendered.MainImage;
        }

        private async void ApplySettings(object sender, RoutedEventArgs e) {
            var active = getActive();
            if (active == null) return;

            Dictionary<PropertyInfo, object> newValues = new();
            foreach (var item in settingsVM.Items)
                newValues.Add(item.Property, item.Value);

            var errors = CheckNewValues(newValues);
            if (errors.Count != 0) {
                thereAreErrors(MakeErrorMsg(errors));
                return;
            }

            AlgorithmSettings newSettings = new(newValues);
            if (active.Settings != newSettings) {
                await applyNewSettings(newSettings);

                var newActive = getActive();
                if (newActive != null) settingsImage.Source = newActive.Rendered.MainImage;
                else (this as IPage).SetInactive();
            }
        }

        private static List<(PropertyInfo, string)> CheckNewValues(Dictionary<PropertyInfo, object> newValues) {
            List<(PropertyInfo, string)> errors = new();

            foreach (var (property, value) in newValues) {
                if (value == null) continue;

                try {
                    Convert.ChangeType(value, property.PropertyType);
                } catch (Exception error) {
                    errors.Add((property, $"{value}. {error.Message}"));
                }
            }

            return errors;
        }

        private static string MakeErrorMsg(in List<(PropertyInfo, string)> errors) {
            StringBuilder errorMsg = new("Can't recognize next values:");

            foreach (var (property, value) in errors)
                errorMsg.AppendLine($"* {AlgorithmSettings.GetPropertyDesc(property)} -- {value};");

            return errorMsg.ToString();
        }
    }
}
