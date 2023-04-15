using System;
using Microsoft.Maui.Graphics;
using MRect = Microsoft.Maui.Graphics.Rect;
using DSizeF = System.Drawing.SizeF;
using Microsoft.Maui.Devices;

namespace StereoKit.Maui.Platform
{
    public static class DPExtensions
    {
        const double DPI = 160.0;
        internal const double ScalingFactor = 1.0; // DeviceInfo.ScalingFactor;

        public static MRect ToDP(this Rect rect)
            => new(ConvertToScaledDP(rect.x), ConvertToScaledDP(rect.y), ConvertToScaledDP(rect.width), ConvertToScaledDP(rect.height));

        public static Rect ToPixel(this MRect rect)
            => new(ConvertToScaledPixel(rect.X), ConvertToScaledPixel(rect.Y), ConvertToScaledPixel(rect.Width), ConvertToScaledPixel(rect.Height));

        public static Point ToPixel(this Point point)
            => new(ConvertToScaledPixel(point.X), ConvertToScaledPixel(point.Y));

        public static Size ToDP(this DSizeF size)
            => new(ConvertToScaledDP(size.Width), ConvertToScaledDP(size.Height));

        public static DSizeF ToPixel(this Size size)
            => new(ConvertToScaledPixel(size.Width), ConvertToScaledPixel(size.Height));

        public static int ToPixel(this double dp)
            => (int)Math.Round(dp * DPI / 160.0);

        public static int ToScaledPixel(this double dp)
        {
            if (double.IsPositiveInfinity(dp))
                return int.MaxValue;
            return (int)Math.Round(dp * ScalingFactor);
        }

        public static double ToScaledDP(this int pixel)
            => pixel / ScalingFactor;

        public static double ToScaledDP(this double pixel)
            => pixel / ScalingFactor;

        public static double ToPoint(this double dp)
            => dp * 72 / 160.0;

        public static double ToScaledPoint(this double dp)
            => dp.ToScaledPixel() * 72 / DPI;

        public static int ConvertToPixel(double dp)
            => (int)Math.Round(dp * DPI / 160.0);

        public static int ConvertToScaledPixel(double dp)
        {
            if (double.IsPositiveInfinity(dp))
                return int.MaxValue;
            return (int)Math.Round(dp * ScalingFactor);
        }

        public static double ConvertToScaledDP(int pixel)
        {
            if (pixel == int.MaxValue)
                return double.PositiveInfinity;
            return pixel / ScalingFactor;
        }

        public static double ConvertToScaledDP(double pixel)
            => pixel / ScalingFactor;

        public static double ConvertToDPFont(int pt)
            => ConvertToScaledDP(pt * DPI / 72.0);
    }
}
