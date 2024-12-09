using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor;
using Travelex.Data;
using Travelex.Services;

namespace Travelex;

public static class MauiProgram {
    public static MauiApp CreateMauiApp() {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMarkup()
            .ConfigureFonts(fonts =>
            {
                // HarmonyOS Sans
                fonts.AddFont("HarmonyOS_Sans_Bold.ttf", "HarmonyBold");
                fonts.AddFont("HarmonyOS_Sans_Medium.ttf", "HarmonyMedium");
                fonts.AddFont("HarmonyOS_Sans_Regular.ttf", "HarmonyRegular");

                // Plus Jakarta Sans
                fonts.AddFont("PlusJakartaSans-Bold.ttf", "PJBold");
                fonts.AddFont("PlusJakartaSans-Medium.ttf", "PJMedium");
                fonts.AddFont("PlusJakartaSans-Regular.ttf", "PJRegular");
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSyncfusionBlazor();
        builder.Services.AddSingleton<DatabaseContext>();
        builder.Services.AddTransient<SeedDataService>();
        builder.Services.AddTransient<AuthService>();


#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}