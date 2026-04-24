using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Snake.SnakeConstants;

namespace SkiaSharpGames.Snake;

internal sealed class StartScreen : Scene
{
    private readonly IDirector director;

    private readonly HudLabel _title = new()
    {
        Text = "SNAKE",
        FontSize = 78f,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 225f,
    };
    private readonly HudLabel _subtitle = new()
    {
        Text = "Eat, grow, survive",
        FontSize = 30f,
        Color = AccentColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 280f,
    };
    private readonly HudLabel _instruction1 = new()
    {
        Text = "Arrow keys or WASD to steer",
        FontSize = 22f,
        Color = DimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 345f,
    };
    private readonly HudLabel _instruction2 = new()
    {
        Text = "Eat the red food to grow",
        FontSize = 22f,
        Color = DimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 377f,
    };
    private readonly HudLabel _instruction3 = new()
    {
        Text = "Don't hit the walls or yourself",
        FontSize = 22f,
        Color = DimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 409f,
    };
    private readonly HudLabel _startPrompt = new()
    {
        Text = "Click, tap, or press Space to start",
        FontSize = 24f,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 474f,
    };

    // Decorative snake preview
    private static readonly SKPaint _previewPaint = new() { IsAntialias = true };

    public StartScreen(IDirector director)
    {
        this.director = director;

        Children.Add(_title);
        Children.Add(_subtitle);
        Children.Add(_instruction1);
        Children.Add(_instruction2);
        Children.Add(_instruction3);
        Children.Add(_startPrompt);
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(BackgroundColor);

        // Draw a small decorative snake icon
        float cx = GameWidth / 2f;
        _previewPaint.Color = SnakeHeadColor;
        canvas.DrawRoundRect(cx - 12f, 140f, CellSize - 2, CellSize - 2, 4f, 4f, _previewPaint);
        _previewPaint.Color = SnakeBodyColor;
        for (int i = 1; i <= 4; i++)
            canvas.DrawRoundRect(
                cx - 12f + i * CellSize,
                140f,
                CellSize - 2,
                CellSize - 2,
                4f,
                4f,
                _previewPaint
            );
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<PlayScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<PlayScreen>(new DissolveCurtain());
    }
}
