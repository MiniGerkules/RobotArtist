﻿using System;
using System.Windows;
using System.Windows.Controls;

using GUI.PLT;

namespace GUI.Pages {
    public partial class StrokesStructurePage : UserControl, IPage {
        private readonly MenuItem menuItem;
        private readonly Func<PLTPicture?> getActive;

        MenuItem IPage.LinkedMenuItem => menuItem;
        Grid IPage.Container => mainGrid;

        public StrokesStructurePage(MenuItem menuItem, Func<PLTPicture?> getActive) {
            InitializeComponent();

            this.menuItem = menuItem;
            this.getActive = getActive;
        }

        public void SetActive() {
            var active = getActive();
            if (active == null) return;

            mainGrid.Visibility = Visibility.Visible;
            menuItem.Background = DefaultGUISettings.activeButton;

            strokesStructureImg.Source = active.Rendered.StrokesStructure;
        }
    }
}
