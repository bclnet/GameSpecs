using System;

namespace GameX.App.ExplorerVR.Controls
{
    public sealed class BindableProperty
    {
        internal static BindableProperty Create(string propertyName, Type returnType, Type declaringType, object defaultValue = null, Action<BindableObject, object, object> propertyChanged = null)
        {
            return new BindableProperty();
        }
    }
}
