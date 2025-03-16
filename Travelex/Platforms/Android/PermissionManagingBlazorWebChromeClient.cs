using Android;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Webkit;
using AndroidX.Activity;
using AndroidX.Activity.Result;
using AndroidX.Core.Content;
using Java.Interop;
using System;
using System.Collections.Generic;
using View = Android.Views.View;
using WebView = Android.Webkit.WebView;

namespace Travelex;

internal class PermissionManagingBlazorWebChromeClient : WebChromeClient, IActivityResultCallback
{
    private const string LocationAccessRationale = "This app requires access to your location. Please grant access to your precise location when requested.";

    private static readonly Dictionary<string, string> s_rationalesByPermission = new()
    {
        [Manifest.Permission.AccessFineLocation] = LocationAccessRationale,
    };

    private readonly WebChromeClient _blazorWebChromeClient;
    private readonly ComponentActivity _activity;
    private Action<bool>? _pendingPermissionRequestCallback;

    public PermissionManagingBlazorWebChromeClient(WebChromeClient blazorWebChromeClient, ComponentActivity activity)
    {
        _blazorWebChromeClient = blazorWebChromeClient;
        _activity = activity;
    }

    public override void OnCloseWindow(Android.Webkit.WebView? window)
    {
        _blazorWebChromeClient.OnCloseWindow(window);
        _pendingPermissionRequestCallback = null;
    }

    public override void OnGeolocationPermissionsShowPrompt(string? origin, GeolocationPermissions.ICallback? callback)
    {
        ArgumentNullException.ThrowIfNull(callback, nameof(callback));
        RequestPermission(Manifest.Permission.AccessFineLocation, isGranted => callback.Invoke(origin, isGranted, false));
    }

    private void RequestPermission(string permission, Action<bool> callback)
    {
        if (ContextCompat.CheckSelfPermission(_activity, permission) == Permission.Granted)
        {
            callback.Invoke(true);
        }
        else if (_activity.ShouldShowRequestPermissionRationale(permission) && s_rationalesByPermission.TryGetValue(permission, out var rationale))
        {
            new AlertDialog.Builder(_activity)
                .SetTitle("Enable app permissions")!
                .SetMessage(rationale)!
                .SetNegativeButton("No thanks", (_, _) => callback(false))!
                .SetPositiveButton("Continue", (_, _) => LaunchPermissionRequestActivity(permission, callback))!
                .Show();
        }
        else
        {
            LaunchPermissionRequestActivity(permission, callback);
        }
    }

    private void LaunchPermissionRequestActivity(string permission, Action<bool> callback)
    {
        if (_pendingPermissionRequestCallback is not null)
        {
            throw new InvalidOperationException("Cannot perform multiple permission requests simultaneously.");
        }

        if (MainActivity.PermissionLauncher == null)
        {
            callback(false);
            return;
        }

        _pendingPermissionRequestCallback = callback;
        MainActivity.CurrentCallback = this;
        MainActivity.PermissionLauncher.Launch(permission);
    }

    void IActivityResultCallback.OnActivityResult(Java.Lang.Object? isGranted)
    {
        var callback = _pendingPermissionRequestCallback;
        _pendingPermissionRequestCallback = null;
        MainActivity.CurrentCallback = null;
        callback?.Invoke(isGranted != null && (bool)isGranted);
    }

    #region Forward other calls to base WebChromeClient
    public override JniPeerMembers JniPeerMembers => _blazorWebChromeClient.JniPeerMembers;
    public override Bitmap? DefaultVideoPoster => _blazorWebChromeClient.DefaultVideoPoster;
    public override Android.Views.View? VideoLoadingProgressView => _blazorWebChromeClient.VideoLoadingProgressView;
    public override void GetVisitedHistory(IValueCallback? callback) => _blazorWebChromeClient.GetVisitedHistory(callback);
    public override bool OnConsoleMessage(ConsoleMessage? consoleMessage) => _blazorWebChromeClient.OnConsoleMessage(consoleMessage);
    public override bool OnCreateWindow(WebView? view, bool isDialog, bool isUserGesture, Message? resultMsg) => _blazorWebChromeClient.OnCreateWindow(view, isDialog, isUserGesture, resultMsg);
    public override void OnGeolocationPermissionsHidePrompt() => _blazorWebChromeClient.OnGeolocationPermissionsHidePrompt();
    public override void OnHideCustomView() => _blazorWebChromeClient.OnHideCustomView();
    public override bool OnJsAlert(WebView? view, string? url, string? message, JsResult? result) => _blazorWebChromeClient.OnJsAlert(view, url, message, result);
    public override bool OnJsBeforeUnload(WebView? view, string? url, string? message, JsResult? result) => _blazorWebChromeClient.OnJsBeforeUnload(view, url, message, result);
    public override bool OnJsConfirm(WebView? view, string? url, string? message, JsResult? result) => _blazorWebChromeClient.OnJsConfirm(view, url, message, result);
    public override bool OnJsPrompt(WebView? view, string? url, string? message, string? defaultValue, JsPromptResult? result) => _blazorWebChromeClient.OnJsPrompt(view, url, message, defaultValue, result);
    public override void OnPermissionRequestCanceled(PermissionRequest? request) => _blazorWebChromeClient.OnPermissionRequestCanceled(request);
    public override void OnProgressChanged(WebView? view, int newProgress) => _blazorWebChromeClient.OnProgressChanged(view, newProgress);
    public override void OnReceivedIcon(WebView? view, Bitmap? icon) => _blazorWebChromeClient.OnReceivedIcon(view, icon);
    public override void OnReceivedTitle(WebView? view, string? title) => _blazorWebChromeClient.OnReceivedTitle(view, title);
    public override void OnReceivedTouchIconUrl(WebView? view, string? url, bool precomposed) => _blazorWebChromeClient.OnReceivedTouchIconUrl(view, url, precomposed);
    public override void OnRequestFocus(WebView? view) => _blazorWebChromeClient.OnRequestFocus(view);
    public override void OnShowCustomView(View? view, ICustomViewCallback? callback) => _blazorWebChromeClient.OnShowCustomView(view, callback);
    #endregion
}
