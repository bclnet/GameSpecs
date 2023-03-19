using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls2.MauiSearchBar;
using QueryEditor = StereoKit.Maui.Controls.Entry;

namespace StereoKit.Maui.Handlers
{
	public partial interface ISKSearchBarHandler : IViewHandler
	{
		new ISearchBar VirtualView { get; }
		new PlatformView PlatformView { get; }
		QueryEditor? QueryEditor { get; }
	}
}