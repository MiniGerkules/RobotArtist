using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GUI
{
    internal class Settings
    {
        private readonly Grid grid;
        private readonly Dictionary<PossibleSettings, (TextBox, double)> settings = new();
        public event Action<ImmutableDictionary<PossibleSettings, double>> applySettings;

        public Settings(Grid grid)
        {
            this.grid = grid;
        }

        public void DisplaySettings(Dictionary<PossibleSettings, (string, double)> userNeed)
        {
            GridInit(userNeed.Count + 1);
            settings.Clear();

            int rowPtr = 0;
            foreach(var pair in userNeed)
            {
                var (label, value) = CreateRecord(pair);

                Grid.SetRow(label, rowPtr);
                Grid.SetColumn(label, 0);
                Grid.SetRow(value, rowPtr);
                Grid.SetColumn(value, 1);

                grid.Children.Add(label);
                grid.Children.Add(value);

                settings.Add(pair.Key, (value, pair.Value.Item2));
                ++rowPtr;
            }

            Button apply = new();
            apply.Content = "Apply settings";
            apply.Click += ApplySettings;
            Grid.SetRow(apply, rowPtr);
            Grid.SetColumn(apply, 0);
            Grid.SetColumnSpan(apply, 2);
            grid.Children.Add(apply);
        }

        private (TextBlock, TextBox) CreateRecord(KeyValuePair<PossibleSettings, (string, double)> pair)
        {
            TextBlock label = new();
            label.Text = pair.Value.Item1;
            label.FontSize = 16;
            label.TextWrapping = TextWrapping.Wrap;
            label.HorizontalAlignment = HorizontalAlignment.Right;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.Margin = new(0, 0, 2.5, 0);

            TextBox value = new();
            value.Text = pair.Value.Item2.ToString();
            value.FontSize = 16;
            value.TextWrapping = TextWrapping.Wrap;
            value.HorizontalContentAlignment = HorizontalAlignment.Left;
            value.VerticalAlignment = VerticalAlignment.Center;
            value.Margin = new(2.5, 0, 0, 0);

            return (label, value);
        }

        private void GridInit(int numOfRows)
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            for (uint i = 0; i < 2; ++i)
            {
                ColumnDefinition column = new();
                grid.ColumnDefinitions.Add(column);
            }

            for (uint i = 0; i < numOfRows; ++i)
            {
                RowDefinition row = new();
                grid.RowDefinitions.Add(row);
            }
        }

        private void ApplySettings(object sender, RoutedEventArgs e)
        {
            if (applySettings == null)
                return;

            Dictionary<PossibleSettings, double> applied = new();
            foreach (var pair in settings)
            {
                if (double.TryParse(pair.Value.Item1.Text, out double value))
                {
                    applied.Add(pair.Key, value);
                }
                else
                {
                    MessageBox.Show($"The \"{pair.Value.Item1.Text}\" input cannot be converted to a " +
                        "number! The setting is set to the default value.", "Convert error", MessageBoxButton.OK);
                    applied.Add(pair.Key, pair.Value.Item2);
                }
            }

            applySettings.Invoke(applied.ToImmutableDictionary());
        }
    }
}
