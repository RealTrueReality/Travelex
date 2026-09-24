using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Controls.Shapes;
using Travelex.Models;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace Travelex.Pages;

public class OnboardingPage : ContentPage
{
    private static readonly Color BrandBlue = Color.FromArgb("#0085FF");
    private static readonly Color BrandCyan = Color.FromArgb("#35C6E8");

    private readonly AppShell _shell;
    private readonly bool _isDarkMode;
    private readonly ObservableCollection<OnboardingModel> _onboardingItems;

    private CarouselView _carousel = null!;
    private IndicatorView _indicator = null!;
    private Button _nextButton = null!;
    private Label _stepLabel = null!;
    private double _visualHeight = 345;
    private bool _isCompleting;

    public double VisualHeight
    {
        get => _visualHeight;
        private set
        {
            if (Math.Abs(_visualHeight - value) < 0.5) return;
            _visualHeight = value;
            OnPropertyChanged();
        }
    }

    public OnboardingPage(AppShell shell)
    {
        _shell = shell;
        _isDarkMode = Application.Current?.RequestedTheme == AppTheme.Dark;
        _onboardingItems = CreateOnboardingItems();

        NavigationPage.SetHasNavigationBar(this, false);
        Shell.SetNavBarIsVisible(this, false);

        Background = CreatePageBackground();
        Content = BuildContent();
        UpdateProgress(0);
    }

    private View BuildContent()
    {
        _indicator = new IndicatorView
        {
            IndicatorColor = _isDarkMode ? Color.FromArgb("#334155") : Color.FromArgb("#DCE7F3"),
            SelectedIndicatorColor = BrandBlue,
            IndicatorSize = 8,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center
        };

        _carousel = new CarouselView
        {
            ItemsSource = _onboardingItems,
            IndicatorView = _indicator,
            Loop = false,
            IsSwipeEnabled = true,
            ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset,
            ItemTemplate = CreateSlideTemplate()
        };
        _carousel.PositionChanged += OnPositionChanged;

        _nextButton = new Button
        {
            Text = "继续  →",
            TextColor = Colors.White,
            BackgroundColor = BrandBlue,
            FontFamily = "HarmonyMedium",
            FontSize = 16,
            HeightRequest = 56,
            CornerRadius = 28,
            HorizontalOptions = LayoutOptions.Fill,
            Padding = new Thickness(24, 0),
            Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#660085FF")),
                Offset = new Point(0, 10),
                Radius = 22,
                Opacity = 0.35f
            }
        };
        _nextButton.Clicked += OnNextClicked;

        _stepLabel = new Label
        {
            FontFamily = "PJMedium",
            FontSize = 12,
            CharacterSpacing = 1.4,
            TextColor = _isDarkMode ? Color.FromArgb("#94A3B8") : Color.FromArgb("#64748B"),
            VerticalTextAlignment = TextAlignment.Center
        };

        var skipButton = new Button
        {
            Text = "跳过",
            TextColor = _isDarkMode ? Color.FromArgb("#CBD5E1") : Color.FromArgb("#475569"),
            BackgroundColor = Colors.Transparent,
            FontFamily = "HarmonyMedium",
            FontSize = 14,
            Padding = new Thickness(16, 8),
            CornerRadius = 18,
            HorizontalOptions = LayoutOptions.End
        };
        skipButton.Clicked += OnSkipClicked;



        var brand = new HorizontalStackLayout
        {
            Spacing = 10,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = "TRAVELEX",
                    FontFamily = "PJBold",
                    FontSize = 15,
                    CharacterSpacing = 1.8,
                    TextColor = _isDarkMode ? Colors.White : Color.FromArgb("#102A43"),
                    VerticalTextAlignment = TextAlignment.Center
                }
            }
        };

        var header = new Grid
        {
            Padding = new Thickness(24, 10, 16, 4),
            ColumnDefinitions = Columns.Define(Star, Auto),
            Children =
            {
                brand.Column(0),
                skipButton.Column(1)
            }
        };

        var progress = new VerticalStackLayout
        {
            Spacing = 9,
            VerticalOptions = LayoutOptions.Center,
            Children = { _stepLabel, _indicator }
        };

        var footer = new Grid
        {
            Padding = new Thickness(24, 10, 24, 26),
            ColumnSpacing = 24,
            ColumnDefinitions = Columns.Define(new GridLength(84), Star),
            Children =
            {
                progress.Column(0),
                _nextButton.Column(1)
            }
        };

        return new Grid
        {
            RowDefinitions = Rows.Define(Auto, Star, Auto),
            Children =
            {
                header.Row(PageRow.Header),
                _carousel.Row(PageRow.Content),
                footer.Row(PageRow.Footer)
            }
        };
    }

    private DataTemplate CreateSlideTemplate()
    {
        return new DataTemplate(() =>
        {
            var image = new Image
            {
                Aspect = Aspect.AspectFit,
                Margin = new Thickness(18),
                MaximumHeightRequest = 330,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            }.Bind(Image.SourceProperty, nameof(OnboardingModel.ImageSource));

            var glowTop = new Ellipse
            {
                WidthRequest = 150,
                HeightRequest = 150,
                Fill = new SolidColorBrush(Color.FromArgb(_isDarkMode ? "#17345C85" : "#3035C6E8")),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                TranslationX = 42,
                TranslationY = -40
            };

            var glowBottom = new Ellipse
            {
                WidthRequest = 110,
                HeightRequest = 110,
                Fill = new SolidColorBrush(Color.FromArgb(_isDarkMode ? "#202563EB" : "#268B7CFF")),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.End,
                TranslationX = -30,
                TranslationY = 34
            };

            var visualCard = new Border
            {
                MaximumWidthRequest = 520,
                Margin = new Thickness(20, 4, 20, 16),
                Padding = 0,
                Stroke = new SolidColorBrush(_isDarkMode ? Color.FromArgb("#263B82F6") : Color.FromArgb("#80FFFFFF")),
                StrokeThickness = 1,
                Background = CreateVisualCardBackground(),
                StrokeShape = new RoundRectangle { CornerRadius = 36 },
                Shadow = new Shadow
                {
                    Brush = new SolidColorBrush(_isDarkMode ? Colors.Black : Color.FromArgb("#334B79A1")),
                    Offset = new Point(0, 14),
                    Radius = 30,
                    Opacity = _isDarkMode ? 0.35f : 0.18f
                },
                Content = new Grid
                {
                    IsClippedToBounds = true,
                    Children = { glowTop, glowBottom, image }
                }
            };
            visualCard.SetBinding(HeightRequestProperty, new Binding(nameof(VisualHeight), source: this));

            var eyebrow = new Label
            {
                FontFamily = "HarmonyMedium",
                FontSize = 13,
                CharacterSpacing = 1.2,
                TextColor = BrandBlue,
                HorizontalTextAlignment = TextAlignment.Center
            }.Bind(Label.TextProperty, nameof(OnboardingModel.Eyebrow));

            var title = new Label
            {
                FontFamily = "HarmonyBold",
                FontSize = 30,
                LineHeight = 1.1,
                TextColor = _isDarkMode ? Color.FromArgb("#F8FAFC") : Color.FromArgb("#102A43"),
                HorizontalTextAlignment = TextAlignment.Center
            }.Bind(Label.TextProperty, nameof(OnboardingModel.Title));

            var description = new Label
            {
                FontFamily = "HarmonyRegular",
                FontSize = 16,
                LineHeight = 1.45,
                TextColor = _isDarkMode ? Color.FromArgb("#A8B7CA") : Color.FromArgb("#5D7187"),
                HorizontalTextAlignment = TextAlignment.Center,
                MaximumWidthRequest = 420
            }.Bind(Label.TextProperty, nameof(OnboardingModel.Description));

            var copy = new VerticalStackLayout
            {
                Spacing = 10,
                Padding = new Thickness(28, 4, 28, 8),
                HorizontalOptions = LayoutOptions.Center,
                Children = { eyebrow, title, description }
            };

            return new VerticalStackLayout
            {
                Spacing = 4,
                VerticalOptions = LayoutOptions.Center,
                Children = { visualCard, copy }
            };
        });
    }

    private Brush CreatePageBackground()
    {
        return new LinearGradientBrush(
            new GradientStopCollection
            {
                new(_isDarkMode ? Color.FromArgb("#07111F") : Color.FromArgb("#F7FBFF"), 0),
                new(_isDarkMode ? Color.FromArgb("#0B1728") : Colors.White, 0.55f),
                new(_isDarkMode ? Color.FromArgb("#08101D") : Color.FromArgb("#F3F8FF"), 1)
            },
            new Point(0, 0),
            new Point(1, 1));
    }

    private Brush CreateVisualCardBackground()
    {
        return new LinearGradientBrush(
            new GradientStopCollection
            {
                new(_isDarkMode ? Color.FromArgb("#172B46") : Color.FromArgb("#EAF6FF"), 0),
                new(_isDarkMode ? Color.FromArgb("#102137") : Color.FromArgb("#F8FCFF"), 0.52f),
                new(_isDarkMode ? Color.FromArgb("#171F3C") : Color.FromArgb("#F0EDFF"), 1)
            },
            new Point(0, 0),
            new Point(1, 1));
    }

    private static ObservableCollection<OnboardingModel> CreateOnboardingItems()
    {
        return
        [
            new OnboardingModel
            {
                Eyebrow = "行程管理",
                Title = "旅程，从容启程",
                Description = "把目的地、日期和灵感收进同一个行程，从计划开始就井井有条。",
                ImageSource = "onboarding_journey.png"
            },
            new OnboardingModel
            {
                Eyebrow = "智能预算",
                Title = "花得明白，玩得尽兴",
                Description = "每一笔支出自动归类，预算、趋势与消费结构随时清晰可见。",
                ImageSource = "onboarding_budget_3d.png"
            },
            new OnboardingModel
            {
                Eyebrow = "AI 洞察",
                Title = "让 AI 成为旅行搭子",
                Description = "让 AI 读懂你的旅行账单，发现趋势并给出真正可执行的建议。",
                ImageSource = "onboarding_ai_3d.png"
            }
        ];
    }

    private async void OnPositionChanged(object? sender, PositionChangedEventArgs e)
    {
        UpdateProgress(e.CurrentPosition);
        _nextButton.Opacity = 0.7;
        await _nextButton.FadeToAsync(1, 160, Easing.CubicOut);
    }

    private void UpdateProgress(int position)
    {
        _stepLabel.Text = $"{position + 1:00}  /  {_onboardingItems.Count:00}";
        _nextButton.Text = position == _onboardingItems.Count - 1 ? "开启旅程  →" : "继续  →";
    }

    private void OnNextClicked(object? sender, EventArgs e)
    {
        if (_carousel.Position < _onboardingItems.Count - 1)
        {
            _carousel.Position += 1;
            return;
        }

        _ = CompleteOnboardingAsync();
    }

    private void OnSkipClicked(object? sender, EventArgs e) => _ = CompleteOnboardingAsync();

    private async Task CompleteOnboardingAsync()
    {
        if (_isCompleting) return;
        _isCompleting = true;
        _nextButton.IsEnabled = false;
        Preferences.Default.Set("FirstLaunch", false);

        var window = Window ?? Application.Current?.Windows.FirstOrDefault();
        if (window is null) return;

        await this.FadeToAsync(0, 180, Easing.CubicIn);
        window.Page = _shell;
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        VisualHeight = height switch
        {
            < 650 => 245,
            < 760 => 285,
            < 880 => 320,
            _ => 345
        };
    }

    protected override bool OnBackButtonPressed() => true;

    private enum PageRow { Header, Content, Footer }
}
