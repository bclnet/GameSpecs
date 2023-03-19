using Microsoft.Maui;
using PlatformView = StereoKit.Maui.Controls.MenuFlyoutItem;

namespace StereoKit.Maui.Handlers
{
    public partial class SKMenuFlyoutItemHandler
    {
        protected override PlatformView CreatePlatformElement() => new();

        protected override void ConnectHandler(PlatformView PlatformView)
        {
            base.ConnectHandler(PlatformView);
            PlatformView.Click += OnClicked;
        }

        protected override void DisconnectHandler(PlatformView PlatformView)
        {
            base.DisconnectHandler(PlatformView);
            PlatformView.Click -= OnClicked;
        }

        void OnClicked(object sender, UI.Xaml.RoutedEventArgs e)
            => VirtualView.Clicked();

        public static void MapSource(ISKMenuFlyoutItemHandler handler, IMenuFlyoutItem view)
            => handler.PlatformView.Icon = view.Source?.ToIconSource(handler.MauiContext!)?.CreateIconElement();

        public static void MapText(ISKMenuFlyoutItemHandler handler, IMenuFlyoutItem view)
            => handler.PlatformView.Text = view.Text;

        public static void MapIsEnabled(ISKMenuFlyoutItemHandler handler, IMenuFlyoutItem view)
            => handler.PlatformView.UpdateIsEnabled(view.IsEnabled);
    }
}
