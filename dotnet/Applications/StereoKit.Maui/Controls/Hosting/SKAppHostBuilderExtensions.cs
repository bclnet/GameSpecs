using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using System;
using System.Diagnostics.CodeAnalysis;
using StereoKit.Maui.Handlers;

#if ANDROID
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
#elif WINDOWS
//using ResourcesProvider = Microsoft.Maui.Controls.Compatibility.Platform.UWP.WindowsResourcesProvider;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
#elif IOS || MACCATALYST
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Handlers.Compatibility;
#elif TIZEN
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
#endif

namespace Microsoft.Maui.Controls.Hosting
{
    public static class SKAppHostBuilderExtensions
    {
        public static MauiAppBuilder UseSKMauiApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder) where TApp : class, IApplication
        {
            builder.UseMauiApp<TApp>()
                .ConfigureMauiHandlers(handlers =>
                {
                    handlers.Clear();
                    handlers.AddXMauiControlsHandlers();
                });

            return builder;
        }

        public static MauiAppBuilder UseSKMauiApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder, Func<IServiceProvider, TApp> implementationFactory) where TApp : class, IApplication
        {
            builder.UseMauiApp<TApp>(implementationFactory)
                .ConfigureMauiHandlers(handlers =>
                {
                    handlers.Clear();
                    handlers.AddXMauiControlsHandlers();
                });

            return builder;
        }

        public static IMauiHandlersCollection AddXMauiControlsHandlers(this IMauiHandlersCollection handlersCollection)
        {
            handlersCollection.AddHandler<CollectionView, CollectionViewHandler>();
            handlersCollection.AddHandler<CarouselView, CarouselViewHandler>();
            handlersCollection.AddHandler<Application, SKApplicationHandler>();
            handlersCollection.AddHandler<ActivityIndicator, SKActivityIndicatorHandler>();
            handlersCollection.AddHandler<BoxView, SKShapeViewHandler>();
            handlersCollection.AddHandler<Button, SKButtonHandler>();
            handlersCollection.AddHandler<CheckBox, SKCheckBoxHandler>();
            handlersCollection.AddHandler<DatePicker, SKDatePickerHandler>();
            handlersCollection.AddHandler<Editor, SKEditorHandler>();
            handlersCollection.AddHandler<Entry, SKEntryHandler>();
            handlersCollection.AddHandler<GraphicsView, SKGraphicsViewHandler>();
            handlersCollection.AddHandler<Image, SKImageHandler>();
            handlersCollection.AddHandler<Label, SKLabelHandler>();
            handlersCollection.AddHandler<Layout, SKLayoutHandler>();
            handlersCollection.AddHandler<Picker, SKPickerHandler>();
            handlersCollection.AddHandler<ProgressBar, SKProgressBarHandler>();
            handlersCollection.AddHandler<ScrollView, SKScrollViewHandler>();
            handlersCollection.AddHandler<SearchBar, SKSearchBarHandler>();
            handlersCollection.AddHandler<Slider, SKSliderHandler>();
            handlersCollection.AddHandler<Stepper, SKStepperHandler>();
            handlersCollection.AddHandler<Switch, SKSwitchHandler>();
            handlersCollection.AddHandler<TimePicker, SKTimePickerHandler>();
            handlersCollection.AddHandler<Page, SKPageHandler>();
            handlersCollection.AddHandler<WebView, SKWebViewHandler>();
            handlersCollection.AddHandler<Border, SKBorderHandler>();
            handlersCollection.AddHandler<IContentView, SKContentViewHandler>();
            handlersCollection.AddHandler<Shapes.Ellipse, SKShapeViewHandler>();
            //handlersCollection.AddHandler<Shapes.Line, SKLineHandler>();
            //handlersCollection.AddHandler<Shapes.Path, SKPathHandler>();
            //handlersCollection.AddHandler<Shapes.Polygon, SKPolygonHandler>();
            //handlersCollection.AddHandler<Shapes.Polyline, SKPolylineHandler>();
            //handlersCollection.AddHandler<Shapes.Rectangle, SKRectangleHandler>();
            //handlersCollection.AddHandler<Shapes.RoundRectangle, SKRoundRectangleHandler>();
            handlersCollection.AddHandler<Window, SKWindowHandler>();
            handlersCollection.AddHandler<ImageButton, SKImageButtonHandler>();
            handlersCollection.AddHandler<IndicatorView, SKIndicatorViewHandler>();
            handlersCollection.AddHandler<RadioButton, SKRadioButtonHandler>();
            handlersCollection.AddHandler<RefreshView, SKRefreshViewHandler>();
            handlersCollection.AddHandler<SwipeItem, SKSwipeItemMenuItemHandler>();
            handlersCollection.AddHandler<SwipeView, SKSwipeViewHandler>();

#pragma warning disable CA1416 //  'MenuBarHandler', MenuFlyoutSubItemHandler, MenuFlyoutSubItemHandler, MenuBarItemHandler is only supported on: 'ios' 13.0 and later
            handlersCollection.AddHandler<MenuBar, SKMenuBarHandler>();
            handlersCollection.AddHandler<MenuFlyoutSubItem, SKMenuFlyoutSubItemHandler>();
            handlersCollection.AddHandler<MenuFlyoutSeparator, SKMenuFlyoutSeparatorHandler>();
            handlersCollection.AddHandler<MenuFlyoutItem, SKMenuFlyoutItemHandler>();
            handlersCollection.AddHandler<MenuBarItem, SKMenuBarItemHandler>();
#pragma warning restore CA1416

#if WINDOWS || ANDROID || IOS || MACCATALYST || TIZEN
            handlersCollection.AddHandler(typeof(ListView), typeof(Handlers.Compatibility.ListViewRenderer));
#if !TIZEN
            handlersCollection.AddHandler(typeof(Cell), typeof(Handlers.Compatibility.CellRenderer));
            handlersCollection.AddHandler(typeof(ImageCell), typeof(Handlers.Compatibility.ImageCellRenderer));
            handlersCollection.AddHandler(typeof(EntryCell), typeof(Handlers.Compatibility.EntryCellRenderer));
            handlersCollection.AddHandler(typeof(TextCell), typeof(Handlers.Compatibility.TextCellRenderer));
            handlersCollection.AddHandler(typeof(ViewCell), typeof(Handlers.Compatibility.ViewCellRenderer));
            handlersCollection.AddHandler(typeof(SwitchCell), typeof(Handlers.Compatibility.SwitchCellRenderer));
#endif
            handlersCollection.AddHandler(typeof(TableView), typeof(Handlers.Compatibility.TableViewRenderer));
            handlersCollection.AddHandler(typeof(Frame), typeof(Handlers.Compatibility.FrameRenderer));
#endif

#if WINDOWS || MACCATALYST
            handlersCollection.AddHandler(typeof(MenuFlyout), typeof(SKMenuFlyoutHandler));
#endif

#if IOS || MACCATALYST
			handlersCollection.AddHandler(typeof(NavigationPage), typeof(Handlers.Compatibility.NavigationRenderer));
			handlersCollection.AddHandler(typeof(TabbedPage), typeof(Handlers.Compatibility.TabbedRenderer));
			handlersCollection.AddHandler(typeof(FlyoutPage), typeof(Handlers.Compatibility.PhoneFlyoutPageRenderer));
#endif

#if ANDROID || IOS || MACCATALYST || TIZEN
			handlersCollection.AddHandler<SwipeItemView, SKSwipeItemViewHandler>();
#if ANDROID || IOS || MACCATALYST
			handlersCollection.AddHandler<Shell, ShellRenderer>();
#else
			handlersCollection.AddHandler<Shell, ShellHandler>();
			handlersCollection.AddHandler<ShellItem, ShellItemHandler>();
			handlersCollection.AddHandler<ShellSection, ShellSectionHandler>();
#endif
#endif
#if WINDOWS || ANDROID || TIZEN
            handlersCollection.AddHandler<NavigationPage, SKNavigationViewHandler>();
            handlersCollection.AddHandler<Toolbar, SKToolbarHandler>();
            handlersCollection.AddHandler<FlyoutPage, SKFlyoutViewHandler>();
            handlersCollection.AddHandler<TabbedPage, SKTabbedViewHandler>();
#endif

#if WINDOWS
            handlersCollection.AddHandler<ShellItem, ShellItemHandler>();
            handlersCollection.AddHandler<ShellSection, ShellSectionHandler>();
            handlersCollection.AddHandler<ShellContent, ShellContentHandler>();
            handlersCollection.AddHandler<Shell, ShellHandler>();
#endif
            return handlersCollection;
        }
    }
}