using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.CheckBox;

namespace StereoKit.Maui.Handlers
{
	public partial class SKCheckBoxHandler : ISKCheckBoxHandler
	{
		public static IPropertyMapper<ICheckBox, ISKCheckBoxHandler> Mapper = new PropertyMapper<ICheckBox, ISKCheckBoxHandler>(SKViewHandler.ViewMapper)
		{
			[nameof(ICheckBox.IsChecked)] = MapIsChecked,
			[nameof(ICheckBox.Foreground)] = MapForeground,
		};

		public static CommandMapper<ICheckBox, SKCheckBoxHandler> CommandMapper = new(ViewCommandMapper);

		public SKCheckBoxHandler() : base(Mapper, CommandMapper) { }

		public SKCheckBoxHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

		public SKCheckBoxHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

		ICheckBox ISKCheckBoxHandler.VirtualView => VirtualView;

		PlatformView ISKCheckBoxHandler.PlatformView => PlatformView;
	}
}