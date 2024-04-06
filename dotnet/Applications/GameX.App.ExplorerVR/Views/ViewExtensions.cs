using GameX.Meta;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TreeView.Maui.Core;

namespace GameX.App.Explorer.Views
{
    public class MetaItemToViewModel : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ToTreeNodes((IEnumerable<MetaItem>)value);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
        static IEnumerable<IHasChildrenTreeViewNode> ToTreeNodes(IEnumerable<MetaItem> source)
        {
            if (source == null) yield break;
            foreach (var s in source) yield return new TreeViewNode
            {
                Name = s.Name,
                Value = s,
                GetChildren = s.Items.Count > 0 ? node => ToTreeNodes(((MetaItem)node.Value).Items).ToList() : null,
            };
        }
    }

    public class MetaInfoToViewModel : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ToTreeNodes((IEnumerable<MetaInfo>)value);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
        static IEnumerable<IHasChildrenTreeViewNode> ToTreeNodes(IEnumerable<MetaInfo> source)
        {
            if (source == null) yield break;
            foreach (var s in source) yield return new TreeViewNode
            {
                Name = s?.Name,
                Value = s,
                GetChildren = s?.Items.Any() == true ? node => ToTreeNodes(((MetaInfo)node.Value).Items).ToList() : null,
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
