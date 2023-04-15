using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using StereoKit.UIX.Views;
using System;
using MRect = Microsoft.Maui.Graphics.Rect;
using NView = StereoKit.UIX.Controls.View;
using StereoKit.Maui.Platform;

namespace StereoKit.Maui.Handlers
{
    public abstract partial class SKViewHandler<TVirtualView, TPlatformView> : ISKPlatformViewHandler
    {
        public override void PlatformArrange(MRect rect) => this.PlatformArrangeHandler(rect);

        public override Size GetDesiredSize(double widthConstraint, double heightConstraint) => this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);

        protected override void SetupContainer()
        {
            if (PlatformView == null || ContainerView != null)
                return;

            var oldParent = (Panel?)PlatformView.Parent;
            var oldIndex = oldParent?.Children.IndexOf(PlatformView);
            if (oldIndex is int oldIdx && oldIdx >= 0)
                oldParent?.Children.RemoveAt(oldIdx);

            ContainerView ??= new WrapperView();
            ((WrapperView)ContainerView).Child = PlatformView;

            if (oldIndex is int idx && idx >= 0)
                oldParent?.Children.Insert(idx, ContainerView);
            else
                oldParent?.Children.Add(ContainerView);
        }

        protected override void RemoveContainer()
        {
            if (PlatformView == null || ContainerView == null || PlatformView.Parent != ContainerView)
            {
                CleanupContainerView(ContainerView);
                ContainerView = null;
                return;
            }

            var oldParent = (Panel?)ContainerView.Parent;
            var oldIndex = oldParent?.Children.IndexOf(ContainerView);
            if (oldIndex is int oldIdx && oldIdx >= 0)
                oldParent?.Children.RemoveAt(oldIdx);

            CleanupContainerView(ContainerView);
            ContainerView = null;

            if (oldIndex is int idx && idx >= 0)
                oldParent?.Children.Insert(idx, PlatformView);
            else
                oldParent?.Children.Add(PlatformView);

            void CleanupContainerView(NView? containerView)
            {
                if (containerView is WrapperView wrapperView)
                {
                    wrapperView.Child = null;
                    wrapperView.Dispose();
                }
            }
        }
    }
}