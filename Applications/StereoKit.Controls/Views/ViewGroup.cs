using System.Collections.ObjectModel;

namespace StereoKit.UIX.Controls
{
    public class ViewGroup : View
    {
        public ObservableCollection<View> Children { get; set; } = new ObservableCollection<View>();
        public object Child { get; set; }
    }
}