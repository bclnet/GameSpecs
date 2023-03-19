using Microsoft.Maui;

namespace StereoKit.Maui.Handlers
{
    public partial class SKLabelHandler : SKViewHandler<ILabel, object>
	{
		protected override object CreatePlatformView() => new();

		public static void MapText(ISKLabelHandler handler, ILabel label) { }
		public static void MapTextColor(ISKLabelHandler handler, ILabel label) { }
		public static void MapCharacterSpacing(ISKLabelHandler handler, ILabel label) { }
		public static void MapFont(ISKLabelHandler handler, ILabel label) { }
		public static void MapHorizontalTextAlignment(ISKLabelHandler handler, ILabel label) { }
		public static void MapVerticalTextAlignment(ISKLabelHandler handler, ILabel label) { }
		public static void MapTextDecorations(ISKLabelHandler handler, ILabel label) { }
		public static void MapMaxLines(ISKLabelHandler handler, ILabel label) { }
		public static void MapPadding(ISKLabelHandler handler, ILabel label) { }
		public static void MapLineHeight(ISKLabelHandler handler, ILabel label) { }
	}
}