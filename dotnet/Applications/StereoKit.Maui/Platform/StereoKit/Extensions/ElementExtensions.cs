using Microsoft.Maui;
using NView = StereoKit.UIX.Controls.View;

namespace StereoKit.Maui.Platform
{
	public static partial class ElementExtensions
	{
		public static NView ToContainerView(this IElement view, IMauiContext context) =>
			new ContainerView(context) { CurrentView = view };
	}
}