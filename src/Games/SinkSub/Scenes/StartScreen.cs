using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class StartScreen(IDirector director) : Scene
{
    private static readonly SKPaint _fillPaint = new() { IsAntialias = true };

    private readonly HudLabel _title = new()
    {
        Text = "SINK SUB",
        FontSize = 72f,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 215f,
    };
    private readonly HudLabel _subtitle = new()
    {
        Text = "Hunt submarines and avoid rising mines",
        FontSize = 26f,
        Color = AccentColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 270f,
    };
    private readonly HudLabel _instruction1 = new()
    {
        Text = "Move with mouse, touch, or arrow keys",
        FontSize = 20f,
        Color = DimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 345f,
    };
    private readonly HudLabel _instruction2 = new()
    {
        Text = "Press Z or X to drop depth charges from the ship sides",
        FontSize = 20f,
        Color = DimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 375f,
    };
    private readonly HudLabel _instruction3 = new()
    {
        Text = "Only four depth charges may be active at once",
        FontSize = 20f,
        Color = DimColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 405f,
    };
    private readonly HudLabel _startPrompt = new()
    {
        Text = "Click, tap, or press Space to begin",
        FontSize = 24f,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 465f,
    };

    public override void OnActivating()
    {
        if (ChildCount == 0)
        {
            Children.Add(_title);
            Children.Add(_subtitle);
            Children.Add(_instruction1);
            Children.Add(_instruction2);
            Children.Add(_instruction3);
            Children.Add(_startPrompt);
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(SkyColor);

        _fillPaint.Color = WaterColor;
        canvas.DrawRect(
            SKRect.Create(0f, WaterlineY, GameWidth, GameHeight - WaterlineY),
            _fillPaint
        );
        _fillPaint.Color = DeepWaterColor.WithAlpha((byte)(255 * 0.6f));
        canvas.DrawRect(
            SKRect.Create(0f, WaterlineY + 180f, GameWidth, GameHeight - WaterlineY - 180f),
            _fillPaint
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
