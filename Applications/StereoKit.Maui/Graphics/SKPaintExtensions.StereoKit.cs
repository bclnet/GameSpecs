using Microsoft.Maui.Graphics;
using StereoKit.Maui.Platform;

namespace StereoKit.Maui.Graphics
{
    public static partial class SKPaintExtensions
    {
        public static Color ToSKPlatform(this Paint paint)
        {
            var color = paint.ToColor();
            return color != null ? color.ToPlatform() : Color.Black;
        }

        public static SKMauiDrawable? ToDrawable(this Paint paint)
        {
            if (paint is SolidPaint solidPaint) return solidPaint.CreateDrawable();
            if (paint is LinearGradientPaint linearGradientPaint) return linearGradientPaint.CreateDrawable();
            if (paint is RadialGradientPaint radialGradientPaint) return radialGradientPaint.CreateDrawable();
            if (paint is ImagePaint imagePaint) return imagePaint.CreateDrawable();
            if (paint is PatternPaint patternPaint) return patternPaint.CreateDrawable();
            return null;
        }

        public static SKMauiDrawable? CreateDrawable(this SolidPaint solidPaint)
            => new() { Background = solidPaint };

        public static SKMauiDrawable? CreateDrawable(this LinearGradientPaint linearGradientPaint)
            => !linearGradientPaint.IsValid()
            ? null
            : new() { Background = linearGradientPaint };

        public static SKMauiDrawable? CreateDrawable(this RadialGradientPaint radialGradientPaint)
            => !radialGradientPaint.IsValid()
            ? null
            : new() { Background = radialGradientPaint };

        public static SKMauiDrawable? CreateDrawable(this ImagePaint imagePaint)
            => new() { Background = imagePaint };

        public static SKMauiDrawable? CreateDrawable(this PatternPaint patternPaint)
            => new() { Background = patternPaint };

        static bool IsValid(this GradientPaint? gradientPaint)
            => gradientPaint?.GradientStops?.Length > 0;
    }
}