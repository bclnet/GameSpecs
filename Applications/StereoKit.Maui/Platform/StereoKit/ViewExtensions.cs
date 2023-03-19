using Microsoft.Maui;
using StereoKit.UIX.Controls;
using System.Threading.Tasks;

namespace StereoKit.Maui.Platform
{
    public static partial class ViewExtensions
	{
		public static void UpdateIsEnabled(this View platformView, IView view) { }

		public static void Focus(this View platformView, FocusRequest request) { }

		public static void Unfocus(this View platformView, IView view) { }

		public static void UpdateVisibility(this View platformView, IView view) { }

		public static Task UpdateBackgroundImageSourceAsync(this View platformView, IImageSource? imageSource, IImageSourceServiceProvider? provider)
			=> Task.CompletedTask;

		public static void UpdateBackground(this View platformView, IView view) { }

		public static void UpdateClipsToBounds(this View platformView, IView view) { }

		public static void UpdateAutomationId(this View platformView, IView view) { }

		public static void UpdateClip(this View platformView, IView view) { }

		public static void UpdateShadow(this View platformView, IView view) { }

		public static void UpdateBorder(this View platformView, IView view) { }

		public static void UpdateOpacity(this View platformView, IView view) { }

		public static void UpdateSemantics(this View platformView, IView view) { }

		public static void UpdateFlowDirection(this View platformView, IView view) { }

		public static void UpdateTranslationX(this View platformView, IView view) { }

		public static void UpdateTranslationY(this View platformView, IView view) { }

		public static void UpdateScale(this View platformView, IView view) { }

		public static void UpdateRotation(this View platformView, IView view) { }

		public static void UpdateRotationX(this View platformView, IView view) { }

		public static void UpdateRotationY(this View platformView, IView view) { }

		public static void UpdateAnchorX(this View platformView, IView view) { }

		public static void UpdateAnchorY(this View platformView, IView view) { }

		public static void InvalidateMeasure(this View platformView, IView view) { }

		public static void UpdateWidth(this View platformView, IView view) { }

		public static void UpdateHeight(this View platformView, IView view) { }

		public static void UpdateMinimumHeight(this View platformView, IView view) { }

		public static void UpdateMaximumHeight(this View platformView, IView view) { }

		public static void UpdateMinimumWidth(this View platformView, IView view) { }

		public static void UpdateMaximumWidth(this View platformView, IView view) { }

		internal static Microsoft.Maui.Graphics.Rect GetPlatformViewBounds(this IView view) => view.Frame;

		internal static System.Numerics.Matrix4x4 GetViewTransform(this IView view) => new System.Numerics.Matrix4x4();

		internal static Microsoft.Maui.Graphics.Rect GetBoundingBox(this IView view) => view.Frame;

		internal static object? GetParent(this View? view)
			=> null;

		internal static IWindow? GetHostedWindow(this IView? view)
			=> null;

		public static void UpdateInputTransparent(this View nativeView, IViewHandler handler, IView view) { }
	}
}
