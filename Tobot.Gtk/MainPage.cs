using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Tobot.Gtk;

public class MainPage : ContentPage
{
    private enum Mood
    {
        Neutral,
        Happy,
        Terrified,
        Angry
    }

    private readonly Label _distanceLabel;
    private readonly Label _statusLabel;

    private readonly Border _leftEye;
    private readonly Border _rightEye;
    private readonly Border _leftPupil;
    private readonly Border _rightPupil;
    private readonly Border _leftEyelid;
    private readonly Border _rightEyelid;

    private readonly Border _nose;
    private readonly Border _mouth;
    private readonly Border _mouthInner;
    private readonly Border _tongue;

    private Mood _currentMood = Mood.Neutral;
    private CancellationTokenSource _animationCts = new();
    private readonly Random _random = new();

    public MainPage()
    {
        Background = new LinearGradientBrush(
            new GradientStopCollection
            {
                new(Color.FromArgb("#1D1536"), 0.0f),
                new(Color.FromArgb("#100B23"), 1.0f)
            },
            new Point(0, 0),
            new Point(0, 1));

        _distanceLabel = new Label
        {
            Text = "-- cm",
            FontSize = 22,
            TextColor = Color.FromArgb("#D7F7FF"),
            FontFamily = "DejaVu Sans",
            Padding = new Thickness(14, 8),
            BackgroundColor = Color.FromRgba(10, 8, 22, 0.65),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(24, 24, 0, 0)
        };

        var featureStage = new AbsoluteLayout
        {
            WidthRequest = 620,
            HeightRequest = 860,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        _leftEye = CreateEye(out _leftPupil, out _leftEyelid);
        _rightEye = CreateEye(out _rightPupil, out _rightEyelid);

        AbsoluteLayout.SetLayoutBounds(_leftEye, new Rect(40, 170, 250, 250));
        AbsoluteLayout.SetLayoutBounds(_rightEye, new Rect(330, 170, 250, 250));

        _nose = new Border
        {
            WidthRequest = 26,
            HeightRequest = 20,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            BackgroundColor = Color.FromArgb("#F28ABC")
        };
        AbsoluteLayout.SetLayoutBounds(_nose, new Rect(297, 490, 26, 20));

        _mouthInner = new Border
        {
            Margin = new Thickness(14, 14, 14, 10),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(0, 0, 86, 86) },
            BackgroundColor = Color.FromArgb("#6E3777")
        };

        _tongue = new Border
        {
            WidthRequest = 98,
            HeightRequest = 56,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(56, 56, 18, 18) },
            BackgroundColor = Color.FromArgb("#FF62A9"),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 0, 0, -2)
        };

        var mouthInnerGrid = new Grid();
        mouthInnerGrid.Children.Add(_mouthInner);
        mouthInnerGrid.Children.Add(_tongue);

        _mouth = new Border
        {
            WidthRequest = 220,
            HeightRequest = 118,
            StrokeThickness = 5,
            Stroke = Color.FromArgb("#45234F"),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(0, 0, 110, 110) },
            BackgroundColor = Color.FromArgb("#2A1535"),
            Content = mouthInnerGrid
        };

        AbsoluteLayout.SetLayoutBounds(_mouth, new Rect(200, 560, 220, 118));

        featureStage.Children.Add(_leftEye);
        featureStage.Children.Add(_rightEye);
        featureStage.Children.Add(_nose);
        featureStage.Children.Add(_mouth);

        _statusLabel = new Label
        {
            Text = "Wi-Fi: --  |  IP: --  |  CPU --%  |  MEM --%",
            FontSize = 16,
            TextColor = Color.FromRgba(235, 246, 255, 0.9f),
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(10, 0, 10, 14)
        };

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        var contentLayer = new Grid();
        contentLayer.Children.Add(featureStage);
        contentLayer.Children.Add(_distanceLabel);

        root.Children.Add(contentLayer);
        root.Children.Add(_statusLabel);
        Grid.SetRow(_statusLabel, 1);

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (_, _) => CycleMood();
        root.GestureRecognizers.Add(tapGesture);

        Content = root;

        StartAnimations();
        StartStatusSimulation();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _animationCts.Cancel();
    }

    private Border CreateEye(out Border pupil, out Border eyelid)
    {
        pupil = new Border
        {
            WidthRequest = 78,
            HeightRequest = 78,
            StrokeThickness = 0,
            StrokeShape = new Ellipse(),
            Background = new RadialGradientBrush(
                new GradientStopCollection
                {
                    new(Color.FromArgb("#3F2A65"), 0.0f),
                    new(Color.FromArgb("#181129"), 1.0f)
                },
                new Point(0.35f, 0.3f),
                1.0f)
        };

        var pupilHighlight = new Border
        {
            WidthRequest = 18,
            HeightRequest = 18,
            StrokeThickness = 0,
            StrokeShape = new Ellipse(),
            BackgroundColor = Colors.White,
            Opacity = 0.9,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(20, 20, 0, 0)
        };

        var shine = new Border
        {
            WidthRequest = 30,
            HeightRequest = 30,
            StrokeThickness = 0,
            StrokeShape = new Ellipse(),
            BackgroundColor = Color.FromRgba(255, 255, 255, 0.9f),
            Opacity = 0.7,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(72, 58, 0, 0)
        };

        var eyeball = new Grid();
        eyeball.Children.Add(pupil);
        eyeball.Children.Add(pupilHighlight);
        eyeball.Children.Add(shine);

        eyelid = new Border
        {
            WidthRequest = 250,
            HeightRequest = 250,
            StrokeThickness = 0,
            StrokeShape = new Ellipse(),
            Background = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new(Color.FromArgb("#FFD4EB"), 0),
                    new(Color.FromArgb("#FFF2FA"), 1)
                },
                new Point(0, 0),
                new Point(0, 1)),
            TranslationY = -250
        };

        var eyeGrid = new Grid();
        eyeGrid.Children.Add(eyeball);
        eyeGrid.Children.Add(eyelid);

        return new Border
        {
            WidthRequest = 250,
            HeightRequest = 250,
            StrokeThickness = 0,
            StrokeShape = new Ellipse(),
            BackgroundColor = Colors.White,
            Content = eyeGrid
        };
    }

    private void StartStatusSimulation()
    {
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(1200), () =>
        {
            var distance = _random.NextDouble() * 80 + 8;
            _distanceLabel.Text = $"{distance:F1} cm";

            _statusLabel.Text = $"Wi-Fi: tobot-net  |  IP: 192.168.0.{_random.Next(20, 90)}  |  CPU {_random.Next(18, 63)}%  |  MEM {_random.Next(28, 74)}%";

            if (distance < 12)
            {
                _currentMood = Mood.Angry;
            }
            else if (distance > 50)
            {
                _currentMood = Mood.Happy;
            }
            else
            {
                _currentMood = Mood.Neutral;
            }

            ApplyMoodStyle();
            return true;
        });
    }

    private void StartAnimations()
    {
        _ = AnimateEyeBobAsync(_leftEye, TimeSpan.Zero, _animationCts.Token);
        _ = AnimateEyeBobAsync(_rightEye, TimeSpan.FromMilliseconds(120), _animationCts.Token);
        _ = AnimateLookAsync(_leftPupil, _rightPupil, _animationCts.Token);
        _ = AnimateBlinkAsync(_leftEyelid, TimeSpan.Zero, _animationCts.Token);
        _ = AnimateBlinkAsync(_rightEyelid, TimeSpan.FromMilliseconds(80), _animationCts.Token);
        _ = AnimateMouthAsync(_animationCts.Token);
    }

    private async Task AnimateEyeBobAsync(View eye, TimeSpan delay, CancellationToken token)
    {
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, token);
        }

        while (!token.IsCancellationRequested)
        {
            await eye.TranslateToAsync(eye.TranslationX, -8, 1600, Easing.SinInOut);
            await eye.TranslateToAsync(eye.TranslationX, 0, 1600, Easing.SinInOut);
        }
    }

    private async Task AnimateLookAsync(View leftPupil, View rightPupil, CancellationToken token)
    {
        var positions = new[]
        {
            new Point(0, 0),
            new Point(-5, -6),
            new Point(5, -4),
            new Point(-4, 5),
            new Point(4, 4),
            new Point(0, 0)
        };

        while (!token.IsCancellationRequested)
        {
            foreach (var pos in positions)
            {
                await leftPupil.TranslateToAsync(pos.X, pos.Y, 550, Easing.SinInOut);
                await rightPupil.TranslateToAsync(pos.X, pos.Y, 550, Easing.SinInOut);
                if (token.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }

    private async Task AnimateBlinkAsync(View eyelid, TimeSpan delay, CancellationToken token)
    {
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, token);
        }

        while (!token.IsCancellationRequested)
        {
            await Task.Delay(3400, token);
            await eyelid.TranslateToAsync(0, 250, 90, Easing.CubicIn);
            await eyelid.TranslateToAsync(0, 0, 110, Easing.CubicOut);
        }
    }

    private async Task AnimateMouthAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await _mouth.ScaleToAsync(1.0, 900, Easing.SinInOut);
            await _mouth.ScaleToAsync(0.96, 900, Easing.SinInOut);
            await _mouth.ScaleToAsync(1.0, 900, Easing.SinInOut);
        }
    }

    private void CycleMood()
    {
        _currentMood = _currentMood switch
        {
            Mood.Neutral => Mood.Happy,
            Mood.Happy => Mood.Terrified,
            Mood.Terrified => Mood.Angry,
            _ => Mood.Neutral
        };

        ApplyMoodStyle();
    }

    private void ApplyMoodStyle()
    {
        switch (_currentMood)
        {
            case Mood.Happy:
                _leftPupil.WidthRequest = 52;
                _leftPupil.HeightRequest = 52;
                _rightPupil.WidthRequest = 52;
                _rightPupil.HeightRequest = 52;
                _mouth.WidthRequest = 280;
                _mouth.HeightRequest = 150;
                _nose.Scale = 1.08;
                break;

            case Mood.Terrified:
                _leftPupil.WidthRequest = 102;
                _leftPupil.HeightRequest = 102;
                _rightPupil.WidthRequest = 102;
                _rightPupil.HeightRequest = 102;
                _mouth.WidthRequest = 150;
                _mouth.HeightRequest = 160;
                _nose.Opacity = 0.0;
                break;

            case Mood.Angry:
                _leftPupil.WidthRequest = 38;
                _leftPupil.HeightRequest = 58;
                _rightPupil.WidthRequest = 38;
                _rightPupil.HeightRequest = 58;
                _mouth.WidthRequest = 250;
                _mouth.HeightRequest = 86;
                _nose.Opacity = 1.0;
                _nose.BackgroundColor = Color.FromArgb("#D35A8D");
                break;

            default:
                _leftPupil.WidthRequest = 78;
                _leftPupil.HeightRequest = 78;
                _rightPupil.WidthRequest = 78;
                _rightPupil.HeightRequest = 78;
                _mouth.WidthRequest = 220;
                _mouth.HeightRequest = 118;
                _nose.Opacity = 0.9;
                _nose.Scale = 1.0;
                _nose.BackgroundColor = Color.FromArgb("#F28ABC");
                break;
        }
    }
}
