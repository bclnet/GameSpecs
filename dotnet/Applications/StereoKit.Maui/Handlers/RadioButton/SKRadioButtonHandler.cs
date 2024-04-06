using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.RadioButton;

namespace StereoKit.Maui.Handlers
{
    public partial class SKRadioButtonHandler : ISKRadioButtonHandler
    {
        public static IPropertyMapper<IRadioButton, ISKRadioButtonHandler> Mapper = new PropertyMapper<IRadioButton, ISKRadioButtonHandler>(SKViewHandler.ViewMapper)
        {
			[nameof(IRadioButton.Background)] = MapBackground,
            [nameof(IRadioButton.IsChecked)] = MapIsChecked,
            [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
            [nameof(ITextStyle.Font)] = MapFont,
            [nameof(ITextStyle.TextColor)] = MapTextColor,
            [nameof(IRadioButton.Content)] = MapContent,
            [nameof(IRadioButton.StrokeColor)] = MapStrokeColor,
            [nameof(IRadioButton.StrokeThickness)] = MapStrokeThickness,
            [nameof(IRadioButton.CornerRadius)] = MapCornerRadius,
        };

        public static CommandMapper<IRadioButton, ISKRadioButtonHandler> CommandMapper = new(ViewCommandMapper);

        public SKRadioButtonHandler() : base(Mapper) { }

        public SKRadioButtonHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKRadioButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IRadioButton ISKRadioButtonHandler.VirtualView => VirtualView;

        PlatformView ISKRadioButtonHandler.PlatformView => PlatformView;
    }
}