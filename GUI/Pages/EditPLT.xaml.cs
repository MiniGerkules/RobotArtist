using GUI.PLT;
using GUI.Settings;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace GUI.Pages {
    public partial class EditPLT : UserControl, IPage {
        private readonly MenuItem menuItem;
        private readonly Func<PLTPicture?> getActive;

        MenuItem IPage.LinkedMenuItem => menuItem;
        Grid IPage.Container => mainGrid;

        public EditPLT(MenuItem menuItem, Func<PLTPicture?> getActive) {
            InitializeComponent();

            this.menuItem = menuItem;
            this.getActive = getActive;
        }

        public void SetActive() {
            var active = getActive();
            if (active == null) return;

            (this as IPage).ShowMainParts();

            // Should display info in text blocks
        }
    }
}
