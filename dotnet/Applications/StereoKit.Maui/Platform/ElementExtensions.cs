using Microsoft.Maui;
using Microsoft.Maui.Platform;
using System;
using PlatformView = StereoKit.UIX.Controls.View;

namespace StereoKit.Maui.Platform
{
    public static partial class ElementExtensions
	{
		internal static PlatformView ToSKPlatform(this IElement view)
		{
			if (view is IReplaceableView replaceableView && replaceableView.ReplacedView != view)
				return replaceableView.ReplacedView.ToSKPlatform();

			_ = view.Handler ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set on parent.");

			if (view.Handler is IViewHandler viewHandler)
			{
				if (viewHandler.ContainerView is PlatformView containerView)
					return containerView;

				if (viewHandler.PlatformView is PlatformView platformView)
					return platformView;
			}

			return (view.Handler?.PlatformView as PlatformView) ?? throw new InvalidOperationException($"Unable to convert {view} to {typeof(PlatformView)}");
		}

        public static PlatformView ToSKPlatform(this IElement view, IMauiContext context)
        {
			//try
			{
				var handler = view.ToHandler(context);

				if (handler.PlatformView is not PlatformView result)
					throw new InvalidOperationException($"Unable to convert {view} to {typeof(PlatformView)}");
			}
			//catch (Exception ex) { }

            return view.ToSKPlatform() ?? throw new InvalidOperationException($"Unable to convert {view} to {typeof(PlatformView)}");
        }
    }
}
