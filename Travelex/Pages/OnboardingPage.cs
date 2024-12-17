using CommunityToolkit.Maui.Markup;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;
using Travelex.Models;
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
        SetupOnboardingItems();
        Build();
    }

    private void Build() {
        IndicatorView = new IndicatorView {
            IndicatorColor = Color.Parse("#E0E0E0"),
            SelectedIndicatorColor = Color.Parse("#0085FF"),
            HorizontalOptions = LayoutOptions.Center,
        };

        OnboardingCarousel = new CarouselView {
            ItemsSource = OnboardingItems,
            IndicatorView = IndicatorView,
            Loop = false,  // 禁用循环滚动
            IsSwipeEnabled = true,  // 启用滑动
        };
        
        // 监听位置变化以更新按钮文本
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
                Children = {
                    new Image()
                        .Height(300)
                        .Center()
                        .Bind(Image.SourceProperty, nameof(OnboardingModel.ImageSource)),
                    new Label()
                        .Font(family: "HarmonyBold", size: 32)
                        .TextColor(Color.Parse("#1A1A1A"))
                        .Start()
                        .Bind(Label.TextProperty, nameof(OnboardingModel.Title)),
                    new Label()
                        .Font(family: "HarmonyRegular", size: 16)
                        .TextColor(Color.Parse("#666666"))
                        .Start()
                        .Bind(Label.TextProperty, nameof(OnboardingModel.Description))
                }
            }
        );

        NextButton = new Button {
            Text = "下一步",
            TextColor = Colors.White,
            BackgroundColor = Color.Parse("#0085FF"),
            FontFamily = "HarmonyMedium",
            FontSize = 16,
            HeightRequest = 50,
            CornerRadius = 25,
            Margin = new Thickness(20, 0, 20, 40)
        };
        NextButton.Clicked += OnNextClicked;

        Content = new Grid {
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
        
        // 初始化按钮文本
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
                Description = "轻松追踪您的旅行信息，包括航班、酒店和租车信息。",
                ImageSource = "onboarding_track.svg"
            },
            new OnboardingModel
            {
                Title = "管理您的预算",
                Description = "使用我们简单易用的工具，追踪您的支出并控制旅行预算。",
                ImageSource = "onboarding_budget.svg"
            },
            new OnboardingModel
            {
                Title = "分享您的体验",
                Description = "与亲朋好友分享您的旅行体验，一起发现新的目的地。",
                ImageSource = "onboarding_share.svg"
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
            // 只有用户点击"开始使用"按钮时才设置 FirstLaunch 为 false
            Preferences.Default.Set("FirstLaunch", false);
            
            // 直接设置主页面
            Application.Current.MainPage = _shell;
        }
    }
    
    protected override bool OnBackButtonPressed()
    {
        // 拦截返回按钮，不设置 FirstLaunch
        return true;
    }

    private enum Row { Carousel, Indicator, Button }
}
