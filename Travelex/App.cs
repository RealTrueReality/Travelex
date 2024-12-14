using Travelex.Services;
using Travelex.Pages;

namespace Travelex;

public class App : Application {
    private readonly SeedDataService _seedDataService;
    private readonly AppShell _shell;

    public App(SeedDataService seedDataService, AppShell shell) {
        _seedDataService = seedDataService;
        _shell = shell;

        // 检查是否是首次启动
        if (Preferences.Default.Get("FirstLaunch", true))
        {
            MainPage = new NavigationPage(new OnboardingPage(_shell));
            Preferences.Default.Set("FirstLaunch", false);
        }
        else
        {
            MainPage = _shell;
        }
    }

    protected override async void OnStart() {
        
        base.OnStart();
        await _seedDataService.SeedDataAsync();
    }
}