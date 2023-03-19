using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Handlers;
using StereoKit.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Hosting
{
    public static class SKAppHostBuilderExtensions
    {
        static readonly MethodInfo? RemapForControlsMethod = typeof(AppHostBuilderExtensions).GetMethod("RemapForControls", BindingFlags.Static | BindingFlags.NonPublic, new[] { typeof(MauiAppBuilder) });

        public static MauiAppBuilder UseSKMauiApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder) where TApp : class, IApplication
        {
#pragma warning disable RS0030 // Do not used banned APIs - don't want to use a factory method here
            builder.Services.TryAddSingleton<IApplication, TApp>();
#pragma warning restore RS0030
            builder.SetupDefaults();
            builder.UseMauiApp<TApp>();
            return builder;
        }

        public static MauiAppBuilder UseSKMauiApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder, Func<IServiceProvider, TApp> implementationFactory) where TApp : class, IApplication
        {
            builder.Services.TryAddSingleton<IApplication>(implementationFactory);
            builder.SetupDefaults();
            //builder.UseMauiApp<TApp>();
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
            handlersCollection.AddHandler<Ellipse, SKShapeViewHandler>();
            handlersCollection.AddHandler<Line, LineHandler>();
            handlersCollection.AddHandler<Path, PathHandler>();
            handlersCollection.AddHandler<Polygon, PolygonHandler>();
            handlersCollection.AddHandler<Polyline, PolylineHandler>();
            handlersCollection.AddHandler<Rectangle, RectangleHandler>();
            handlersCollection.AddHandler<RoundRectangle, RoundRectangleHandler>();
            handlersCollection.AddHandler<Window, SKWindowHandler>();
            handlersCollection.AddHandler<ImageButton, SKImageButtonHandler>();
            handlersCollection.AddHandler<IndicatorView, SKIndicatorViewHandler>();
            handlersCollection.AddHandler<RadioButton, SKRadioButtonHandler>();
            handlersCollection.AddHandler<RefreshView, SKRefreshViewHandler>();
            handlersCollection.AddHandler<SwipeItem, SKSwipeItemMenuItemHandler>();
            handlersCollection.AddHandler<SwipeView, SKSwipeViewHandler>();

#pragma warning disable CA1416 //  'MenuBarHandler', MenuFlyoutSubItemHandler, MenuFlyoutSubItemHandler, MenuBarItemHandler is only supported on: 'ios' 13.0 and later
            handlersCollection.AddHandler<MenuBar, SKMenuBarHandler>();
#if MENU2
            handlersCollection.AddHandler<MenuFlyoutSubItem, SKMenuFlyoutSubItemHandler>();
            handlersCollection.AddHandler<MenuFlyoutSeparator, SKMenuFlyoutSeparatorHandler>();
            handlersCollection.AddHandler<MenuFlyoutItem, SKMenuFlyoutItemHandler>();
#endif
            handlersCollection.AddHandler<MenuBarItem, SKMenuBarItemHandler>();
#pragma warning restore CA1416

            //#if WINDOWS || ANDROID || IOS || MACCATALYST || TIZEN
            //			handlersCollection.AddHandler(typeof(ListView), typeof(Handlers.Compatibility.ListViewRenderer));
            //#if !TIZEN
            //			handlersCollection.AddHandler(typeof(Cell), typeof(Handlers.Compatibility.CellRenderer));
            //			handlersCollection.AddHandler(typeof(ImageCell), typeof(Handlers.Compatibility.ImageCellRenderer));
            //			handlersCollection.AddHandler(typeof(EntryCell), typeof(Handlers.Compatibility.EntryCellRenderer));
            //			handlersCollection.AddHandler(typeof(TextCell), typeof(Handlers.Compatibility.TextCellRenderer));
            //			handlersCollection.AddHandler(typeof(ViewCell), typeof(Handlers.Compatibility.ViewCellRenderer));
            //			handlersCollection.AddHandler(typeof(SwitchCell), typeof(Handlers.Compatibility.SwitchCellRenderer));
            //#endif
            //			handlersCollection.AddHandler(typeof(TableView), typeof(Handlers.Compatibility.TableViewRenderer));
            //			handlersCollection.AddHandler(typeof(Frame), typeof(Handlers.Compatibility.FrameRenderer));
            //#endif

            //#if WINDOWS || MACCATALYST
            //			handlersCollection.AddHandler(typeof(MenuFlyout), typeof(MenuFlyoutHandler));
            //#endif

            //#if IOS || MACCATALYST
            //			handlersCollection.AddHandler(typeof(NavigationPage), typeof(Handlers.Compatibility.NavigationRenderer));
            //			handlersCollection.AddHandler(typeof(TabbedPage), typeof(Handlers.Compatibility.TabbedRenderer));
            //			handlersCollection.AddHandler(typeof(FlyoutPage), typeof(Handlers.Compatibility.PhoneFlyoutPageRenderer));
            //#endif

            //#if ANDROID || IOS || MACCATALYST || TIZEN
            //			handlersCollection.AddHandler<SwipeItemView, SwipeItemViewHandler>();
            //#if ANDROID || IOS || MACCATALYST
            //			handlersCollection.AddHandler<Shell, ShellRenderer>();
            //#else
            //			handlersCollection.AddHandler<Shell, ShellHandler>();
            //			handlersCollection.AddHandler<ShellItem, ShellItemHandler>();
            //			handlersCollection.AddHandler<ShellSection, ShellSectionHandler>();
            //#endif
            //#endif
            //#if WINDOWS || ANDROID || TIZEN
            //			handlersCollection.AddHandler<NavigationPage, NavigationViewHandler>();
            //			handlersCollection.AddHandler<Toolbar, ToolbarHandler>();
            //			handlersCollection.AddHandler<FlyoutPage, FlyoutViewHandler>();
            //			handlersCollection.AddHandler<TabbedPage, TabbedViewHandler>();
            //#endif

            //#if WINDOWS
            //			handlersCollection.AddHandler<ShellItem, ShellItemHandler>();
            //			handlersCollection.AddHandler<ShellSection, ShellSectionHandler>();
            //			handlersCollection.AddHandler<ShellContent, ShellContentHandler>();
            //			handlersCollection.AddHandler<Shell, ShellHandler>();
            //#endif
            return handlersCollection;
        }

        public static IMauiHandlersCollection AddMauiControlsHandlers(this IMauiHandlersCollection handlersCollection)
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
            handlersCollection.AddHandler<Ellipse, ShapeViewHandler>();
            handlersCollection.AddHandler<Line, LineHandler>();
            handlersCollection.AddHandler<Path, PathHandler>();
            handlersCollection.AddHandler<Polygon, PolygonHandler>();
            handlersCollection.AddHandler<Polyline, PolylineHandler>();
            handlersCollection.AddHandler<Rectangle, RectangleHandler>();
            handlersCollection.AddHandler<RoundRectangle, RoundRectangleHandler>();
            handlersCollection.AddHandler<Window, WindowHandler>();
            handlersCollection.AddHandler<ImageButton, ImageButtonHandler>();
            handlersCollection.AddHandler<IndicatorView, IndicatorViewHandler>();
            handlersCollection.AddHandler<RadioButton, RadioButtonHandler>();
            handlersCollection.AddHandler<RefreshView, RefreshViewHandler>();
            handlersCollection.AddHandler<SwipeItem, SwipeItemMenuItemHandler>();
            handlersCollection.AddHandler<SwipeView, SwipeViewHandler>();

#pragma warning disable CA1416 //  'MenuBarHandler', MenuFlyoutSubItemHandler, MenuFlyoutSubItemHandler, MenuBarItemHandler is only supported on: 'ios' 13.0 and later
            handlersCollection.AddHandler<MenuBar, MenuBarHandler>();
#if MENU2
            handlersCollection.AddHandler<MenuFlyoutSubItem, MenuFlyoutSubItemHandler>();
            handlersCollection.AddHandler<MenuFlyoutSeparator, MenuFlyoutSeparatorHandler>();
            handlersCollection.AddHandler<MenuFlyoutItem, MenuFlyoutItemHandler>();
#endif
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
                    handlers.AddMauiControlsHandlers();
                });

#if WINDOWS
			builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeService, MauiControlsInitializer>());
#endif
            //builder.RemapForControls();

            return builder;
        }

        internal static MauiAppBuilder ConfigureImageSourceHandlers(this MauiAppBuilder builder)
        {
            builder.ConfigureImageSources(services =>
            {
                services.AddService<FileImageSource>(svcs => new FileImageSourceService(svcs.CreateLogger<FileImageSourceService>()));
                services.AddService<FontImageSource>(svcs => new FontImageSourceService(svcs.GetRequiredService<IFontManager>(), svcs.CreateLogger<FontImageSourceService>()));
                services.AddService<StreamImageSource>(svcs => new StreamImageSourceService(svcs.CreateLogger<StreamImageSourceService>()));
                services.AddService<UriImageSource>(svcs => new UriImageSourceService(svcs.CreateLogger<UriImageSourceService>()));
            });

            return builder;
        }



        internal static MauiAppBuilder RemapForControls(this MauiAppBuilder builder)
        {
#if __ANDROID__
			var remapForControlsMethod = typeof(AppHostBuilderExtensions).GetMethod("RemapForControls", BindingFlags.Static | BindingFlags.NonPublic);
            System.Console.WriteLine("HERE");
#endif

            //var abc = (MauiAppBuilder)RemapForControlsMethod.Invoke(null, new[] { builder });
            return builder;
        }
    }
}