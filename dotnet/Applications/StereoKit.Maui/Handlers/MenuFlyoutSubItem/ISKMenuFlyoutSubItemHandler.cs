using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MenuFlyoutSubItem;

namespace StereoKit.Maui.Handlers
{
	public interface ISKMenuFlyoutSubItemHandler : IElementHandler
	{
		void Add(IMenuElement view);
		void Remove(IMenuElement view);
		void Clear();
		void Insert(int index, IMenuElement view);
		new PlatformView PlatformView { get; }
		new IMenuFlyoutSubItem VirtualView { get; }
	}
}
