using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Travelex.ViewModels;

public partial class ActivityIndicatorViewModel : BaseViewModel{
    [ObservableProperty]
    private bool _isLoading; // 私有字段
    
}