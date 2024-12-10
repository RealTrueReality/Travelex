using Travelex.ViewModels;

namespace Travelex.Pages;

public class BaseContentPage<TViewModel> : ContentPage where TViewModel : BaseViewModel {
    protected BaseContentPage(TViewModel viewModel) {
        BindingContext = viewModel;
    }

    
}