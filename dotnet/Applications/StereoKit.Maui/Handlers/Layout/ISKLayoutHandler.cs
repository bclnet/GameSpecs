using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Platform.LayoutViewGroup;

namespace StereoKit.Maui.Handlers
{
	public interface ISKLayoutHandler : IViewHandler
	{
		new ILayout VirtualView { get; }
		new PlatformView PlatformView { get; }

		void Add(IView view);
		void Remove(IView view);
		void Clear();
		void Insert(int index, IView view);
		void Update(int index, IView view);
		void UpdateZIndex(IView view);
	}
}
