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
    public async Task ShowAlertAsync(string? message, string title = "警告") =>
        await Shell.Current.DisplayAlert(title, message, "关闭");

    //show toast async communityTool
    public async Task ShowToastAsync(string message) => await Toast.Make(message).Show();

    // OpenInLauncher
    public async Task OpenInLauncher(string url) {
        try {
            if (string.IsNullOrEmpty(url)) {
                await ShowAlertAsync("无效的URL");
                return;
            }

            Uri uri = new Uri(url);

            // Try to use Browser.OpenAsync first for web URLs
            if (uri.Scheme == "http" || uri.Scheme == "https") {
                try {
                    await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
                    return;
                }
                catch (Exception ex) {
                    await ShowAlertAsync($"无法使用浏览器打开链接: {ex.Message}");
                }
            }

            // Fall back to Launcher for non-web URLs or if Browser failed
            bool success = await Launcher.Default.TryOpenAsync(uri);

            if (!success) {
                await ShowAlertAsync($"无法打开链接: {url}");
            }
            else {
                await ShowAlertAsync($"已打开链接: {url}");
            }
        }
        catch (UriFormatException ex) {
            Console.WriteLine($"Invalid URI format: {ex.Message}");
            await ShowAlertAsync($"无效的URL格式: {ex.Message}");
        }
        catch (Exception ex) {
            Console.WriteLine($"Error opening URL: {ex.Message}");
            await ShowAlertAsync($"打开链接时出错: {ex.Message}");
        }
    }
}