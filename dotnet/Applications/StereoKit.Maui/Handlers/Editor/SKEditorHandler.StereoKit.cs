using Microsoft.Maui;
using System;
using PlatformView = StereoKit.UIX.Controls.Editor;

namespace StereoKit.Maui.Handlers
{
    public partial class SKEditorHandler : SKViewHandler<IEditor, PlatformView>
    {
        protected override PlatformView CreatePlatformView() => new();

        public static void MapText(IViewHandler handler, IEditor editor) { }
        public static void MapTextColor(IViewHandler handler, IEditor editor) { }
        public static void MapPlaceholder(IViewHandler handler, IEditor editor) { }
        public static void MapPlaceholderColor(IViewHandler handler, IEditor editor) { }
        public static void MapCharacterSpacing(IViewHandler handler, IEditor editor) { }
        public static void MapMaxLength(IViewHandler handler, IEditor editor) { }
        public static void MapIsTextPredictionEnabled(ISKEditorHandler handler, IEditor editor) { }
        public static void MapFont(IViewHandler handler, IEditor editor) { }
        public static void MapIsReadOnly(IViewHandler handler, IEditor editor) { }
        public static void MapTextColor(ISKEditorHandler handler, IEditor editor) { }
        public static void MapHorizontalTextAlignment(ISKEditorHandler handler, IEditor editor) { }
        public static void MapVerticalTextAlignment(ISKEditorHandler handler, IEditor editor) { }
        public static void MapKeyboard(ISKEditorHandler handler, IEditor editor) { }
        public static void MapCursorPosition(ISKEditorHandler handler, ITextInput editor) { }
        public static void MapSelectionLength(ISKEditorHandler handler, ITextInput editor) { }
    }
}
