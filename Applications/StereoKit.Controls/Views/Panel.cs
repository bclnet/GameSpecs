using StereoKit.UIX.Controls;
using System.Collections.Generic;

namespace StereoKit.UIX.Views
{
    public class Panel : View
    {
        public IList<View> Children { get; set; } = new List<View>();
    }
}