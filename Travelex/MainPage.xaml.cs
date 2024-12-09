using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;

namespace Travelex;

public partial class MainPage : ContentPage {
    public MainPage() {
        InitializeComponent();
#pragma warning disable CA1416
        this.Behaviors.Add(new StatusBarBehavior() {
            StatusBarColor = Colors.White,
            StatusBarStyle = StatusBarStyle.DarkContent
        });
#pragma warning restore CA1416
    }
}