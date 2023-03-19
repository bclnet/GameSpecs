using Microsoft.Maui.Graphics;
using System.Threading.Tasks;
using Microsoft.Maui.Media;
using System.IO;
using IPlatformViewHandler = Microsoft.Maui.IViewHandler;
using PlatformView = StereoKit.UIX.Controls.View;
using ParentView = StereoKit.UIX.Controls.View;
using System;
using Microsoft.Maui;

namespace StereoKit.Maui
{
    public static partial class ViewExtensions
    {
        internal static bool NeedsContainer(this IView? view)
        {
            if (view?.Clip != null || view?.Shadow != null)
                return true;

            return false;
        }
    }
}
