using Microsoft.Maui;
using System;
using NView = StereoKit.UIX.Controls.View;

namespace StereoKit.Maui
{
    public interface ISKPlatformViewHandler : IViewHandler, IDisposable
	{
		new NView? PlatformView { get; }

		new NView? ContainerView { get; }
	}
}