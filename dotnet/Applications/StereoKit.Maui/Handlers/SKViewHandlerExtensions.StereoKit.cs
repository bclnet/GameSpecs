using System;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using StereoKit.Maui.Platform;
using MRect = Microsoft.Maui.Graphics.Rect;

namespace StereoKit.Maui
{
    internal static partial class SKViewHandlerExtensions
    {
        internal static Size LayoutVirtualView(
            this ISKPlatformViewHandler viewHandler, MRect frame,
            Func<MRect, Size>? arrangeFunc = null)
        {
            var virtualView = viewHandler.VirtualView;
            var platformView = viewHandler.PlatformView;

            if (virtualView == null || platformView == null)
                return Size.Zero;

            arrangeFunc ??= virtualView.Arrange;
            return arrangeFunc(frame);
        }

        internal static Size MeasureVirtualView(
            this ISKPlatformViewHandler viewHandler,
            double widthConstraint,
            double heightConstraint,
            Func<double, double, Size>? measureFunc = null)
        {
            var virtualView = viewHandler.VirtualView;
            var platformView = viewHandler.PlatformView;

            if (virtualView == null || platformView == null)
                return Size.Zero;

            measureFunc ??= virtualView.Measure;
            var measure = measureFunc(widthConstraint, heightConstraint);

            return measure;
        }

        internal static Size GetDesiredSizeFromHandler(this IViewHandler viewHandler, double widthConstraint, double heightConstraint)
        {
            var platformView = viewHandler.ToPlatform();
            var virtualView = viewHandler.VirtualView;

            if (platformView == null || virtualView == null)
                return virtualView == null || double.IsNaN(virtualView.Width) || double.IsNaN(virtualView.Height) ? Size.Zero : new Size(virtualView.Width, virtualView.Height);

            int availableWidthAsInt = widthConstraint.ToScaledPixel();
            int availableHeightAsInt = heightConstraint.ToScaledPixel();

            double availableWidth = (availableWidthAsInt < 0 || availableWidthAsInt == int.MaxValue) ? double.PositiveInfinity : availableWidthAsInt;
            double availableHeight = (availableHeightAsInt < 0 || availableHeightAsInt == int.MaxValue) ? double.PositiveInfinity : availableHeightAsInt;

            double? explicitWidth = (virtualView.Width >= 0) ? virtualView.Width : null;
            double? explicitHeight = (virtualView.Height >= 0) ? virtualView.Height : null;

            var measured = platformView.Measured.ToDP();

            return new Size(explicitWidth ?? measured.Width, explicitHeight ?? measured.Height);
        }

        internal static void PlatformArrangeHandler(this IViewHandler viewHandler, MRect frame)
        {
            var platformView = viewHandler.ToPlatform();

            if (platformView == null)
                return;

            if (frame.Width < 0 || frame.Height < 0)
                // This is just some initial Forms value nonsense, nothing is actually laying out yet
                return;

            var bounds = frame.ToPixel();
            //if (platformView.Layout != null)
            //{
            //    platformView.Layout.MeasuredWidth = new NMeasuredSize(new NLayoutLength((float)bounds.Width), NMeasuredSize.StateType.MeasuredSizeOK);
            //    platformView.Layout.MeasuredHeight = new NMeasuredSize(new NLayoutLength((float)bounds.Height), NMeasuredSize.StateType.MeasuredSizeOK);
            //}
            platformView.UpdateBounds(bounds);

            viewHandler.Invoke(nameof(IView.Frame), frame);
        }
    }
}
