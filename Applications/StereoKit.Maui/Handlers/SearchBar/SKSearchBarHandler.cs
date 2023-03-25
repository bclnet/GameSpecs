using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MauiSearchBar;

namespace StereoKit.Maui.Handlers
{
    public partial class SKSearchBarHandler : ISKSearchBarHandler
    {
        public static IPropertyMapper<ISearchBar, ISKSearchBarHandler> Mapper = new PropertyMapper<ISearchBar, ISKSearchBarHandler>(SKViewHandler.ViewMapper)
        {
            [nameof(ISearchBar.Background)] = MapBackground,
            [nameof(ISearchBar.CharacterSpacing)] = MapCharacterSpacing,
            [nameof(ISearchBar.Font)] = MapFont,
            [nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
            [nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
            [nameof(ISearchBar.IsReadOnly)] = MapIsReadOnly,
            [nameof(ISearchBar.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
            [nameof(ISearchBar.MaxLength)] = MapMaxLength,
            [nameof(ISearchBar.Placeholder)] = MapPlaceholder,
            [nameof(ISearchBar.PlaceholderColor)] = MapPlaceholderColor,
            [nameof(ISearchBar.Text)] = MapText,
            [nameof(ISearchBar.TextColor)] = MapTextColor,
            [nameof(ISearchBar.CancelButtonColor)] = MapCancelButtonColor,
            [nameof(ISearchBar.Keyboard)] = MapKeyboard
        };

        public static CommandMapper<ISearchBar, ISKSearchBarHandler> CommandMapper = new(ViewCommandMapper)
        {
            [nameof(ISearchBar.Focus)] = MapFocus
        };

        public SKSearchBarHandler() : this(Mapper) { }

        public SKSearchBarHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKSearchBarHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        ISearchBar ISKSearchBarHandler.VirtualView => VirtualView;

        PlatformView ISKSearchBarHandler.PlatformView => PlatformView;
    }
}