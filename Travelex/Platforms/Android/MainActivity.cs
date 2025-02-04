using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;

namespace Travelex;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity {
    internal static ActivityResultLauncher? PermissionLauncher { get; private set; }
    internal static IActivityResultCallback? CurrentCallback { get; set; }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // 在 Activity 创建时注册权限处理器
        PermissionLauncher = RegisterForActivityResult(new ActivityResultContracts.RequestPermission(), 
            new ActivityResultCallback { Activity = this });
        
        WebViewSoftInputPatch.Initialize();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        PermissionLauncher?.Unregister();
        PermissionLauncher = null;
        CurrentCallback = null;
    }

    private class ActivityResultCallback : Java.Lang.Object, IActivityResultCallback
    {
        public MainActivity? Activity { get; set; }

        void IActivityResultCallback.OnActivityResult(Java.Lang.Object? result)
        {
            CurrentCallback?.OnActivityResult(result);
        }
    }
}