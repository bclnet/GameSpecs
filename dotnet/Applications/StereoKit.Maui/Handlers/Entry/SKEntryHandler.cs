using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.Entry;

namespace StereoKit.Maui.Handlers
{
    public partial class SKEntryHandler : ISKEntryHandler
    {
        public static IPropertyMapper<IEntry, ISKEntryHandler> Mapper = new PropertyMapper<IEntry, ISKEntryHandler>(SKViewHandler.ViewMapper)
        {
            [nameof(IEntry.Background)] = MapBackground,
            [nameof(IEntry.CharacterSpacing)] = MapCharacterSpacing,
            [nameof(IEntry.ClearButtonVisibility)] = MapClearButtonVisibility,
            [nameof(IEntry.Font)] = MapFont,
            [nameof(IEntry.IsPassword)] = MapIsPassword,
            [nameof(IEntry.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
            [nameof(IEntry.VerticalTextAlignment)] = MapVerticalTextAlignment,
            [nameof(IEntry.IsReadOnly)] = MapIsReadOnly,
            [nameof(IEntry.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
            [nameof(IEntry.Keyboard)] = MapKeyboard,
            [nameof(IEntry.MaxLength)] = MapMaxLength,
            [nameof(IEntry.Placeholder)] = MapPlaceholder,
            [nameof(IEntry.PlaceholderColor)] = MapPlaceholderColor,
            [nameof(IEntry.ReturnType)] = MapReturnType,
            [nameof(IEntry.Text)] = MapText,
            [nameof(IEntry.TextColor)] = MapTextColor,
            [nameof(IEntry.CursorPosition)] = MapCursorPosition,
            [nameof(IEntry.SelectionLength)] = MapSelectionLength
        };

        public static CommandMapper<IEntry, ISKEntryHandler> CommandMapper = new(ViewCommandMapper);

        public SKEntryHandler() : this(Mapper) { }

        public SKEntryHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKEntryHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IEntry ISKEntryHandler.VirtualView => VirtualView;

        PlatformView ISKEntryHandler.PlatformView => PlatformView;
    }
}
