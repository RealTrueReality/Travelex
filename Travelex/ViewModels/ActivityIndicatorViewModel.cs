using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Travelex.ViewModels;

public partial class ActivityIndicatorViewModel : BaseViewModel{
    [ObservableProperty]
    public partial bool IsLoading { get; set; }
    
}
