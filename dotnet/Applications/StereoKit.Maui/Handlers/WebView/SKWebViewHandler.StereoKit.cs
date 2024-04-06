using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.WebView;

namespace StereoKit.Maui.Handlers
{
    public partial class SKWebViewHandler : SKViewHandler<IWebView, PlatformView>
    {
        protected override PlatformView CreatePlatformView() => new();

        public static void MapSource(ISKWebViewHandler handler, IWebView webView) { }
        public static void MapUserAgent(ISKWebViewHandler handler, IWebView webView) { }

        public static void MapGoBack(ISKWebViewHandler handler, IWebView webView, object? arg) { }
        public static void MapGoForward(ISKWebViewHandler handler, IWebView webView, object? arg) { }
        public static void MapReload(ISKWebViewHandler handler, IWebView webView, object? arg) { }
        public static void MapEval(ISKWebViewHandler handler, IWebView webView, object? arg) { }
        public static void MapEvaluateJavaScriptAsync(ISKWebViewHandler handler, IWebView webView, object? arg) { }
    }
}