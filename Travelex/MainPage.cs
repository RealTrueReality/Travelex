using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Markup;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using Travelex.Pages;
using Travelex.ViewModels;
#if ANDROID
using AndroidX.Activity;
#endif
namespace Travelex;

public class MainPage : BaseContentPage<ActivityIndicatorViewModel> {
    private BlazorWebView blazorWebView;

    public MainPage(ActivityIndicatorViewModel indicatorViewModel) : base(indicatorViewModel) {
        
        
        BackgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.Black : Colors.White;
#pragma warning disable CA1416
        this.Behaviors.Add(new StatusBarBehavior() {
            StatusBarColor = Colors.White,
            StatusBarStyle = StatusBarStyle.DarkContent
        });
#pragma warning restore CA1416

#if ANDROID
        Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.Window.SetNavigationBarColor(Android.Graphics.Color.White);
        Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.Window.NavigationBarContrastEnforced = false;
#endif
        Content = new Grid {
            Children = {
                new BlazorWebView {
                    HostPage = "wwwroot/index.html",
                    RootComponents = {
                        new RootComponent {
                            Selector = "#app",
                            ComponentType = typeof(Components.Routes)
                        }
                    }
                }.Assign(out blazorWebView),

                new ActivityIndicator(){Color = Color.FromArgb("#3b82f6")}
                    .Center() // This applies both horizontal and vertical center
                   
                    .Bind(ActivityIndicator.IsRunningProperty, getter: (ActivityIndicatorViewModel vm) => vm.IsLoading,
                        setter: (ActivityIndicatorViewModel vm, bool value) => vm.IsLoading = !value)
            }
        };

        blazorWebView.BlazorWebViewInitialized += BlazorWebViewInitialized;
        blazorWebView.BlazorWebViewInitializing += BlazorWebViewInitializing;
    }

    private void BlazorWebViewInitialized(object? sender, BlazorWebViewInitializedEventArgs e)
    {
#if ANDROID
        if (e.WebView.Context?.GetActivity() is not ComponentActivity activity)
        {
            throw new InvalidOperationException($"The permission-managing WebChromeClient requires that the current activity be a '{nameof(ComponentActivity)}'.");
        }

        e.WebView.Settings.JavaScriptEnabled = true;
        e.WebView.Settings.AllowFileAccess = true;
        e.WebView.Settings.SetGeolocationEnabled(true);
        e.WebView.Settings.SetGeolocationDatabasePath(e.WebView.Context?.FilesDir?.Path);
        e.WebView.SetWebChromeClient(new PermissionManagingBlazorWebChromeClient(e.WebView.WebChromeClient!, activity));
#endif
    }

    private void BlazorWebViewInitializing(object? sender, BlazorWebViewInitializingEventArgs e)
    {
#if IOS || MACCATALYST                   
        e.Configuration.AllowsInlineMediaPlayback = true;
        e.Configuration.MediaTypesRequiringUserActionForPlayback = WebKit.WKAudiovisualMediaTypes.None;
#endif
    }
}