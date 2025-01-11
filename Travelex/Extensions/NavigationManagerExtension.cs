using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace Travelex.Extensions;

public static class NavigationManagerExtension {
    private const string NAVIGATION_CHAIN_KEY = "navigationChain";
    
    public static string GetCurrentRoute(this NavigationManager navigationManager) {
        return navigationManager.Uri[navigationManager.BaseUri.Length..].Insert(0, "/");
    }

    public static void NavigateToSubPage(this NavigationManager navigationManager, string subPageUrl) {
        var currentRoute = navigationManager.GetCurrentRoute();
        var navigationChain = GetNavigationChain(navigationManager);
        navigationChain.Add(currentRoute);
        
        navigationManager.NavigateTo(subPageUrl, new NavigationOptions() {
            HistoryEntryState = JsonSerializer.Serialize(navigationChain)
        });
    }

    public static void NavigateBack(this NavigationManager navigationManager) {
        var navigationChainJson = navigationManager.HistoryEntryState?.ToString();
        if (string.IsNullOrEmpty(navigationChainJson))
        {
            navigationManager.NavigateTo("/home", new NavigationOptions {
                HistoryEntryState = null,
                ReplaceHistoryEntry = true
            });
            return;
        }

        var navigationChain = JsonSerializer.Deserialize<List<string>>(navigationChainJson);
        if (navigationChain == null || !navigationChain.Any())
        {
            navigationManager.NavigateTo("/home", new NavigationOptions {
                HistoryEntryState = null,
                ReplaceHistoryEntry = true
            });
            return;
        }

        var previousRoute = navigationChain.Last();
        navigationChain.RemoveAt(navigationChain.Count - 1);
        
        navigationManager.NavigateTo(previousRoute, new NavigationOptions {
            HistoryEntryState = navigationChain.Any() ? JsonSerializer.Serialize(navigationChain) : null,
            ReplaceHistoryEntry = true
        });
    }

    private static List<string> GetNavigationChain(NavigationManager navigationManager)
    {
        var navigationChainJson = navigationManager.HistoryEntryState?.ToString();
        return string.IsNullOrEmpty(navigationChainJson) 
            ? new List<string>() 
            : JsonSerializer.Deserialize<List<string>>(navigationChainJson) ?? new List<string>();
    }
}