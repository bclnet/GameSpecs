using System.Collections.ObjectModel;

namespace StereoKit.UIX.Controls
{
    public class ViewGroup : View
    {
        public ObservableCollection<object> Children { get; set; }
        public object Child { get; set; }
    }
}