using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.WebView;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKWebViewHandler : IViewHandler
	{
		new IWebView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}