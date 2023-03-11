using GameSpec.Metadata;
using System.Globalization;
using TreeView.Maui.Core;

namespace GameSpec.App.Explorer.Views
{
    public class MetadataItemToTreeViewNodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ToTreeNodes((IEnumerable<MetadataItem>)value);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
        static IEnumerable<IHasChildrenTreeViewNode> ToTreeNodes(IEnumerable<MetadataItem> source)
        {
            if (source == null) yield break;
            foreach (var s in source) yield return new TreeViewNode
            {
                Name = s.Name,
                Value = s,
                GetChildren = s.Items.Count > 0 ? node => ToTreeNodes(((MetadataItem)node.Value).Items).ToList() : null
            };
        }
    }

    public class MetadataInfoToTreeViewNodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ToTreeNodes((IEnumerable<MetadataInfo>)value);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
        static IEnumerable<IHasChildrenTreeViewNode> ToTreeNodes(IEnumerable<MetadataInfo> source)
        {
            if (source == null) yield break;
            foreach (var s in source) yield return new TreeViewNode
            {
                Name = s?.Name,
                Value = s,
                GetChildren = s?.Items.Any() == true ? node => ToTreeNodes(((MetadataInfo)node.Value).Items).ToList() : null
            };
        }
    }

    public static class ViewExtensions
    {
        public static void WriteLine(this Label source, string line)
        {
            source.Text ??= string.Empty;
            if (source.Text.Length != 0) source.Text += "\n";
            source.Text += line;
            //label.ScrollToEnd();
        }

        public static void WriteLine(this Entry source, string line)
        {
            source.Text ??= string.Empty;
            if (source.Text.Length != 0) source.Text += "\n";
            source.Text += line;
            //label.ScrollToEnd();
        }
    }
}
