using Microsoft.AspNetCore.Components;

namespace Travelex.Extensions;

public static class NavigationManagerExtension {
    public static string GetCurrentRoute(this NavigationManager navigationManager) {
        return navigationManager.Uri[navigationManager.BaseUri.Length..].Insert(0, "/");
    }

    public static void NavigateToSubPage(this NavigationManager navigationManager,string subPageUrl) {
        navigationManager.NavigateTo(subPageUrl,new NavigationOptions() {
            HistoryEntryState = navigationManager.GetCurrentRoute()
        });
    }
    public static void NavigateBack(this NavigationManager navigationManager) {
        navigationManager.NavigateTo(navigationManager.HistoryEntryState ?? "/home", new NavigationOptions() {
            HistoryEntryState = null,
            ReplaceHistoryEntry = true
        });
    }
}