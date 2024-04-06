#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.LifecycleEvents;

namespace StereoKit.Maui.LifecycleEvents
{
	internal static class LifecycleEventServiceExtensions
	{
		internal static void InvokeLifecycleEvents<TDelegate>(this IServiceProvider services, Action<TDelegate> action)
			where TDelegate : Delegate
		{
			if (services == null)
				return;

			var delegates = services.GetLifecycleEventDelegates<TDelegate>();

			foreach (var del in delegates)
				action?.Invoke(del);
		}

		internal static IEnumerable<TDelegate> GetLifecycleEventDelegates<TDelegate>(this IServiceProvider services, string? eventName = null)
			where TDelegate : Delegate
		{
			var lifecycleService = services?.GetService<ILifecycleEventService>();
			if (lifecycleService == null)
				yield break;

			if (eventName == null)
				eventName = typeof(TDelegate).Name;

			foreach (var del in lifecycleService.GetEventDelegates<TDelegate>(eventName))
				yield return del;
		}

		internal static bool ContainsLifecycleEvent<TDelegate>(this IServiceProvider services, string? eventName = null)
			where TDelegate : Delegate
		{
			var lifecycleService = services?.GetService<ILifecycleEventService>();
			if (lifecycleService == null)
				return false;

			if (eventName == null)
				eventName = typeof(TDelegate).Name;

			if (lifecycleService.ContainsEvent(eventName))
				return true;

			return false;
		}
	}
}