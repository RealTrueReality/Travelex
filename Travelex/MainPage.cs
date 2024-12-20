﻿using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Markup;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Travelex.Pages;
using Travelex.ViewModels;

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
    }
}