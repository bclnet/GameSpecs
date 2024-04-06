using System.Windows;
using System.Windows.Controls;

namespace GameX.App.Explorer.Views
{
    //public class BindingProxy_ : Freezable
    //{
    //    protected override Freezable CreateInstanceCore() => new BindingProxy_();

    //    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy_), new UIPropertyMetadata(null));
    //    public object Data
    //    {
    //        get => GetValue(DataProperty);
    //        set => SetValue(DataProperty, value);
    //    }
    //}

    //T FindVisualChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
    //{
    //    T child = default;
    //    for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
    //    {
    //        var ch = VisualTreeHelper.GetChild(parent, i);
    //        child = ch as T;
    //        if (child != null && child.Name == name) break;
    //        else child = FindVisualChildByName<T>(ch, name);
    //        if (child != null) break;
    //    }
    //    return child;
    //}

    public static class ViewExtensions
    {
        public static void WriteLine(this TextBox textBox, string line)
        {
            if (textBox.Text.Length != 0) textBox.AppendText("\n");
            textBox.AppendText(line);
            textBox.ScrollToEnd();
        }
    }
}
