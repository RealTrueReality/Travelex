using CommunityToolkit.Maui.Markup;
using Travelex.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace Travelex;

public class AppShell : Shell
{
    private readonly IServiceProvider _serviceProvider;
    
    public AppShell(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        RegisterRoutes();
        Build();
        
        // 全局禁用导航栏
        Shell.SetNavBarIsVisible(this, false);
        // 禁用返回按钮
        // Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = false });
    }

    private void Build()
    {
        Items.Add(new ShellContent
        {
            Title = "Home",
            ContentTemplate = new DataTemplate(() => _serviceProvider.GetService<MainPage>()),
            Route = GetRoute<MainPage>()
        });
    }
    public static string GetRoute<T>() where T : ContentPage {
        
        if (typeof(T) == typeof(MainPage)) {
            return $"//{nameof(MainPage)}";
        }

        throw new NotImplementedException($"Route for {typeof(T).Name} not implemented");
    }
    private void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(OnboardingPage), typeof(OnboardingPage));
    }
}
