using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MenuBarItem;

namespace StereoKit.Maui.Handlers
{
    public interface ISKMenuBarItemHandler : IElementHandler
	{
		void Add(IMenuElement view);
		void Remove(IMenuElement view);
		void Clear();
		void Insert(int index, IMenuElement view);
		new PlatformView PlatformView { get; }
		new IMenuBarItem VirtualView { get; }
	}
}
