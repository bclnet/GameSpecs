using System;
using Microsoft.Maui;
using Microsoft.Maui.HotReload;
using StereoKit.UIX.Controls;
using NView = StereoKit.UIX.Controls.View;

namespace StereoKit.Maui.Platform
{
    public class ContainerView : ViewGroup, IReloadHandler
    {
        readonly IMauiContext? _context;

        IElement? _view;

        public ContainerView(IMauiContext context)
            => _context = context;

        public IElement? CurrentView
        {
            get => _view;
            set => SetView(value);
        }

        public NView? CurrentPlatformView { get; private set; }

        void SetView(IElement? view, bool forceRefresh = false)
        {
            if (view == _view && !forceRefresh)
                return;

            _view = view;

            if (_view is IHotReloadableView ihr)
            {
                ihr.ReloadHandler = this;
                MauiHotReloadHelper.AddActiveView(ihr);
            }

            Children.Clear();
            CurrentPlatformView = null;

            if (_view != null)
            {
                _ = _context ?? throw new ArgumentNullException(nameof(_context));
                var nativeView = _view.ToSKPlatform(_context);
                nativeView.WidthSpecification = LayoutParamPolicies.MatchParent;
                nativeView.HeightSpecification = LayoutParamPolicies.MatchParent;
                Children.Add(nativeView);
                CurrentPlatformView = nativeView;
            }
        }

        public void Reload() => SetView(CurrentView, true);
    }
}
