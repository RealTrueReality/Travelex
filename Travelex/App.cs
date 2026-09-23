using Travelex.Services;
using Travelex.Pages;

namespace Travelex;

public class App : Application {
    private readonly SeedDataService _seedDataService;
    private readonly Page _initialPage;

    public App(SeedDataService seedDataService, AppShell shell) {
        _seedDataService = seedDataService;

        // 检查是否是首次启动
        if (Preferences.Default.Get("FirstLaunch", true))
        {
            _initialPage = new NavigationPage(new OnboardingPage(shell));
        }
        else
        {
            _initialPage = shell;
        }
    }

    protected override Window CreateWindow(IActivationState? activationState) => new(_initialPage);

    protected override async void OnStart() {
        base.OnStart();
        try {
            await _seedDataService.SeedDataAsync();
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex}");
        }
    }
}
