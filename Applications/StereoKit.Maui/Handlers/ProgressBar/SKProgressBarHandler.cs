using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.ProgressBar;

namespace StereoKit.Maui.Handlers
{
    public partial class SKProgressBarHandler : ISKProgressBarHandler
    {
        public static IPropertyMapper<IProgress, ISKProgressBarHandler> Mapper = new PropertyMapper<IProgress, SKProgressBarHandler>(SKViewHandler.ViewMapper)
        {
            [nameof(IProgress.Progress)] = MapProgress,
            [nameof(IProgress.ProgressColor)] = MapProgressColor
        };

        public static CommandMapper<IProgress, ISKProgressBarHandler> CommandMapper = new(ViewCommandMapper);

        public SKProgressBarHandler() : base(Mapper) { }

        public SKProgressBarHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper) { }

        public SKProgressBarHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IProgress ISKProgressBarHandler.VirtualView => VirtualView;

        PlatformView ISKProgressBarHandler.PlatformView => PlatformView;
    }
}