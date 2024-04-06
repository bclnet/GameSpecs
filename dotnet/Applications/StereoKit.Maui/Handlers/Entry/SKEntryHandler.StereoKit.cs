using Microsoft.Maui;
using System;
using PlatformView = StereoKit.UIX.Controls.Entry;

namespace StereoKit.Maui.Handlers
{
	public partial class SKEntryHandler : SKViewHandler<IEntry, PlatformView>
	{
		protected override PlatformView CreatePlatformView() => new();

		public static void MapText(ISKEntryHandler handler, IEntry entry) { }
		public static void MapTextColor(ISKEntryHandler handler, IEntry entry) { }
		public static void MapIsPassword(ISKEntryHandler handler, IEntry entry) { }
		public static void MapHorizontalTextAlignment(ISKEntryHandler handler, IEntry entry) { }
		public static void MapVerticalTextAlignment(ISKEntryHandler handler, IEntry entry) { }
		public static void MapIsTextPredictionEnabled(ISKEntryHandler handler, IEntry entry) { }
		public static void MapMaxLength(ISKEntryHandler handler, IEntry entry) { }
		public static void MapPlaceholder(ISKEntryHandler handler, IEntry entry) { }
		public static void MapPlaceholderColor(ISKEntryHandler handler, IEntry entry) { }
		public static void MapIsReadOnly(ISKEntryHandler handler, IEntry entry) { }
		public static void MapKeyboard(ISKEntryHandler handler, IEntry entry) { }
		public static void MapFont(ISKEntryHandler handler, IEntry entry) { }
		public static void MapReturnType(ISKEntryHandler handler, IEntry entry) { }
		public static void MapClearButtonVisibility(ISKEntryHandler handler, IEntry entry) { }
		public static void MapCharacterSpacing(ISKEntryHandler handler, IEntry entry) { }
		public static void MapCursorPosition(ISKEntryHandler handler, IEntry entry) { }
		public static void MapSelectionLength(ISKEntryHandler handler, IEntry entry) { }
	}
}