using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MenuBar;

namespace StereoKit.Maui.Handlers
{
	public interface ISKMenuBarHandler : IElementHandler
	{
		void Add(IMenuBarItem view);
		void Remove(IMenuBarItem view);
		void Clear();
		void Insert(int index, IMenuBarItem view);
		new PlatformView PlatformView { get; }
		new IMenuBar VirtualView { get; }
	}
}
