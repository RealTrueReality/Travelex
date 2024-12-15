using CommunityToolkit.Maui.Alerts;
using Syncfusion.Blazor.Notifications;
using Travelex.ViewModels;

namespace Travelex.Utils;

public class BlazorMauiInterop {
    private readonly ActivityIndicatorViewModel _indicatorViewModel;

    public BlazorMauiInterop(ActivityIndicatorViewModel indicatorViewModel) {
        _indicatorViewModel = indicatorViewModel;
    }
    public void ShowIndicator() {
        _indicatorViewModel.IsLoading = true;
        Console.WriteLine($"ShowIndicator called: {_indicatorViewModel.IsLoading}");
    }
    public void HideIndicator() {
        _indicatorViewModel.IsLoading = false;
        Console.WriteLine($"HideIndicator called: {_indicatorViewModel.IsLoading}");
    }

    
    
    //show alert async
    public async Task ShowAlertAsync(string? message,string title="警告") => await Shell.Current.DisplayAlert(title, message, "关闭");
    //show toast async communityTool
    public async Task ShowToastAsync(string message) => await Toast.Make(message).Show();
    
    
}