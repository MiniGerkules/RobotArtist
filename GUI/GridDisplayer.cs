using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace GUI
{
    internal class GridDisplayer
    {
        private readonly Grid grid;
        private int lastFreeRow = 0;

        public GridDisplayer(Grid grid)
        {
            this.grid = grid;
        }

        public void DisplayPairs(List<(UIElement, UIElement)> pairs)
        {
            if (grid.RowDefinitions.Count < pairs.Count)
            {
                Reset();
                GridInit(pairs.Count);
            }

            foreach (var (item1, item2) in pairs)
            {
                AddElementInGrid(item1, 0, 1);
                AddElementInGrid(item2, 1, 1);
                ++lastFreeRow;
            }
        }

        public void DisplayElemByRow(List<UIElement> elements)
        {
            if (grid.RowDefinitions.Count < elements.Count)
            {
                Reset();
                GridInit(elements.Count);
            }

            foreach (var elem in elements)
            {
                AddElementInGrid(elem, 0, 2);
                ++lastFreeRow;
            }
        }

        public void DisplayButton(string text, RoutedEventHandler click)
        {
            if (grid.RowDefinitions.Count < lastFreeRow)
                throw new IndexOutOfRangeException("Can't add button! Don't have free rows!");

            Button button = new();
            button.Content = text;
            button.Margin = new(5, 5, 5, 5);
            button.Background = MainWindow.buttonColor;
            button.Click += click;

            Grid.SetRow(button, lastFreeRow);
            Grid.SetColumn(button, 0);
            Grid.SetColumnSpan(button, 2);
            grid.Children.Add(button);

            ++lastFreeRow;
        }

        public void GridInit(int numOfRows)
        {
            for (uint i = 0; i < 2; ++i)
            {
                ColumnDefinition column = new();
                column.Width = new(2 - i, GridUnitType.Star);
                grid.ColumnDefinitions.Add(column);
            }

            for (uint i = 0; i < numOfRows; ++i)
            {
                RowDefinition row = new();
                grid.RowDefinitions.Add(row);
            }
        }

        public void Reset()
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            lastFreeRow = 0;
        }

        private void AddElementInGrid(UIElement element, int column, int columnSpan)
        {
            Grid.SetRow(element, lastFreeRow);
            Grid.SetColumn(element, column);
            Grid.SetColumnSpan(element, columnSpan);
            grid.Children.Add(element);
        }
    }
}
