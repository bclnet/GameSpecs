using Microsoft.Maui;
using Microsoft.Maui.Devices;

namespace StereoKit.Maui
{
    public static partial class WindowExtensions
	{
		internal static DisplayOrientation GetOrientation(this IWindow? window) =>
			DeviceDisplay.Current.MainDisplayInfo.Orientation;
	}
}
