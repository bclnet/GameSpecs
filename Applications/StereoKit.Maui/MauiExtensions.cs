using Microsoft.Maui;
using System.Reflection;

namespace StereoKit.Maui
{
    static partial class MauiExtensions
    {
        static readonly MethodInfo? InvokeMethod = typeof(CommandMapper).GetMethod("Invoke", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static void Invoke(this CommandMapper source, IElementHandler viewHandler, IElement? virtualView, string property, object? args)
            => InvokeMethod?.Invoke(source, new[] { viewHandler, virtualView, property, args });
    }
}
