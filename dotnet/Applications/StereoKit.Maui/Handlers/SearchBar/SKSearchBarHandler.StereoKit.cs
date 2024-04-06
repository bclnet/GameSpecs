using Microsoft.Maui;
using PlatformView = StereoKit.UIX.Controls.MauiSearchBar;
using QueryEditor = StereoKit.UIX.Controls.Entry;

namespace StereoKit.Maui.Handlers
{
    public partial class SKSearchBarHandler : SKViewHandler<ISearchBar, PlatformView>
    {
        protected override PlatformView CreatePlatformView() => new();
        public QueryEditor? QueryEditor => new();

        public static void MapBackground(ISKSearchBarHandler handler, ISearchBar searchBar) { }
        public static void MapIsEnabled(ISKSearchBarHandler handler, ISearchBar searchBar) { }
        public static void MapText(IViewHandler handler, ISearchBar searchBar) { }
        public static void MapPlaceholder(IViewHandler handler, ISearchBar searchBar) { }
        public static void MapPlaceholderColor(IViewHandler handler, ISearchBar searchBar) { }
        public static void MapFont(IViewHandler handler, ISearchBar searchBar) { }
        public static void MapHorizontalTextAlignment(IViewHandler handler, ISearchBar searchBar) { }
        public static void MapVerticalTextAlignment(IViewHandler handler, ISearchBar searchBar) { }
        public static void MapCharacterSpacing(IViewHandler handler, ISearchBar searchBar) { }
        public static void MapTextColor(IViewHandler handler, ISearchBar searchBar) { }
        public static void MapCancelButtonColor(IViewHandler handler, ISearchBar searchBar) { }
        public static void MapIsTextPredictionEnabled(IViewHandler handler, ISearchBar searchBar) { }
        public static void MapMaxLength(IViewHandler handler, ISearchBar searchBar) { }
        public static void MapIsReadOnly(IViewHandler handler, ISearchBar searchBar) { }
        public static void MapKeyboard(IViewHandler handler, ISearchBar searchBar) { }
    }
}