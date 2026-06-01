using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Tobot.Gtk;

/// <summary>
/// Provides the main portrait bot page for the GTK target.
/// </summary>
public class MainPage : ContentPage
{
    /// <summary>
    /// Label showing the latest distance text at the top-left corner.
    /// </summary>
    private readonly Label _distanceLabel;

    /// <summary>
    /// Label showing simulated network and system status at the bottom.
    /// </summary>
    private readonly Label _statusLabel;

    /// <summary>
    /// Full-window drawing surface for the animated face.
    /// </summary>
    private readonly GraphicsView _graphicsView;

    /// <summary>
    /// Drawable that renders and animates the bot face.
    /// </summary>
    private readonly BotFaceDrawable _drawable;

    /// <summary>
    /// Random source used for simulated status values.
    /// </summary>
    private readonly Random _random = new();

    /// <summary>
    /// Initializes the page layout, creates overlays, and starts animation/data loops.
    /// </summary>
    public MainPage()
    {
        // Step 1: Set the background color used behind the face.
        BackgroundColor = Color.FromArgb("#130F1F");

        // Step 2: Create drawable and full-window graphics host.
        _drawable = new BotFaceDrawable();
        _graphicsView = new GraphicsView
        {
            Drawable = _drawable,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        // Step 3: Create the top-left distance overlay.
        _distanceLabel = new Label
        {
            Text = "--cm",
            FontSize = 22,
            TextColor = Color.FromArgb("#D7F7FF"),
            FontFamily = "DejaVu Sans",
            Padding = new Thickness(14, 8),
            BackgroundColor = Color.FromRgba(10, 8, 22, 0.65),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            LineBreakMode = LineBreakMode.NoWrap,
            MaxLines = 1,
            MinimumWidthRequest = 110,
            Margin = new Thickness(24, 24, 0, 0)
        };

        // Step 4: Create the bottom status overlay.
        _statusLabel = new Label
        {
            Text = "Wi-Fi: --  |  IP: --  |  CPU --%  |  MEM --%",
            FontSize = 16,
            TextColor = Color.FromRgba(235, 246, 255, 0.9f),
            HorizontalOptions = LayoutOptions.Fill,
            HorizontalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.NoWrap,
            MaxLines = 1,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(10, 0, 10, 14)
        };

        // Step 5: Layer the drawing surface and overlays in one grid.
        var root = new Grid
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };
        root.Children.Add(_graphicsView);
        root.Children.Add(_distanceLabel);
        root.Children.Add(_statusLabel);
        Grid.SetRowSpan(_graphicsView, 2);
        Grid.SetRow(_distanceLabel, 0);
        Grid.SetRow(_statusLabel, 1);

        // Step 6: Ensure overlays stay above the rendered face.
        _graphicsView.ZIndex = 0;
        _distanceLabel.ZIndex = 1;
        _statusLabel.ZIndex = 1;

        // Step 7: Add tap support to manually cycle moods for testing.
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (_, _) =>
        {
            _drawable.CycleMood();
            _graphicsView.Invalidate();
        };
        root.GestureRecognizers.Add(tapGesture);

        // Step 8: Assign content and start update loops.
        Content = root;
        StartAnimationLoop();
        StartStatusSimulation();
    }

    /// <summary>
    /// Starts a high-frequency timer that advances animation state and redraws the face.
    /// </summary>
    private void StartAnimationLoop()
    {
        // Step 1: Capture initial timestamp for delta-time calculations.
        var lastTick = DateTime.UtcNow;

        // Step 2: Tick at ~30 FPS for smooth but lightweight animation.
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(33), () =>
        {
            // Step 3: Compute elapsed seconds since previous frame.
            var now = DateTime.UtcNow;
            var delta = (now - lastTick).TotalSeconds;
            lastTick = now;

            // Step 4: Advance state and force redraw.
            _drawable.Advance(delta);
            _graphicsView.Invalidate();
            return true;
        });
    }

    /// <summary>
    /// Starts a timer that simulates distance and system data for the status overlays.
    /// </summary>
    private void StartStatusSimulation()
    {
        // Step 1: Tick slower than animation for readable telemetry updates.
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(1200), () =>
        {
            // Step 2: Simulate distance and update the top-left label.
            var distance = _random.NextDouble() * 80 + 8;
            _distanceLabel.Text = $"{distance:F1}cm";

            // Step 3: Simulate networking/system line and update bottom label.
            _statusLabel.Text = $"Wi-Fi: tobot-net  |  IP: 192.168.0.{_random.Next(20, 90)}  |  CPU {_random.Next(18, 63)}%  |  MEM {_random.Next(28, 74)}%";

            // Step 4: Drive mood from distance and redraw.
            _drawable.SetMoodFromDistance(distance);
            _graphicsView.Invalidate();
            return true;
        });
    }
}

/// <summary>
/// Draws and animates a kawaii bot face using MAUI graphics primitives.
/// </summary>
internal sealed class BotFaceDrawable : IDrawable
{
    /// <summary>
    /// Represents facial expression modes.
    /// </summary>
    private enum Mood
    {
        /// <summary>
        /// Default calm expression.
        /// </summary>
        Neutral,

        /// <summary>
        /// Cheerful expression with smaller pupils and bigger mouth.
        /// </summary>
        Happy,

        /// <summary>
        /// Shocked expression with larger eyes and reduced features.
        /// </summary>
        Terrified,

        /// <summary>
        /// Angry expression with narrow pupils and brow lines.
        /// </summary>
        Angry
    }

    /// <summary>
    /// Current active mood.
    /// </summary>
    private Mood _mood = Mood.Neutral;

    /// <summary>
    /// Total elapsed animation time in seconds.
    /// </summary>
    private double _elapsed;

    /// <summary>
    /// Blink phase accumulator.
    /// </summary>
    private double _blink;

    /// <summary>
    /// Time until next blink starts.
    /// </summary>
    private double _nextBlink = 2.8;

    /// <summary>
    /// Vertical bob offset for the left eye.
    /// </summary>
    private double _leftBob;

    /// <summary>
    /// Vertical bob offset for the right eye.
    /// </summary>
    private double _rightBob;

    /// <summary>
    /// Horizontal pupil offset.
    /// </summary>
    private float _pupilOffsetX;

    /// <summary>
    /// Vertical pupil offset.
    /// </summary>
    private float _pupilOffsetY;

    /// <summary>
    /// Animated mouth scale factor.
    /// </summary>
    private float _mouthScale = 1f;

    /// <summary>
    /// Draws the full face for the current frame.
    /// </summary>
    /// <param name="canvas">Canvas receiving draw commands.</param>
    /// <param name="dirtyRect">Current drawable viewport.</param>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // Step 1: Save canvas state and enable anti-aliasing.
        canvas.SaveState();
        canvas.Antialias = true;

        // Step 2: Compute dynamic face size from available viewport area.
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

        // Step 3: Compute face center and feature anchors.
        var eyeGap = faceWidth * (_mood == Mood.Terrified ? 0.055f : 0.085f);
        var centerX = dirtyRect.Center.X - (dirtyRect.Width * 0.14f);
        var eyeTop = dirtyRect.Height * 0.24f;
        var leftEyeX = centerX - eyeGap / 2f - eyeWidth;
        var rightEyeX = centerX + eyeGap / 2f;

        // Step 4: Draw all features.
        DrawEye(canvas, leftEyeX, eyeTop + (float)_leftBob, eyeWidth, eyeHeight, true);
        DrawEye(canvas, rightEyeX, eyeTop + (float)_rightBob, eyeWidth, eyeHeight, false);
        DrawNose(canvas, centerX, dirtyRect.Height * 0.53f);
        DrawMouth(canvas, centerX, dirtyRect.Height * 0.67f, faceWidth);

        // Step 5: Restore canvas state.
        canvas.RestoreState();
    }

    /// <summary>
    /// Advances internal animation values using the provided delta time.
    /// </summary>
    /// <param name="deltaSeconds">Elapsed time in seconds since previous frame.</param>
    public void Advance(double deltaSeconds)
    {
        // Step 1: Advance global time.
        _elapsed += deltaSeconds;

        // Step 2: Animate eye bob.
        _leftBob = Math.Sin(_elapsed * 2.2) * 8;
        _rightBob = Math.Sin((_elapsed + 0.12) * 2.2) * 8;

        // Step 3: Animate pupil movement.
        _pupilOffsetX = (float)(Math.Sin(_elapsed * 1.3) * 5);
        _pupilOffsetY = (float)(Math.Cos(_elapsed * 1.7) * 4);

        // Step 4: Animate mouth scale with mood-dependent cadence.
        _mouthScale = _mood switch
        {
            Mood.Happy => 1.08f + (float)(Math.Sin(_elapsed * 6.0) * 0.05),
            Mood.Terrified => 1.02f + (float)(Math.Sin(_elapsed * 9.0) * 0.04),
            Mood.Angry => 1.0f + (float)(Math.Sin(_elapsed * 15.0) * 0.02),
            _ => 0.96f + (float)(Math.Sin(_elapsed * 3.4) * 0.04)
        };

        // Step 5: Advance blink cycle and schedule next blink when complete.
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

    /// <summary>
    /// Cycles to the next mood in the manual test sequence.
    /// </summary>
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

    /// <summary>
    /// Sets mood from a distance value.
    /// </summary>
    /// <param name="distance">Distance in centimeters.</param>
    public void SetMoodFromDistance(double distance)
    {
        _mood = distance switch
        {
            < 12 => Mood.Angry,
            > 50 => Mood.Happy,
            _ => Mood.Neutral
        };
    }

    /// <summary>
    /// Draws a single eye with pupil, highlights, optional blink overlay, and angry brow.
    /// </summary>
    /// <param name="canvas">Canvas receiving draw commands.</param>
    /// <param name="x">Eye left position.</param>
    /// <param name="y">Eye top position.</param>
    /// <param name="eyeWidth">Eye width.</param>
    /// <param name="eyeHeight">Eye height.</param>
    /// <param name="left">True when drawing the left eye.</param>
    private void DrawEye(ICanvas canvas, float x, float y, float eyeWidth, float eyeHeight, bool left)
    {
        // Step 1: Draw eye white.
        canvas.FillColor = Colors.White;
        canvas.FillEllipse(x, y, eyeWidth, eyeHeight);

        // Step 2: Compute pupil size by mood.
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

        // Step 3: Center pupil and apply animated offsets.
        var pupilX = x + (eyeWidth - pupilWidth) / 2f + _pupilOffsetX;
        var pupilY = y + (eyeHeight - pupilHeight) / 2f + _pupilOffsetY;

        // Step 4: Draw pupil base.
        canvas.FillColor = _mood == Mood.Angry ? Color.FromArgb("#780F0F") : Color.FromArgb("#241636");
        canvas.FillEllipse(pupilX, pupilY, pupilWidth, pupilHeight);

        // Step 5: Draw glossy highlights.
        canvas.FillColor = Colors.White;
        canvas.FillEllipse(pupilX + pupilWidth * 0.22f, pupilY + pupilHeight * 0.22f, pupilWidth * 0.24f, pupilWidth * 0.24f);
        canvas.FillEllipse(pupilX + pupilWidth * 0.64f, pupilY + pupilHeight * 0.30f, pupilWidth * 0.14f, pupilWidth * 0.14f);

        // Step 6: Draw blink overlay while blink is active.
        var blinkValue = GetBlinkValue();
        if (_mood != Mood.Terrified && blinkValue > 0.01f)
        {
            canvas.FillColor = Color.FromArgb("#FFD4EB");
            canvas.FillRoundedRectangle(x, y, eyeWidth, eyeHeight * blinkValue, eyeWidth * 0.36f);
        }

        // Step 7: Draw brow accent in angry mode.
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

    /// <summary>
    /// Draws the nose centered on the current face center line.
    /// </summary>
    /// <param name="canvas">Canvas receiving draw commands.</param>
    /// <param name="centerX">Face center X coordinate.</param>
    /// <param name="top">Nose top Y coordinate.</param>
    private void DrawNose(ICanvas canvas, float centerX, float top)
    {
        // Step 1: Hide nose in terrified mode.
        if (_mood == Mood.Terrified)
        {
            return;
        }

        // Step 2: Compute centered nose position.
        var noseWidth = 26f;
        var noseHeight = 20f;
        var x = centerX - (noseWidth / 2f);

        // Step 3: Draw nose body and highlight.
        canvas.FillColor = _mood == Mood.Angry ? Color.FromArgb("#D35A8D") : Color.FromArgb("#F28ABC");
        canvas.FillEllipse(x, top, noseWidth, noseHeight);
        canvas.FillColor = Color.FromRgba(255, 255, 255, 0.7f);
        canvas.FillEllipse(x + 8, top + 4, 8, 5);
    }

    /// <summary>
    /// Draws the mouth, inner cavity, and tongue or angry texture depending on mood.
    /// </summary>
    /// <param name="canvas">Canvas receiving draw commands.</param>
    /// <param name="centerX">Face center X coordinate.</param>
    /// <param name="centerY">Mouth center Y coordinate.</param>
    /// <param name="faceWidth">Overall face width used to scale mouth proportions.</param>
    private void DrawMouth(ICanvas canvas, float centerX, float centerY, float faceWidth)
    {
        // Step 1: Compute mood-specific mouth dimensions.
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

        // Step 2: Apply animated scaling and center the mouth.
        var scaledWidth = width * _mouthScale;
        var scaledHeight = height * _mouthScale;
        var x = centerX - scaledWidth / 2f;
        var y = centerY - scaledHeight / 2f;

        // Step 3: Draw outer mouth shape.
        canvas.FillColor = Color.FromArgb("#2A1535");
        canvas.FillRoundedRectangle(x, y, scaledWidth, scaledHeight, _mood == Mood.Terrified ? 70 : 36);

        if (_mood != Mood.Angry)
        {
            // Step 4: Draw inner cavity for non-angry moods.
            canvas.FillColor = Color.FromArgb("#6E3777");
            canvas.FillRoundedRectangle(x + 14, y + 14, scaledWidth - 28, scaledHeight - 24, _mood == Mood.Terrified ? 56 : 32);

            if (_mood != Mood.Terrified)
            {
                // Step 5: Draw tongue for neutral/happy moods.
                var tongueWidth = MathF.Min(98, scaledWidth - 60);
                var tongueHeight = MathF.Min(56, scaledHeight * 0.45f);
                canvas.FillColor = Color.FromArgb("#FF62A9");
                canvas.FillRoundedRectangle(x + (scaledWidth - tongueWidth) / 2f, y + scaledHeight - tongueHeight - 2, tongueWidth, tongueHeight, 24);
            }
        }
        else
        {
            // Step 4 (angry): Draw striped texture for intensity.
            canvas.StrokeColor = Color.FromArgb("#9E3F58");
            canvas.StrokeSize = 4;
            for (var offset = 16f; offset < scaledWidth - 12f; offset += 16f)
            {
                canvas.DrawLine(x + offset, y + 12, x + offset - 10, y + scaledHeight - 10);
            }
        }
    }

    /// <summary>
    /// Converts blink phase into a normalized eyelid coverage factor.
    /// </summary>
    /// <returns>A value in the range [0, 1] that indicates blink closure.</returns>
    private float GetBlinkValue()
    {
        // Step 1: No blink active.
        if (_blink <= 0)
        {
            return 0f;
        }

        // Step 2: Closing phase (0 to 1).
        if (_blink < 1)
        {
            return (float)_blink;
        }

        // Step 3: Opening phase (1 to 2).
        return Math.Max(0f, (float)(2 - _blink));
    }
}
