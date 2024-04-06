using Microsoft.Maui;

namespace StereoKit.Maui
{
    static partial class ViewExtensions
    {
        internal static bool NeedsContainer(this IView? view)
        {
            if (view?.Clip != null || view?.Shadow != null)
                return true;

            return false;
        }
    }
}
