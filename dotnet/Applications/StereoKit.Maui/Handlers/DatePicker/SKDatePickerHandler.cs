using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.DatePicker;

namespace StereoKit.Maui.Handlers
{
    public partial class SKDatePickerHandler : ISKDatePickerHandler
    {
        public static IPropertyMapper<IDatePicker, ISKDatePickerHandler> Mapper = new PropertyMapper<IDatePicker, ISKDatePickerHandler>(SKViewHandler.ViewMapper)
        {
            [nameof(IDatePicker.FlowDirection)] = MapFlowDirection,
            [nameof(IDatePicker.CharacterSpacing)] = MapCharacterSpacing,
            [nameof(IDatePicker.Date)] = MapDate,
            [nameof(IDatePicker.Font)] = MapFont,
            [nameof(IDatePicker.Format)] = MapFormat,
            [nameof(IDatePicker.MaximumDate)] = MapMaximumDate,
            [nameof(IDatePicker.MinimumDate)] = MapMinimumDate,
            [nameof(IDatePicker.TextColor)] = MapTextColor,
        };

        public static CommandMapper<IPicker, ISKDatePickerHandler> CommandMapper = new(ViewCommandMapper);

        public SKDatePickerHandler() : base(Mapper, CommandMapper) { }

        public SKDatePickerHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKDatePickerHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IDatePicker ISKDatePickerHandler.VirtualView => VirtualView;

        PlatformView ISKDatePickerHandler.PlatformView => PlatformView;
    }
}