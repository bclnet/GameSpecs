using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameX.App.ExplorerVR.Controls
{
    public abstract class BindableObject
    {
        public object BindingContext { get; set; }

        public object GetValue(BindableProperty property)
        {
            return default;
        }

        public void SetValue(BindableProperty property, object value)
        {
        }
    }
}
