using StereoKit.UIX.Controls;
using System;
using System.Collections.Generic;

namespace StereoKit.UIX.Views
{
    public class Panel : View
    {
        public Panel() => Console.WriteLine("Controls: Panel");

        public IList<View> Children { get; set; } = new List<View>();
    }
}