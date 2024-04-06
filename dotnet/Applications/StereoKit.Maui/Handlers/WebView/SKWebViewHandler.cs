using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.WebView;

namespace StereoKit.Maui.Handlers
{
    public partial class SKWebViewHandler : ISKWebViewHandler
    {
        public static IPropertyMapper<IWebView, ISKWebViewHandler> Mapper = new PropertyMapper<IWebView, ISKWebViewHandler>(SKViewHandler.ViewMapper)
        {
            [nameof(IWebView.Source)] = MapSource,
            //[nameof(IWebView.UserAgent)] = MapUserAgent,
        };

        public static CommandMapper<IWebView, ISKWebViewHandler> CommandMapper = new(ViewCommandMapper)
        {
            [nameof(IWebView.GoBack)] = MapGoBack,
            [nameof(IWebView.GoForward)] = MapGoForward,
            [nameof(IWebView.Reload)] = MapReload,
            [nameof(IWebView.Eval)] = MapEval,
            [nameof(IWebView.EvaluateJavaScriptAsync)] = MapEvaluateJavaScriptAsync,
        };

        public SKWebViewHandler() : base(Mapper, CommandMapper) { }

        public SKWebViewHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null) : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

        IWebView ISKWebViewHandler.VirtualView => VirtualView;

        PlatformView ISKWebViewHandler.PlatformView => PlatformView;
    }
}