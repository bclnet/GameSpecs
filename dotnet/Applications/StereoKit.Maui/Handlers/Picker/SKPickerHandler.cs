using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.ComboBox;

namespace StereoKit.Maui.Handlers
{
    public partial class SKPickerHandler : ISKPickerHandler
    {
        public static IPropertyMapper<IPicker, ISKPickerHandler> Mapper = new PropertyMapper<IPicker, SKPickerHandler>(ViewMapper)
        {
            [nameof(IPicker.Background)] = MapBackground,
            [nameof(IPicker.CharacterSpacing)] = MapCharacterSpacing,
            [nameof(IPicker.Font)] = MapFont,
            [nameof(IPicker.SelectedIndex)] = MapSelectedIndex,
            [nameof(IPicker.TextColor)] = MapTextColor,
            [nameof(IPicker.Title)] = MapTitle,
            [nameof(IPicker.TitleColor)] = MapTitleColor,
            [nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
            [nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
            [nameof(IPicker.Items)] = MapItems,
        };

        public static CommandMapper<IPicker, ISKPickerHandler> CommandMapper = new(ViewCommandMapper);

        public SKPickerHandler() : base(Mapper, CommandMapper) { }

        public SKPickerHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKPickerHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IPicker ISKPickerHandler.VirtualView => VirtualView;

        PlatformView ISKPickerHandler.PlatformView => PlatformView;
    }
}
