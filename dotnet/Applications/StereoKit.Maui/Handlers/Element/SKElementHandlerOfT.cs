using Microsoft.Maui;
using System;

namespace StereoKit.Maui.Handlers
{
    public abstract partial class SKElementHandler<TVirtualView, TPlatformView> : SKElementHandler, IElementHandler
        where TVirtualView : class, IElement
        where TPlatformView : class
    {
        [Microsoft.Maui.HotReload.OnHotReload]
        internal static void OnHotReload() { }

        protected SKElementHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper) { }

        public new TPlatformView PlatformView
        {
            get => (TPlatformView?)base.PlatformView ?? throw new InvalidOperationException($"PlatformView cannot be null here");
            private set => base.PlatformView = value;
        }

        public new TVirtualView VirtualView
        {
            get => (TVirtualView?)base.VirtualView ?? throw new InvalidOperationException($"VirtualView cannot be null here");
            private protected set => base.VirtualView = value;
        }

        IElement? IElementHandler.VirtualView => base.VirtualView;

        object? IElementHandler.PlatformView => base.PlatformView;

        protected abstract TPlatformView CreatePlatformElement();

        protected virtual void ConnectHandler(TPlatformView platformView) { }

        protected virtual void DisconnectHandler(TPlatformView platformView) { }

        private protected override object OnCreatePlatformElement() =>
            CreatePlatformElement();

        private protected override void OnConnectHandler(object platformView) =>
            ConnectHandler((TPlatformView)platformView);

        private protected override void OnDisconnectHandler(object platformView) =>
            DisconnectHandler((TPlatformView)platformView);
    }
}