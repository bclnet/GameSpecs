using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using StereoKit.Maui.Platform;
using System;
using System.Threading.Tasks;
using PlatformView = StereoKit.UIX.Controls.View;

namespace StereoKit.Maui
{
    static class ElementHandlerExtensions
	{
		public static PlatformView ToPlatform(this IElementHandler elementHandler) =>
			(elementHandler.VirtualView?.ToSKPlatform() as PlatformView) ??
				throw new InvalidOperationException($"Unable to convert {elementHandler} to {typeof(PlatformView)}");

		public static IServiceProvider GetServiceProvider(this IElementHandler handler)
		{
			var context = handler.MauiContext ??
				throw new InvalidOperationException($"Unable to find the context. The {nameof(ElementHandler.MauiContext)} property should have been set by the host.");

			var services = context?.Services ??
				throw new InvalidOperationException($"Unable to find the service provider. The {nameof(ElementHandler.MauiContext)} property should have been set by the host.");

			return services;
		}

		public static T? GetService<T>(this IElementHandler handler, Type type)
		{
			var services = handler.GetServiceProvider();

			var service = services.GetService(type);

			return (T?)service;
		}

		public static T? GetService<T>(this IElementHandler handler)
		{
			var services = handler.GetServiceProvider();

			var service = services.GetService<T>();

			return service;
		}

		public static T GetRequiredService<T>(this IElementHandler handler, Type type)
			where T : notnull
		{
			var services = handler.GetServiceProvider();

			var service = services.GetRequiredService(type);

			return (T)service;
		}

		public static T GetRequiredService<T>(this IElementHandler handler)
			where T : notnull
		{
			var services = handler.GetServiceProvider();

			var service = services.GetRequiredService<T>();

			return service;
		}

		public static Task<T> InvokeAsync<T>(this IElementHandler handler, string commandName,
			TaskCompletionSource<T> args)
		{
			handler?.Invoke(commandName, args);
			return args.Task;
		}

		public static T InvokeWithResult<T>(this IElementHandler handler, string commandName,
			RetrievePlatformValueRequest<T> args)
		{
			handler?.Invoke(commandName, args);
			return args.Result;
		}

		public static bool CanInvokeMappers(this IElementHandler viewHandler)
		{
//#if ANDROID
			//var platformView = viewHandler?.PlatformView;

			//if (platformView is PlatformView androidView && androidView.IsDisposed())
			//	return false;
//#endif
			return true;
		}
	}
}