using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MenuFlyout;

namespace StereoKit.Maui.Handlers
{
    public interface ISKMenuFlyoutHandler : IElementHandler
    {
        void Add(IMenuElement view);
        void Remove(IMenuElement view);
        void Clear();
        void Insert(int index, IMenuElement view);

        new PlatformView PlatformView { get; }
        new IMenuFlyout VirtualView { get; }
    }
}
