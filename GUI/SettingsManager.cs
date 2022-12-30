using System;
using System.Windows;
using System.Reflection;
using System.Windows.Controls;
using System.Collections.Generic;

using GUI.Settings;

namespace GUI {
    using ApplySettingsEvent = Action<AlgorithmSettings, bool>;

    internal class SettingsManager {
        private GridDisplayer displayer;
        private AlgorithmSettings oldSettings;
        private event ApplySettingsEvent ReturnNewSettings;

        public SettingsManager(ApplySettingsEvent returnNewSettings) {
            ReturnNewSettings += returnNewSettings;
        }

        public void DisplaySettings(Grid grid, AlgorithmSettings settings) {
            displayer = new(grid);
            displayer.Reset();
            displayer.GridInit(settings.NumOfSettings + 1);

            List<(UIElement, UIElement)> pairs = new(settings.NumOfSettings);
            foreach (var pair in settings) {
                var (label, value) = CreateRecord(pair);
                pairs.Add((label, value));
            }

            displayer.DisplayPairs(pairs);
            displayer.DisplayButton("Apply settings", ApplySettings);

            oldSettings = new(settings);
        }

        private (SettingToDisplay, TextBox) CreateRecord((PropertyInfo, object) pair) {
            SettingToDisplay label = new(pair.Item1);
            label.Text = AlgorithmSettings.GetPropertyDesc(pair.Item1);
            label.FontSize = DefaultGUISettings.FontSize;
            label.TextWrapping = TextWrapping.Wrap;
            label.HorizontalAlignment = HorizontalAlignment.Right;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.Margin = new(5, 2.5, 2.5, 2.5);

            TextBox value = new();
            value.Text = pair.Item2.ToString();
            value.FontSize = DefaultGUISettings.FontSize;
            value.TextWrapping = TextWrapping.Wrap;
            value.HorizontalContentAlignment = HorizontalAlignment.Left;
            value.VerticalAlignment = VerticalAlignment.Center;
            value.Margin = new(2.5, 2.5, 5, 2.5);

            return (label, value);
        }

        private void ApplySettings(object sender, RoutedEventArgs e) {
            List<string> errors = new();

            var displayedElems = displayer.GetDisplayedElems();
            Dictionary<PropertyInfo, object> values = new();
            foreach (var row in displayedElems) {
                SettingToDisplay setting = (SettingToDisplay)row.Find(elem => elem is SettingToDisplay);
                TextBox settingValue = (TextBox)row.Find(elem => elem is TextBox);
                if (row.Count != 2 || setting == null || settingValue == null)
                    continue;

                try {
                    var value = Convert.ChangeType(settingValue.Text, setting.Setting.PropertyType);
                    values[setting.Setting] = value;
                } catch (Exception) {
                    errors.Add(setting.Text);
                    var oldValue = setting.Setting.GetValue(oldSettings);
                    values[setting.Setting] = oldValue;
                    settingValue.Text = oldValue.ToString();
                }
            }

            DisplayErrors(errors);
            AlgorithmSettings newSettings = new(values);
            ReturnNewSettings(newSettings, !newSettings.Equals(oldSettings));

            oldSettings = new(newSettings);
        }

        private void DisplayErrors(List<string> errors) {
            if (errors.Count == 0)
                return;

            string toOutput = "The values of ";
            errors.ForEach(elem => toOutput += '"' + elem + '"' + ", ");
            toOutput = toOutput[..^2] + " could not be parsed. Old values are set instead.";

            MessageBox.Show(toOutput, "Convert errors!", MessageBoxButton.OK);
        }
    }
}
