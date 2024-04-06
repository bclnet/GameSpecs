using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.TimePicker;

namespace StereoKit.Maui.Handlers
{
	public partial class SKTimePickerHandler : ISKTimePickerHandler
    {
		public static IPropertyMapper<ITimePicker, ISKTimePickerHandler> Mapper = new PropertyMapper<ITimePicker, ISKTimePickerHandler>(SKViewHandler.ViewMapper)
		{
			[nameof(ITimePicker.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ITimePicker.Font)] = MapFont,
			[nameof(ITimePicker.Format)] = MapFormat,
			[nameof(ITimePicker.TextColor)] = MapTextColor,
			[nameof(ITimePicker.Time)] = MapTime,
		};

		public static CommandMapper<ITimePicker, ISKTimePickerHandler> CommandMapper = new(ViewCommandMapper);

		public SKTimePickerHandler() : base(Mapper) { }

		public SKTimePickerHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

		public SKTimePickerHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

		ITimePicker ISKTimePickerHandler.VirtualView => VirtualView;

		PlatformView ISKTimePickerHandler.PlatformView => PlatformView;
	}
}