using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GUI
{
    internal class Settings
    {
        public event Action<ImmutableDictionary<PossibleSettings, double>, bool> applySettings;
        private readonly Dictionary<PossibleSettings, (TextBox, double)> settings = new();
        private readonly GridDisplayer displayer;

        public Settings(Grid grid)
        {
            displayer = new(grid);
        }

        public void DisplaySettings(Dictionary<PossibleSettings, double> userNeed)
        {
            displayer.Reset();
            displayer.GridInit(userNeed.Count + 1);
            settings.Clear();

            List<(UIElement, UIElement)> pairs = new(userNeed.Count);
            foreach(var pair in userNeed)
            {
                var (label, value) = CreateRecord(pair);
                pairs.Add((label, value));
                settings.Add(pair.Key, (value, pair.Value));
            }

            displayer.DisplayPairs(pairs);
            displayer.DisplayButton("Apply settings", ApplySettings);
        }

        private (TextBlock, TextBox) CreateRecord(KeyValuePair<PossibleSettings, double> pair)
        {
            TextBlock label = Helpers.CreateTextBlock(pair.Key.GetDescription(),
                HorizontalAlignment.Right, new(5, 2.5, 2.5, 2.5));

            TextBox value = new();
            value.Text = pair.Value.ToString();
            value.FontSize = 16;
            value.TextWrapping = TextWrapping.Wrap;
            value.HorizontalContentAlignment = HorizontalAlignment.Left;
            value.VerticalAlignment = VerticalAlignment.Center;
            value.Margin = new(2.5, 2.5, 5, 2.5);

            return (label, value);
        }

        private void ApplySettings(object sender, RoutedEventArgs e)
        {
            if (applySettings == null)
                return;

            List<string> errors = new();
            bool isChanged = false;
            Dictionary<PossibleSettings, double> applied = new();
            foreach (var pair in settings)
            {
                if (double.TryParse(pair.Value.Item1.Text, out double value))
                {
                    applied.Add(pair.Key, value);
                    isChanged |= value != settings[pair.Key].Item2;
                }
                else
                {
                    errors.Add(pair.Value.Item1.Text);
                    applied.Add(pair.Key, pair.Value.Item2);
                }
            }

            DisplayErrors(errors);
            applySettings.Invoke(applied.ToImmutableDictionary(), isChanged);
            UpdateActualSettings(applied);
        }

        private void DisplayErrors(List<string> errors)
        {
            if (errors.Count == 0)
                return;

            string toOutput = "The values of ";
            errors.ForEach(elem => toOutput += '"' + elem + '"' + ", ");
            toOutput = toOutput[..^2] + " could not be parsed. Default values are set instead.";

            MessageBox.Show(toOutput, "Convert error", MessageBoxButton.OK);
        }

        private void UpdateActualSettings(Dictionary<PossibleSettings, double> applied)
        {
            foreach (var pair in applied)
            {
                TextBox textBox = settings[pair.Key].Item1;
                textBox.Text = pair.Value.ToString();
                settings[pair.Key] = (textBox, pair.Value);
            }
        }
    }
}
