using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.Immutable;

using GUI.PLT;
using GUI.Settings;

namespace GUI.Pages {
    public partial class InformationPage : UserControl, IPage {
        private readonly MenuItem menuItem;
        private readonly Func<PLTPicture?> getActive;

        MenuItem IPage.LinkedMenuItem => menuItem;
        Grid IPage.Container => mainGrid;

        public InformationPage(MenuItem menuItem, Func<PLTPicture?> getActive) {
            InitializeComponent();
            
            this.menuItem = menuItem;
            this.getActive = getActive;
        }

        public void SetActive() {
            var active = getActive();
            if (active == null) return;

            (this as IPage).ShowMainParts();

            List<string> infoFields = new(active.Settings.numOfSettings) {
                $"Width (mm) = {active.Width}.", $"Height (mm) = {active.Height}."
            };
            foreach (var (property, value) in active.Settings) {
                if (value == null) continue;
                infoFields.Add($"{AlgorithmSettings.GetPropertyDesc(property)} = {value}.");
            }

            ShowInfo(infoFields.ToImmutableList());
        }

        private void ShowInfo(ImmutableList<string> info) {
            StringBuilder builder = new();
            foreach (var infoField in info)
                builder.AppendLine(infoField);

            infoDisplayer.Text = builder.ToString();
        }
    }
}
