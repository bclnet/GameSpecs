using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MauiSearchBar;
using QueryEditor = StereoKit.UIX.Controls.Entry;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKSearchBarHandler : IViewHandler
	{
		new ISearchBar VirtualView { get; }
		new PlatformView PlatformView { get; }
		QueryEditor? QueryEditor { get; }
	}
}