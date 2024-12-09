using Travelex.Services;
using Travelex.Pages;

namespace Travelex;

public class App : Application {
    private readonly SeedDataService _seedDataService;

    public App(SeedDataService seedDataService) {

        // 检查是否是首次启动
        if (Preferences.Default.Get("FirstLaunch", true))
        {
            MainPage = new NavigationPage(new OnboardingPage());
            Preferences.Default.Set("FirstLaunch", false);
        }
        else
        {
            //为了测试暂时注释
            MainPage = new AppShell();
        }

        _seedDataService = seedDataService;
    }

    protected override async void OnStart() {
        
        base.OnStart();
        await _seedDataService.SeedDataAsync();
    }
}