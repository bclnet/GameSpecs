using Microsoft.Maui;
using System;
using PlatformView = StereoKit.UIX.Controls.View;

namespace StereoKit.Maui.Handlers
{
    public abstract partial class SKViewHandler<TVirtualView, TPlatformView> : SKViewHandler, IViewHandler
		where TVirtualView : class, IView
		where TPlatformView : PlatformView
	{
		[Microsoft.Maui.HotReload.OnHotReload]
		internal static void OnHotReload() { }

		protected SKViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper) { }

		public new TPlatformView PlatformView
		{
			get => (TPlatformView?)base.PlatformView ?? throw new InvalidOperationException($"PlatformView cannot be null here");
			private protected set => base.PlatformView = value;
		}

		public new TVirtualView VirtualView
		{
			get => (TVirtualView?)base.VirtualView ?? throw new InvalidOperationException($"VirtualView cannot be null here");
			private protected set => base.VirtualView = value;
		}

		IView? IViewHandler.VirtualView => base.VirtualView;

		IElement? IElementHandler.VirtualView => base.VirtualView;

		object? IElementHandler.PlatformView => base.PlatformView;

		public virtual void SetVirtualView(IView view) =>
			base.SetVirtualView(view);

		public sealed override void SetVirtualView(IElement view) =>
			SetVirtualView((IView)view);

		public static Func<SKViewHandler<TVirtualView, TPlatformView>, TPlatformView>? PlatformViewFactory { get; set; }

		protected abstract TPlatformView CreatePlatformView();

		protected virtual void ConnectHandler(TPlatformView platformView) { }

		protected virtual void DisconnectHandler(TPlatformView platformView) { }

		private protected override PlatformView OnCreatePlatformView()
			=> PlatformViewFactory?.Invoke(this) ?? CreatePlatformView();

		private protected override void OnConnectHandler(PlatformView platformView) =>
			ConnectHandler((TPlatformView)platformView);

		private protected override void OnDisconnectHandler(PlatformView platformView) =>
			DisconnectHandler((TPlatformView)platformView);
	}
}