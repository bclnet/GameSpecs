using Microsoft.Maui;
using StereoKit.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using NView = StereoKit.UIX.Controls.View;
using PlatformView = StereoKit.Maui.Platform.LayoutViewGroup;

namespace StereoKit.Maui.Handlers
{
    public partial class SKLayoutHandler : SKViewHandler<ILayout, PlatformView>
    {
        protected override PlatformView CreatePlatformView()
        {
            if (VirtualView == null)
                throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutViewGroup");

            return new(VirtualView)
            {
                CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
                CrossPlatformArrange = VirtualView.CrossPlatformArrange
            };
        }

        public override void SetVirtualView(IView view)
        {
            base.SetVirtualView(view);

            _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
            _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
            _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

            PlatformView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
            PlatformView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;

            PlatformView.Children.Clear();

            foreach (var child in VirtualView.OrderByZIndex())
                PlatformView.Children.Add(child.ToSKPlatform(MauiContext));
        }

        public void Add(IView child)
        {
            _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
            _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
            _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

            var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
            PlatformView.Children.Insert(targetIndex, child.ToSKPlatform(MauiContext));

            EnsureZIndexOrder(child);
            PlatformView.SetNeedMeasureUpdate();
        }

        public void Remove(IView child)
        {
            _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
            _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

            if (child?.ToSKPlatform() is NView childView)
                PlatformView.Children.Remove(childView);

            PlatformView.MarkChanged();
            PlatformView.SetNeedMeasureUpdate();
        }

        public void Clear()
        {
            if (PlatformView == null)
                return;

            PlatformView.Children.Clear();

            PlatformView.SetNeedMeasureUpdate();
        }

        public void Insert(int index, IView child)
        {
            _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
            _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
            _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

            var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
            PlatformView.Children.Insert(targetIndex, child.ToSKPlatform(MauiContext));

            EnsureZIndexOrder(child);
            PlatformView.SetNeedMeasureUpdate();
        }

        public void Update(int index, IView child)
        {
            _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
            _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
            _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

            PlatformView.Children[index] = child.ToSKPlatform(MauiContext);
            EnsureZIndexOrder(child);
            PlatformView.SetNeedMeasureUpdate();
        }

        public void UpdateZIndex(IView child)
        {
            _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
            _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
            _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

            EnsureZIndexOrder(child);
        }

        void EnsureZIndexOrder(IView child)
        {
            if (PlatformView.Children.Count == 0)
                return;

            var currentIndex = PlatformView.Children.IndexOf(child.ToSKPlatform(MauiContext!));
            if (currentIndex == -1)
                return;

            var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
            if (currentIndex != targetIndex)
                PlatformView.Children.Move(currentIndex, targetIndex);
        }
    }
}
