using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using System;

namespace StereoKit.Maui
{
    static class ServiceProviderExtensions
    {
        internal static ILogger<T>? CreateLogger<T>(this IMauiContext context) =>
            context.Services.CreateLogger<T>();

        internal static ILogger<T>? CreateLogger<T>(this IServiceProvider services) =>
            services.GetService<ILogger<T>>();

        internal static ILogger? CreateLogger(this IMauiContext context, string loggerName) =>
            context.Services.CreateLogger(loggerName);

        internal static ILogger? CreateLogger(this IServiceProvider services, string loggerName) =>
            services.GetService<ILoggerFactory>()?.CreateLogger(loggerName);
    }
}