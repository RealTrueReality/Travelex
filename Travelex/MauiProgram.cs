using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;

using Microsoft.Extensions.Logging;
using Syncfusion.Blazor;
using Travelex.Data;
using Travelex.Pages;
using Travelex.Services;
using Travelex.Utils;
using Travelex.ViewModels;

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
        builder.Services.AddTransient<TravelService>();
        builder.Services.AddTransient<ExpenseService>();
        builder.Services.AddTransient<ActivityIndicatorViewModel>();
        
        // 注册应用程序核心服务
        builder.Services.AddTransient<AppShell>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<OnboardingPage>();
        
        // 注册 Blazor MAUI 互操作服务
        builder.Services.AddSingleton<ActivityIndicatorViewModel>();
        builder.Services.AddSingleton<BlazorMauiInterop>();   
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NMaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWH1fc3RVRWFZVUJ0VkE=");

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}