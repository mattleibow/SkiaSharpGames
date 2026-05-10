using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.LunarLander.LunarLanderConstants;

namespace SkiaSharpGames.LunarLander;

/// <summary>Title scene: game title, instructions, click to start.</summary>
internal sealed class StartScreen(IDirector director) : Scene
{
    private readonly HudLabel _title = new()
    {
        Text = "LUNAR LANDER",
        FontSize = 64f,
        Color = SKColors.White,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 300f,
    };
    private readonly HudLabel _subtitle = new()
    {
        Text = "Actor Rotation & Child Entities Demo",
        FontSize = 18f,
        Color = AccentColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 335f,
    };
    private readonly HudLabel _startPrompt = new()
    {
        Text = "Click or Tap to Start",
        FontSize = 28f,
        Color = AccentColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 400f,
    };
    private readonly HudLabel _instructions1 = new()
    {
        Text = "LEFT RIGHT rotate    UP / SPACE thrust",
        FontSize = 18f,
        Color = DimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 450f,
    };
    private readonly HudLabel _instructions2 = new()
    {
        Text = "Land gently on the green pad",
        FontSize = 18f,
        Color = DimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 478f,
    };

    private readonly SKPoint[] _stars = new SKPoint[StarCount];
    private readonly float[] _starBrightness = new float[StarCount];
    private readonly Random _rng = new(42);

    public override void OnActivating()
    {
        for (int i = 0; i < StarCount; i++)
        {
            _stars[i] = new SKPoint(_rng.Next(0, GameWidth), _rng.Next(0, GameHeight));
            _starBrightness[i] = 0.3f + (float)_rng.NextDouble() * 0.7f;
        }

        if (ChildCount == 0)
        {
            Children.Add(_title);
            Children.Add(_subtitle);
            Children.Add(_startPrompt);
            Children.Add(_instructions1);
            Children.Add(_instructions2);
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(BackgroundColor);

        // Stars
        using var starPaint = new SKPaint { IsAntialias = true };
        for (int i = 0; i < StarCount; i++)
        {
            byte a = (byte)(255 * _starBrightness[i]);
            starPaint.Color = SKColors.White.WithAlpha(a);
            canvas.DrawCircle(_stars[i].X, _stars[i].Y, 1.2f, starPaint);
        }

        // Simple lander illustration
        DrawLanderIcon(canvas, GameWidth / 2f, 200f);
    }

    private static void DrawLanderIcon(SKCanvas canvas, float cx, float cy)
    {
        using var paint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = LanderColor,
        };
        using var path = new SKPath();
        float s = 2.5f; // scale
        path.MoveTo(cx, cy - 12f * s);
        path.LineTo(cx + 15f * s, cy + 12f * s);
        path.LineTo(cx - 15f * s, cy + 12f * s);
        path.Close();
        canvas.DrawPath(path, paint);
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<PlayScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<PlayScreen>(new DissolveCurtain());
    }
}
