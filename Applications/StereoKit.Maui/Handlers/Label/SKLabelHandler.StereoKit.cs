using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.Label;

namespace StereoKit.Maui.Handlers
{
    public partial class SKLabelHandler : SKViewHandler<ILabel, PlatformView>
	{
		protected override PlatformView CreatePlatformView() => new();

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