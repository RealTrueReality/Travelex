using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.Maui.Behaviors;
using Microsoft.JSInterop;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Platform;
using Microsoft.Maui.ApplicationModel;

namespace Travelex.Services;

public enum ThemePreference {
    System,
    Light,
    Dark
}

public class ThemeService : INotifyPropertyChanged {
    private readonly IJSRuntime _jsRuntime;
    private bool _isDarkMode;
    private ThemePreference _preference = ThemePreference.System;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<bool> ThemeChanged;

    // 静态实例，用于从JS回调
    private static ThemeService? _instance;

    public ThemeService(IJSRuntime jsRuntime) {
        _jsRuntime = jsRuntime;
        _instance = this;
    }

    public bool IsDarkMode {
        get => _isDarkMode;
        private set {
            if (_isDarkMode != value) {
                _isDarkMode = value;
                OnPropertyChanged();
                ThemeChanged?.Invoke(value);
                UpdateStatusBarColor(value);
            }
        }
    }

    public ThemePreference Preference {
        get => _preference;
        private set {
            if (_preference != value) {
                _preference = value;
                OnPropertyChanged();
            }
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task InitializeAsync() {
        // 从本地存储加载主题偏好
        var storedPreference = await GetStoredPreferenceAsync();
        Preference = storedPreference;

        if (Preference == ThemePreference.System) {
            // 获取系统主题
            IsDarkMode = await IsSystemInDarkModeAsync();
        }
        else {
            IsDarkMode = Preference == ThemePreference.Dark;
        }

        // 应用当前主题
        await ApplyThemeAsync();
    }

    private async Task<ThemePreference> GetStoredPreferenceAsync() {
        var preference = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "themePreference");
        if (string.IsNullOrEmpty(preference))
            return ThemePreference.System;

        return Enum.TryParse<ThemePreference>(preference, out var result)
            ? result
            : ThemePreference.System;
    }

    public async Task SetPreferenceAsync(ThemePreference preference) {
        Preference = preference;
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "themePreference", preference.ToString());

        if (preference == ThemePreference.System) {
            IsDarkMode = await IsSystemInDarkModeAsync();
        }
        else {
            IsDarkMode = preference == ThemePreference.Dark;
        }

        await ApplyThemeAsync();
    }

    public async Task ApplyThemeAsync() {
        await _jsRuntime.InvokeVoidAsync("setDarkMode", IsDarkMode);
    }

    private async Task<bool> IsSystemInDarkModeAsync() {
        return await _jsRuntime.InvokeAsync<bool>("isSystemInDarkMode");
    }

    // 监听系统主题变化
    public async Task SetupSystemThemeListenerAsync() {
        await _jsRuntime.InvokeVoidAsync("setupSystemThemeListener");
    }

    // 用于从JS回调的静态方法
    [JSInvokable]
    public static async Task OnSystemThemeChanged(bool isDark) {
        if (_instance != null && _instance.Preference == ThemePreference.System) {
            _instance.IsDarkMode = isDark;
            await _instance.ApplyThemeAsync();
        }
    }

    // 更新状态栏颜色
    private void UpdateStatusBarColor(bool isDarkMode) {
        // 确保在主线程上运行UI操作
        MainThread.BeginInvokeOnMainThread(() => {
            var statusBarColor = isDarkMode ? Colors.Black : Colors.White;


            Application.Current.MainPage.BackgroundColor = statusBarColor;
#pragma warning disable CA1416
            Application.Current.MainPage.Behaviors.Add(new StatusBarBehavior() {
                StatusBarColor = statusBarColor,
                StatusBarStyle = isDarkMode ? StatusBarStyle.LightContent : StatusBarStyle.DarkContent
            });
#pragma warning restore CA1416
        });
    }
}