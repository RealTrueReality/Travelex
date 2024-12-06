using Travelex.Services;

namespace Travelex;

public partial class App : Application {
    private readonly SeedDataService _seedDataService;

    public App(SeedDataService seedDataService) {
        InitializeComponent();
        MainPage = new MainPage();
        _seedDataService = seedDataService;
    }

    protected override async void OnStart() {
        
        base.OnStart();
        await _seedDataService.SeedDataAsync();
    }
}