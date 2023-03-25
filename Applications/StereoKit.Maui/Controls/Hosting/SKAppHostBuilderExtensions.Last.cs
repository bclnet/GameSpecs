using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using StereoKit.Maui.Handlers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

#if ANDROID
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
#elif WINDOWS
using ResourcesProvider = Microsoft.Maui.Controls.Compatibility.Platform.UWP.WindowsResourcesProvider;
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
        static readonly Type AppHostBuilderExtensionsType = typeof(AppThemeBindingExtension).Assembly.GetType("Microsoft.Maui.Controls.Hosting.AppHostBuilderExtensions")!;
        static readonly MethodInfo AddMauiControlsHandlersMethod = AppHostBuilderExtensionsType.GetMethod("AddMauiControlsHandlers", BindingFlags.Static | BindingFlags.NonPublic)!;
        static readonly MethodInfo ConfigureImageSourceHandlersMethod = AppHostBuilderExtensionsType.GetMethod(nameof(ConfigureImageSourceHandlers), BindingFlags.Static | BindingFlags.NonPublic)!;
        static readonly MethodInfo RemapForControlsMethod = AppHostBuilderExtensionsType.GetMethod(nameof(RemapForControls), BindingFlags.Static | BindingFlags.NonPublic)!;

        public static MauiAppBuilder UseSKMauiApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder) where TApp : class, IApplication
        {
#pragma warning disable RS0030 // Do not used banned APIs - don't want to use a factory method here
            builder.Services.TryAddSingleton<IApplication, TApp>();
#pragma warning restore RS0030
            builder.SetupDefaults();
            return builder;
        }

        public static MauiAppBuilder UseSKMauiApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder, Func<IServiceProvider, TApp> implementationFactory) where TApp : class, IApplication
        {
            builder.Services.TryAddSingleton<IApplication>(implementationFactory);
            builder.SetupDefaults();
            return builder;
        }

        public static IMauiHandlersCollection AddSKMauiControlsHandlers(this IMauiHandlersCollection handlersCollection)
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
            handlersCollection.AddHandler<Shapes.Line, LineHandler>();
            handlersCollection.AddHandler<Shapes.Path, PathHandler>();
            handlersCollection.AddHandler<Shapes.Polygon, PolygonHandler>();
            handlersCollection.AddHandler<Shapes.Polyline, PolylineHandler>();
            handlersCollection.AddHandler<Shapes.Rectangle, RectangleHandler>();
            handlersCollection.AddHandler<Shapes.RoundRectangle, RoundRectangleHandler>();
            handlersCollection.AddHandler<Window, SKWindowHandler>();
            handlersCollection.AddHandler<ImageButton, SKImageButtonHandler>();
            handlersCollection.AddHandler<IndicatorView, SKIndicatorViewHandler>();
            handlersCollection.AddHandler<RadioButton, SKRadioButtonHandler>();
            handlersCollection.AddHandler<RefreshView, SKRefreshViewHandler>();
            handlersCollection.AddHandler<SwipeItem, SKSwipeItemMenuItemHandler>();
            handlersCollection.AddHandler<SwipeView, SKSwipeViewHandler>();

#pragma warning disable CA1416 //  'MenuBarHandler', MenuFlyoutSubItemHandler, MenuFlyoutSubItemHandler, MenuBarItemHandler is only supported on: 'ios' 13.0 and later
            handlersCollection.AddHandler<MenuBar, SKMenuBarHandler>();
            handlersCollection.AddHandler<MenuFlyoutSubItem, MenuFlyoutSubItemHandler>();
            handlersCollection.AddHandler<MenuFlyoutSeparator, MenuFlyoutSeparatorHandler>();
            handlersCollection.AddHandler<MenuFlyoutItem, MenuFlyoutItemHandler>();
            handlersCollection.AddHandler<MenuBarItem, MenuBarItemHandler>();
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
			handlersCollection.AddHandler(typeof(MenuFlyout), typeof(MenuFlyoutHandler));
#endif

#if IOS || MACCATALYST
			handlersCollection.AddHandler(typeof(NavigationPage), typeof(Handlers.Compatibility.NavigationRenderer));
			handlersCollection.AddHandler(typeof(TabbedPage), typeof(Handlers.Compatibility.TabbedRenderer));
			handlersCollection.AddHandler(typeof(FlyoutPage), typeof(Handlers.Compatibility.PhoneFlyoutPageRenderer));
#endif

#if ANDROID || IOS || MACCATALYST || TIZEN
			handlersCollection.AddHandler<SwipeItemView, SwipeItemViewHandler>();
#if ANDROID || IOS || MACCATALYST
			handlersCollection.AddHandler<Shell, ShellRenderer>();
#else
			handlersCollection.AddHandler<Shell, ShellHandler>();
			handlersCollection.AddHandler<ShellItem, ShellItemHandler>();
			handlersCollection.AddHandler<ShellSection, ShellSectionHandler>();
#endif
#endif
#if WINDOWS || ANDROID || TIZEN
			handlersCollection.AddHandler<NavigationPage, NavigationViewHandler>();
			handlersCollection.AddHandler<Toolbar, ToolbarHandler>();
			handlersCollection.AddHandler<FlyoutPage, FlyoutViewHandler>();
			handlersCollection.AddHandler<TabbedPage, TabbedViewHandler>();
#endif

#if WINDOWS
			handlersCollection.AddHandler<ShellItem, ShellItemHandler>();
			handlersCollection.AddHandler<ShellSection, ShellSectionHandler>();
			handlersCollection.AddHandler<ShellContent, ShellContentHandler>();
			handlersCollection.AddHandler<Shell, ShellHandler>();
#endif
            return handlersCollection;
        }

        public static IMauiHandlersCollection AddXMauiControlsHandlers(this IMauiHandlersCollection handlersCollection)
            => (IMauiHandlersCollection)AddMauiControlsHandlersMethod.Invoke(null, new[] { handlersCollection })!;

        public static IMauiHandlersCollection AddXYMauiControlsHandlers(this IMauiHandlersCollection handlersCollection)
        {
            handlersCollection.AddHandler<CollectionView, CollectionViewHandler>();
            handlersCollection.AddHandler<CarouselView, CarouselViewHandler>();
            handlersCollection.AddHandler<Application, ApplicationHandler>();
            handlersCollection.AddHandler<ActivityIndicator, ActivityIndicatorHandler>();
            handlersCollection.AddHandler<BoxView, ShapeViewHandler>();
            handlersCollection.AddHandler<Button, ButtonHandler>();
            handlersCollection.AddHandler<CheckBox, CheckBoxHandler>();
            handlersCollection.AddHandler<DatePicker, DatePickerHandler>();
            handlersCollection.AddHandler<Editor, EditorHandler>();
            handlersCollection.AddHandler<Entry, EntryHandler>();
            handlersCollection.AddHandler<GraphicsView, GraphicsViewHandler>();
            handlersCollection.AddHandler<Image, ImageHandler>();
            handlersCollection.AddHandler<Label, LabelHandler>();
            handlersCollection.AddHandler<Layout, LayoutHandler>();
            handlersCollection.AddHandler<Picker, PickerHandler>();
            handlersCollection.AddHandler<ProgressBar, ProgressBarHandler>();
            handlersCollection.AddHandler<ScrollView, ScrollViewHandler>();
            handlersCollection.AddHandler<SearchBar, SearchBarHandler>();
            handlersCollection.AddHandler<Slider, SliderHandler>();
            handlersCollection.AddHandler<Stepper, StepperHandler>();
            handlersCollection.AddHandler<Switch, SwitchHandler>();
            handlersCollection.AddHandler<TimePicker, TimePickerHandler>();
            handlersCollection.AddHandler<Page, PageHandler>();
            handlersCollection.AddHandler<WebView, WebViewHandler>();
            handlersCollection.AddHandler<Border, BorderHandler>();
            handlersCollection.AddHandler<IContentView, ContentViewHandler>();
            handlersCollection.AddHandler<Shapes.Ellipse, ShapeViewHandler>();
            handlersCollection.AddHandler<Shapes.Line, LineHandler>();
            handlersCollection.AddHandler<Shapes.Path, PathHandler>();
            handlersCollection.AddHandler<Shapes.Polygon, PolygonHandler>();
            handlersCollection.AddHandler<Shapes.Polyline, PolylineHandler>();
            handlersCollection.AddHandler<Shapes.Rectangle, RectangleHandler>();
            handlersCollection.AddHandler<Shapes.RoundRectangle, RoundRectangleHandler>();
            handlersCollection.AddHandler<Window, WindowHandler>();
            handlersCollection.AddHandler<ImageButton, ImageButtonHandler>();
            handlersCollection.AddHandler<IndicatorView, IndicatorViewHandler>();
            handlersCollection.AddHandler<RadioButton, RadioButtonHandler>();
            handlersCollection.AddHandler<RefreshView, RefreshViewHandler>();
            handlersCollection.AddHandler<SwipeItem, SwipeItemMenuItemHandler>();
            handlersCollection.AddHandler<SwipeView, SwipeViewHandler>();

#pragma warning disable CA1416 //  'MenuBarHandler', MenuFlyoutSubItemHandler, MenuFlyoutSubItemHandler, MenuBarItemHandler is only supported on: 'ios' 13.0 and later
            handlersCollection.AddHandler<MenuBar, MenuBarHandler>();
            handlersCollection.AddHandler<MenuFlyoutSubItem, MenuFlyoutSubItemHandler>();
            handlersCollection.AddHandler<MenuFlyoutSeparator, MenuFlyoutSeparatorHandler>();
            handlersCollection.AddHandler<MenuFlyoutItem, MenuFlyoutItemHandler>();
            handlersCollection.AddHandler<MenuBarItem, MenuBarItemHandler>();
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
			handlersCollection.AddHandler(typeof(MenuFlyout), typeof(MenuFlyoutHandler));
#endif

#if IOS || MACCATALYST
			handlersCollection.AddHandler(typeof(NavigationPage), typeof(Handlers.Compatibility.NavigationRenderer));
			handlersCollection.AddHandler(typeof(TabbedPage), typeof(Handlers.Compatibility.TabbedRenderer));
			handlersCollection.AddHandler(typeof(FlyoutPage), typeof(Handlers.Compatibility.PhoneFlyoutPageRenderer));
#endif

#if ANDROID || IOS || MACCATALYST || TIZEN
			handlersCollection.AddHandler<SwipeItemView, SwipeItemViewHandler>();
#if ANDROID || IOS || MACCATALYST
			handlersCollection.AddHandler<Shell, ShellRenderer>();
#else
			handlersCollection.AddHandler<Shell, ShellHandler>();
			handlersCollection.AddHandler<ShellItem, ShellItemHandler>();
			handlersCollection.AddHandler<ShellSection, ShellSectionHandler>();
#endif
#endif
#if WINDOWS || ANDROID || TIZEN
			handlersCollection.AddHandler<NavigationPage, NavigationViewHandler>();
			handlersCollection.AddHandler<Toolbar, ToolbarHandler>();
			handlersCollection.AddHandler<FlyoutPage, FlyoutViewHandler>();
			handlersCollection.AddHandler<TabbedPage, TabbedViewHandler>();
#endif

#if WINDOWS
			handlersCollection.AddHandler<ShellItem, ShellItemHandler>();
			handlersCollection.AddHandler<ShellSection, ShellSectionHandler>();
			handlersCollection.AddHandler<ShellContent, ShellContentHandler>();
			handlersCollection.AddHandler<Shell, ShellHandler>();
#endif
            return handlersCollection;
        }

        static MauiAppBuilder SetupDefaults(this MauiAppBuilder builder)
        {
#if WINDOWS || ANDROID || IOS || MACCATALYST || TIZEN
			// initialize compatibility DependencyService
			DependencyService.SetToInitialized();
			DependencyService.Register<Xaml.ResourcesLoader>();
			DependencyService.Register<Xaml.ValueConverterProvider>();
			DependencyService.Register<PlatformSizeService>();

#pragma warning disable CS0612, CA1416 // Type or member is obsolete, 'ResourcesProvider' is unsupported on: 'iOS' 14.0 and later
			DependencyService.Register<ResourcesProvider>();
			DependencyService.Register<FontNamedSizeService>();
#pragma warning restore CS0612, CA1416 // Type or member is obsolete
#endif

            builder.ConfigureImageSourceHandlers();
            builder
                .ConfigureMauiHandlers(handlers =>
                {
                    handlers.AddSKMauiControlsHandlers();
                });

#if WINDOWS
			builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeService, MauiControlsInitializer>());
#endif
            builder.RemapForControls();

            return builder;
        }

        internal static MauiAppBuilder ConfigureImageSourceHandlers(this MauiAppBuilder builder) => (MauiAppBuilder)ConfigureImageSourceHandlersMethod.Invoke(null, new[] { builder })!;

        internal static MauiAppBuilder RemapForControls(this MauiAppBuilder builder) => (MauiAppBuilder)RemapForControlsMethod.Invoke(null, new[] { builder })!;
    }
}