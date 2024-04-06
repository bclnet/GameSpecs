using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;

namespace GameX.App.Explorer
{
    public partial class MainPagex : ContentPage
    {
        int count = 0;

        public MainPagex() => InitializeComponent();

        public void OnFirstLoad() { }

        public void Open(Family familySelectedItem, IList<Uri> pakUris) { }

        void OnCounterClicked(object sender, EventArgs e)
        {
            count++;
            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";
        }
    }
}