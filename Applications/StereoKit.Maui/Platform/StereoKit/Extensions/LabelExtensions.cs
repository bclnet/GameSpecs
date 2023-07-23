using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using StereoKit.UIX.Controls;

namespace StereoKit.Maui.Platform
{
    public static class LabelExtensions
    {
        public static void UpdateText(this Label platformLabel, ILabel label)
            => platformLabel.Text = label.Text ?? "";

        //public static void UpdateTextColor(this Label platformLabel, ILabel label)
        //    => platformLabel.TextColor = label.TextColor == null ? NColor.Black : label.TextColor.ToPlatform();

        //public static void UpdateFont(this Label platformLabel, ILabel label, IFontManager fontManager)
        //{
        //    platformLabel.FontSize = label.Font.Size > 0 ? label.Font.Size.ToScaledPoint() : 14d.ToScaledPoint();
        //    platformLabel.FontAttributes = label.Font.GetFontAttributes();
        //    platformLabel.FontFamily = fontManager.GetFontFamily(label.Font.Family) ?? "";
        //}

        //public static void UpdateHorizontalTextAlignment(this Label platformLabel, ILabel label)
        //    => platformLabel.HorizontalTextAlignment = label.HorizontalTextAlignment.ToPlatform();

        //public static void UpdateVerticalTextAlignment(this Label platformLabel, ILabel label)
        //    => platformLabel.VerticalTextAlignment = label.VerticalTextAlignment.ToPlatform();

        //public static void UpdateTextDecorations(this Label platformLabel, ILabel label)
        //    => platformLabel.TextDecorations = label.TextDecorations.ToPlatform();

        //public static void UpdateShadow(this Label platformLabel, IView view)
        //{
        //    if (view.Shadow != null)
        //    {
        //        var offsetX = view.Shadow.Offset.X.ToScaledPixel();
        //        var offsetY = view.Shadow.Offset.Y.ToScaledPixel();
        //        var radius = ((double)view.Shadow.Radius).ToScaledPixel();
        //        var color = view.Shadow.Paint.ToColor() != null ? view.Shadow.Paint.ToColor()!.MultiplyAlpha(view.Shadow.Opacity) : Colors.Black.MultiplyAlpha(view.Shadow.Opacity);
        //        var ncolor = color.ToPlatform().ToNative();

        //        PropertyMap shadow = new PropertyMap();
        //        shadow.Add("offset", new PropertyValue(new Vector2(offsetX, offsetY)));
        //        shadow.Add("color", new PropertyValue(ncolor));
        //        shadow.Add("blurRadius", new PropertyValue(radius));

        //        platformLabel.Shadow = shadow;
        //    }
        //    else
        //        platformLabel.Shadow = new PropertyMap();
        //}

        //public static void UpdateCharacterSpacing(this Label platformLabel, ILabel label)
        //    => platformLabel.CharacterSpacing = label.CharacterSpacing.ToScaledPixel();

        //public static void UpdateLineHeight(this Label platformLabel, ILabel label)
        //    => platformLabel.RelativeLineHeight = (float)label.LineHeight;

        //public static FontAttributes GetFontAttributes(this Font font)
        //{
        //    var attributes = font.Weight == FontWeight.Bold ? FontAttributes.Bold : FontAttributes.None;
        //    if (font.Slant != FontSlant.Default)
        //    {
        //        if (attributes == FontAttributes.None) attributes = FontAttributes.Italic;
        //        else attributes |= FontAttributes.Italic;
        //    }
        //    return attributes;
        //}

        //public static TLineBreakMode ToPlatform(this LineBreakMode mode)
        //    => mode switch
        //    {
        //        LineBreakMode.CharacterWrap => TLineBreakMode.CharacterWrap,
        //        LineBreakMode.HeadTruncation => TLineBreakMode.HeadTruncation,
        //        LineBreakMode.MiddleTruncation => TLineBreakMode.MiddleTruncation,
        //        LineBreakMode.NoWrap => TLineBreakMode.NoWrap,
        //        LineBreakMode.TailTruncation => TLineBreakMode.TailTruncation,
        //        _ => TLineBreakMode.WordWrap,
        //    };

        //public static TTextDecorationse ToPlatform(this TextDecorations td)
        //{
        //    if (td == TextDecorations.Strikethrough) return TTextDecorationse.Strikethrough;
        //    else if (td == TextDecorations.Underline) return TTextDecorationse.Underline;
        //    else return TTextDecorationse.None;
        //}
    }
}
