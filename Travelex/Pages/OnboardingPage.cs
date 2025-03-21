using CommunityToolkit.Maui.Markup;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;
using Travelex.Models;
using Travelex.Services;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace Travelex.Pages;

public class OnboardingPage : ContentPage
{
    private CarouselView OnboardingCarousel;
    private IndicatorView IndicatorView;
    private Button NextButton;
    
    private ObservableCollection<OnboardingModel> _onboardingItems;
    private readonly AppShell _shell;
    
    public ObservableCollection<OnboardingModel> OnboardingItems
    {
        get => _onboardingItems;
        set
        {
            _onboardingItems = value;
            OnPropertyChanged();
        }
    }

    public OnboardingPage(AppShell shell)
    {
        _shell = shell;
        NavigationPage.SetHasNavigationBar(this, false);
        Shell.SetNavBarIsVisible(this, false);
        
        bool isDarkMode = Application.Current.RequestedTheme == AppTheme.Dark;
        
        BackgroundColor = isDarkMode ? Colors.Black : Colors.White;
        
        SetupOnboardingItems();
        Build(isDarkMode);
    }

    private void Build(bool isDarkMode = false) {
        IndicatorView = new IndicatorView {
            IndicatorColor = isDarkMode ? Color.Parse("#555555") : Color.Parse("#E0E0E0"),
            SelectedIndicatorColor = isDarkMode ? Color.Parse("#1E90FF") : Color.Parse("#0085FF"),
            HorizontalOptions = LayoutOptions.Center,
        };

        OnboardingCarousel = new CarouselView {
            ItemsSource = OnboardingItems,
            IndicatorView = IndicatorView,
            Loop = false,  
            IsSwipeEnabled = true,  
        };
        
        OnboardingCarousel.PropertyChanged += (sender, args) => {
            if (args.PropertyName == nameof(CarouselView.Position))
            {
                UpdateNextButton();
            }
        };
        
        OnboardingCarousel.ItemTemplate = new DataTemplate(() =>
            new VerticalStackLayout {
                Spacing = 20,
                Padding = new Thickness(20),
                BackgroundColor = isDarkMode ? Colors.Black : Colors.White,
                Children = {
                    new Image()
                        .Height(300)
                        .Center()
                        .Bind(Image.SourceProperty, nameof(OnboardingModel.ImageSource)),
                    new Label()
                        .Font(family: "HarmonyBold", size: 32)
                        .TextColor(isDarkMode ? Color.Parse("#E0E0E0") : Color.Parse("#1A1A1A"))
                        .Start()
                        .Bind(Label.TextProperty, nameof(OnboardingModel.Title)),
                    new Label()
                        .Font(family: "HarmonyRegular", size: 16)
                        .TextColor(isDarkMode ? Color.Parse("#A0A0A0") : Color.Parse("#666666"))
                        .Start()
                        .Bind(Label.TextProperty, nameof(OnboardingModel.Description))
                }
            }
        );

        NextButton = new Button {
            Text = "下一步",
            TextColor = Colors.White,
            BackgroundColor = isDarkMode ? Color.Parse("#1E90FF") : Color.Parse("#0085FF"),
            FontFamily = "HarmonyMedium",
            FontSize = 16,
            HeightRequest = 50,
            CornerRadius = 25,
            Margin = new Thickness(20, 0, 20, 40)
        };
        NextButton.Clicked += OnNextClicked;

        Content = new Grid {
            BackgroundColor = isDarkMode ? Colors.Black : Colors.White,
            RowDefinitions = Rows.Define(
                (Row.Carousel, Star),
                (Row.Indicator, Auto),
                (Row.Button, Auto)
            ),
            Children = {
                OnboardingCarousel.Row(Row.Carousel),
                IndicatorView.Row(Row.Indicator).Margins(0, 0, 0, 20),
                NextButton.Row(Row.Button)
            }
        };
        
        UpdateNextButton();
    }

    private void UpdateNextButton()
    {
        NextButton.Text = OnboardingCarousel.Position == OnboardingItems.Count - 1 ? "开始使用" : "下一步";
    }

    private void SetupOnboardingItems()
    {
        OnboardingItems = new ObservableCollection<OnboardingModel>
        {
            new OnboardingModel
            {
                Title = "轻松追踪您的旅程",
                Description = "轻松管理您的所有旅程，从南极到北极，一站式添加所有行程信息。",
                ImageSource = "onboarding_track.svg"
            },
            new OnboardingModel
            {
                Title = "管理您的预算",
                Description = "轻松跟踪旅行支出，智能分析图表展示，让您的旅行预算一目了然，追踪您的支出并控制旅行预算。",
                ImageSource = "onboarding_budget.svg"
            },
            new OnboardingModel
            {
                Title = "AI旅行助手",
                Description = "通义千问AI为您提供个性化旅行建议，轻松获得旅游信息并获取智能推荐",
                ImageSource = "onboarding_ai.svg"
            }
        };
    }

    private void OnNextClicked(object sender, EventArgs e)
    {
        if (OnboardingCarousel.Position < OnboardingItems.Count - 1)
        {
            OnboardingCarousel.Position += 1;
        }
        else
        {
            Preferences.Default.Set("FirstLaunch", false);
            
            Application.Current.MainPage = _shell;
        }
    }
    
    protected override bool OnBackButtonPressed()
    {
        return true;
    }

    private enum Row { Carousel, Indicator, Button }
}
