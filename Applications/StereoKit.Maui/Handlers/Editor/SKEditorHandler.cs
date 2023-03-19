using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.Editor;

namespace StereoKit.Maui.Handlers
{
    public partial class SKEditorHandler : ISKEditorHandler
    {
        public static IPropertyMapper<IEditor, ISKEditorHandler> Mapper = new PropertyMapper<IEditor, ISKEditorHandler>(SKViewHandler.ViewMapper)
        {
            [nameof(IEditor.Background)] = MapBackground,
            [nameof(IEditor.CharacterSpacing)] = MapCharacterSpacing,
            [nameof(IEditor.Font)] = MapFont,
            [nameof(IEditor.IsReadOnly)] = MapIsReadOnly,
            [nameof(IEditor.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
            [nameof(IEditor.MaxLength)] = MapMaxLength,
            [nameof(IEditor.Placeholder)] = MapPlaceholder,
            [nameof(IEditor.PlaceholderColor)] = MapPlaceholderColor,
            [nameof(IEditor.Text)] = MapText,
            [nameof(IEditor.TextColor)] = MapTextColor,
            [nameof(IEditor.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
            [nameof(IEditor.VerticalTextAlignment)] = MapVerticalTextAlignment,
            [nameof(IEditor.Keyboard)] = MapKeyboard,
            [nameof(IEditor.CursorPosition)] = MapCursorPosition,
            [nameof(IEditor.SelectionLength)] = MapSelectionLength,
        };

        public static CommandMapper<IEditor, ISKEditorHandler> CommandMapper = new(ViewCommandMapper);

        public SKEditorHandler() : this(Mapper) { }

        public SKEditorHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKEditorHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IEditor ISKEditorHandler.VirtualView => VirtualView;

        PlatformView ISKEditorHandler.PlatformView => PlatformView;
    }
}
