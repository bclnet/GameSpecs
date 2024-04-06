using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.Label;

namespace StereoKit.Maui.Handlers
{
    public partial class SKLabelHandler : ISKLabelHandler
    {
        public static IPropertyMapper<ILabel, ISKLabelHandler> Mapper = new PropertyMapper<ILabel, ISKLabelHandler>(SKViewHandler.ViewMapper)
        {
            [nameof(ILabel.Background)] = MapBackground,
            [nameof(ILabel.Height)] = MapHeight,
            [nameof(ILabel.Opacity)] = MapOpacity,
            [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
            [nameof(ITextStyle.Font)] = MapFont,
            [nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
            [nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
            [nameof(ILabel.LineHeight)] = MapLineHeight,
            [nameof(ILabel.Padding)] = MapPadding,
            [nameof(ILabel.Text)] = MapText,
            [nameof(ITextStyle.TextColor)] = MapTextColor,
            [nameof(ILabel.TextDecorations)] = MapTextDecorations,
        };

        public static CommandMapper<ILabel, ISKLabelHandler> CommandMapper = new(ViewCommandMapper);

        public SKLabelHandler() : base(Mapper, CommandMapper) { }

        public SKLabelHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKLabelHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        ILabel ISKLabelHandler.VirtualView => VirtualView;

        PlatformView ISKLabelHandler.PlatformView => (PlatformView)PlatformView;
    }
}