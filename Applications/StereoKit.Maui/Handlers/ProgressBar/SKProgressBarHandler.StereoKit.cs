using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.ProgressBar;

namespace StereoKit.Maui.Handlers
{
	public partial class SKProgressBarHandler : SKViewHandler<IProgress, PlatformView>
	{
		protected override PlatformView CreatePlatformView() => new();

		public static void MapProgress(ISKProgressBarHandler handler, IProgress progress) { }
		public static void MapProgressColor(ISKProgressBarHandler handler, IProgress progress) { }
	}
}