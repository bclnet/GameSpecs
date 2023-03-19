﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using StereoKit.Maui.Hosting.Internal;
using System;

namespace StereoKit.Maui.Hosting
{
    public static class HandlerMauiAppBuilderExtensions
    {
        public static MauiAppBuilder ConfigureMauiHandlers(this MauiAppBuilder builder, Action<IMauiHandlersCollection>? configureDelegate)
        {
            ConfigureMauiHandlers(builder.Services, configureDelegate);
            return builder;
        }

        public static IServiceCollection ConfigureMauiHandlers(this IServiceCollection services, Action<IMauiHandlersCollection>? configureDelegate)
        {
            services.TryAddSingleton<IMauiHandlersFactory>(sp => new MauiHandlersFactory(sp.GetServices<HandlerRegistration>()));
            if (configureDelegate != null)
                services.AddSingleton(new HandlerRegistration(configureDelegate));
            return services;
        }

        internal class HandlerRegistration
        {
            private readonly Action<IMauiHandlersCollection> _registerAction;
            public HandlerRegistration(Action<IMauiHandlersCollection> registerAction) => _registerAction = registerAction;
            internal void AddRegistration(IMauiHandlersCollection builder) => _registerAction(builder);
        }
    }
}
