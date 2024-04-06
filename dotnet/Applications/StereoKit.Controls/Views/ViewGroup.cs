using System.Collections.ObjectModel;

namespace StereoKit.UIX.Controls
{
    public class ViewGroup : View
    {
        public ObservableCollection<View> Children { get; set; } = new ObservableCollection<View>();
        public object Child { get; set; }

        public override void OnStep(object? arg)
        {
            foreach (var s in Children)
                s.OnStep(null);
        }
    }
}