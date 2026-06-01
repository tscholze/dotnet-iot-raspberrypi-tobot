using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Tobot.Gtk;

public class MainPage : ContentPage
{
    private readonly Label _distanceLabel;
    private readonly Label _statusLabel;
    private readonly GraphicsView _graphicsView;
    private readonly BotFaceDrawable _drawable;
    private readonly Random _random = new();

    public MainPage()
    {
        BackgroundColor = Color.FromArgb("#130F1F");

        _drawable = new BotFaceDrawable();
        _graphicsView = new GraphicsView
        {
            Drawable = _drawable,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

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

        _statusLabel = new Label
        {
            Text = "Wi-Fi: --  |  IP: --  |  CPU --%  |  MEM --%",
            FontSize = 16,
            TextColor = Color.FromRgba(235, 246, 255, 0.9f),
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(10, 0, 10, 14)
        };

        var root = new Grid();

        root.Children.Add(_graphicsView);
        root.Children.Add(_distanceLabel);
        root.Children.Add(_statusLabel);

        _graphicsView.ZIndex = 0;
        _distanceLabel.ZIndex = 1;
        _statusLabel.ZIndex = 1;
        _statusLabel.VerticalOptions = LayoutOptions.End;

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (_, _) =>
        {
            _drawable.CycleMood();
            _graphicsView.Invalidate();
        };
        root.GestureRecognizers.Add(tapGesture);

        Content = root;

        StartAnimationLoop();
        StartStatusSimulation();
    }

    private void StartAnimationLoop()
    {
        var lastTick = DateTime.UtcNow;

        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(33), () =>
        {
            var now = DateTime.UtcNow;
            var delta = (now - lastTick).TotalSeconds;
            lastTick = now;

            _drawable.Advance(delta);
            _graphicsView.Invalidate();
            return true;
        });
    }

    private void StartStatusSimulation()
    {
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(1200), () =>
        {
            var distance = _random.NextDouble() * 80 + 8;
            _distanceLabel.Text = $"{distance:F1} cm";
            _statusLabel.Text = $"Wi-Fi: tobot-net  |  IP: 192.168.0.{_random.Next(20, 90)}  |  CPU {_random.Next(18, 63)}%  |  MEM {_random.Next(28, 74)}%";

            _drawable.SetMoodFromDistance(distance);
            _graphicsView.Invalidate();
            return true;
        });
    }
}

internal sealed class BotFaceDrawable : IDrawable
{
    private enum Mood
    {
        Neutral,
        Happy,
        Terrified,
        Angry
    }

    private Mood _mood = Mood.Neutral;
    private double _elapsed;
    private double _blink;
    private double _nextBlink = 2.8;
    private double _leftBob;
    private double _rightBob;
    private float _pupilOffsetX;
    private float _pupilOffsetY;
    private float _mouthScale = 1f;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();
        canvas.Antialias = true;

        var usableWidth = dirtyRect.Width * 0.88f;
        var usableHeight = dirtyRect.Height * 0.84f;
        var faceWidth = MathF.Min(usableWidth, usableHeight * 0.76f);
        var eyeWidth = _mood == Mood.Terrified ? faceWidth * 0.43f : faceWidth * 0.40f;
        var eyeHeight = _mood switch
        {
            Mood.Happy => eyeWidth * 0.82f,
            Mood.Angry => eyeWidth * 0.88f,
            Mood.Terrified => eyeWidth,
            _ => eyeWidth * 0.94f
        };

        var eyeGap = faceWidth * (_mood == Mood.Terrified ? 0.055f : 0.085f);
        var centerX = dirtyRect.Center.X;
        var eyeTop = dirtyRect.Height * 0.24f;
        var leftEyeX = centerX - eyeGap / 2f - eyeWidth;
        var rightEyeX = centerX + eyeGap / 2f;

        DrawEye(canvas, leftEyeX, eyeTop + (float)_leftBob, eyeWidth, eyeHeight, true);
        DrawEye(canvas, rightEyeX, eyeTop + (float)_rightBob, eyeWidth, eyeHeight, false);
        DrawNose(canvas, centerX, dirtyRect.Height * 0.53f);
        DrawMouth(canvas, centerX, dirtyRect.Height * 0.67f, faceWidth);

        canvas.RestoreState();
    }

    public void Advance(double deltaSeconds)
    {
        _elapsed += deltaSeconds;

        _leftBob = Math.Sin(_elapsed * 2.2) * 8;
        _rightBob = Math.Sin((_elapsed + 0.12) * 2.2) * 8;

        _pupilOffsetX = (float)(Math.Sin(_elapsed * 1.3) * 5);
        _pupilOffsetY = (float)(Math.Cos(_elapsed * 1.7) * 4);

        _mouthScale = _mood switch
        {
            Mood.Happy => 1.08f + (float)(Math.Sin(_elapsed * 6.0) * 0.05),
            Mood.Terrified => 1.02f + (float)(Math.Sin(_elapsed * 9.0) * 0.04),
            Mood.Angry => 1.0f + (float)(Math.Sin(_elapsed * 15.0) * 0.02),
            _ => 0.96f + (float)(Math.Sin(_elapsed * 3.4) * 0.04)
        };

        _nextBlink -= deltaSeconds;
        if (_nextBlink <= 0)
        {
            _blink += deltaSeconds * 10;
            if (_blink >= 2)
            {
                _blink = 0;
                _nextBlink = 2.4 + Random.Shared.NextDouble() * 1.8;
            }
        }
    }

    public void CycleMood()
    {
        _mood = _mood switch
        {
            Mood.Neutral => Mood.Happy,
            Mood.Happy => Mood.Terrified,
            Mood.Terrified => Mood.Angry,
            _ => Mood.Neutral
        };
    }

    public void SetMoodFromDistance(double distance)
    {
        _mood = distance switch
        {
            < 12 => Mood.Angry,
            > 50 => Mood.Happy,
            _ => Mood.Neutral
        };
    }

    private void DrawEye(ICanvas canvas, float x, float y, float eyeWidth, float eyeHeight, bool left)
    {
        canvas.FillColor = Colors.White;
        canvas.FillEllipse(x, y, eyeWidth, eyeHeight);

        var pupilWidth = _mood switch
        {
            Mood.Happy => eyeWidth * 0.20f,
            Mood.Terrified => eyeWidth * 0.38f,
            Mood.Angry => eyeWidth * 0.15f,
            _ => eyeWidth * 0.29f
        };

        var pupilHeight = _mood switch
        {
            Mood.Angry => eyeHeight * 0.26f,
            _ => pupilWidth
        };

        var pupilX = x + (eyeWidth - pupilWidth) / 2f + _pupilOffsetX;
        var pupilY = y + (eyeHeight - pupilHeight) / 2f + _pupilOffsetY;

        canvas.FillColor = _mood == Mood.Angry ? Color.FromArgb("#780F0F") : Color.FromArgb("#241636");
        canvas.FillEllipse(pupilX, pupilY, pupilWidth, pupilHeight);

        canvas.FillColor = Colors.White;
        canvas.FillEllipse(pupilX + pupilWidth * 0.22f, pupilY + pupilHeight * 0.22f, pupilWidth * 0.24f, pupilWidth * 0.24f);
        canvas.FillEllipse(pupilX + pupilWidth * 0.64f, pupilY + pupilHeight * 0.30f, pupilWidth * 0.14f, pupilWidth * 0.14f);

        var blinkValue = GetBlinkValue();
        if (_mood != Mood.Terrified && blinkValue > 0.01f)
        {
            canvas.FillColor = Color.FromArgb("#FFD4EB");
            canvas.FillRoundedRectangle(x, y, eyeWidth, eyeHeight * blinkValue, eyeWidth * 0.36f);
        }

        if (_mood == Mood.Angry)
        {
            canvas.StrokeColor = Color.FromArgb("#7B4B99");
            canvas.StrokeSize = 8;
            if (left)
            {
                canvas.DrawLine(x + eyeWidth * 0.08f, y + eyeHeight * 0.14f, x + eyeWidth * 0.92f, y + eyeHeight * 0.03f);
            }
            else
            {
                canvas.DrawLine(x + eyeWidth * 0.08f, y + eyeHeight * 0.03f, x + eyeWidth * 0.92f, y + eyeHeight * 0.14f);
            }
        }
    }

    private void DrawNose(ICanvas canvas, float centerX, float top)
    {
        if (_mood == Mood.Terrified)
        {
            return;
        }

        var noseWidth = 26f;
        var noseHeight = 20f;
        var x = centerX - (noseWidth / 2f);

        canvas.FillColor = _mood == Mood.Angry ? Color.FromArgb("#D35A8D") : Color.FromArgb("#F28ABC");
        canvas.FillEllipse(x, top, noseWidth, noseHeight);
        canvas.FillColor = Color.FromRgba(255, 255, 255, 0.7f);
        canvas.FillEllipse(x + 8, top + 4, 8, 5);
    }

    private void DrawMouth(ICanvas canvas, float centerX, float centerY, float faceWidth)
    {
        var width = _mood switch
        {
            Mood.Happy => faceWidth * 0.44f,
            Mood.Terrified => faceWidth * 0.24f,
            Mood.Angry => faceWidth * 0.39f,
            _ => faceWidth * 0.34f
        };

        var height = _mood switch
        {
            Mood.Happy => faceWidth * 0.24f,
            Mood.Terrified => faceWidth * 0.25f,
            Mood.Angry => faceWidth * 0.135f,
            _ => faceWidth * 0.18f
        };

        var scaledWidth = width * _mouthScale;
        var scaledHeight = height * _mouthScale;
        var x = centerX - scaledWidth / 2f;
        var y = centerY - scaledHeight / 2f;

        canvas.FillColor = Color.FromArgb("#2A1535");
        canvas.FillRoundedRectangle(x, y, scaledWidth, scaledHeight, _mood == Mood.Terrified ? 70 : 36);

        if (_mood != Mood.Angry)
        {
            canvas.FillColor = Color.FromArgb("#6E3777");
            canvas.FillRoundedRectangle(x + 14, y + 14, scaledWidth - 28, scaledHeight - 24, _mood == Mood.Terrified ? 56 : 32);

            if (_mood != Mood.Terrified)
            {
                var tongueWidth = MathF.Min(98, scaledWidth - 60);
                var tongueHeight = MathF.Min(56, scaledHeight * 0.45f);
                canvas.FillColor = Color.FromArgb("#FF62A9");
                canvas.FillRoundedRectangle(x + (scaledWidth - tongueWidth) / 2f, y + scaledHeight - tongueHeight - 2, tongueWidth, tongueHeight, 24);
            }
        }
        else
        {
            canvas.StrokeColor = Color.FromArgb("#9E3F58");
            canvas.StrokeSize = 4;
            for (var offset = 16f; offset < scaledWidth - 12f; offset += 16f)
            {
                canvas.DrawLine(x + offset, y + 12, x + offset - 10, y + scaledHeight - 10);
            }
        }
    }

    private float GetBlinkValue()
    {
        if (_blink <= 0)
        {
            return 0f;
        }

        if (_blink < 1)
        {
            return (float)_blink;
        }

        return Math.Max(0f, (float)(2 - _blink));
    }
}
