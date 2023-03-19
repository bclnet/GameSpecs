﻿using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace GameSpec.App.Explorer
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseSKMauiApp<App>();
                //.ConfigureFonts(fonts =>
                //{
                //    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                //    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                //});

            return builder.Build();
        }
    }
}