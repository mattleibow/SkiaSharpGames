using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Catch.CatchConstants;

namespace SkiaSharpGames.Catch;

internal sealed class GameOverScreen(CatchGameState state, IDirector director) : Scene
{
    private static readonly SKPaint _overlayPaint = new() { Color = SKColors.Black.WithAlpha((byte)(255 * 0.8f)) };

    private readonly HudLabel _titleText = new() { Text = "GAME OVER", FontSize = 72f, Color = DangerColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 250f };
    private readonly HudLabel _scoreText = new() { FontSize = 34f, Align = TextAlign.Center, X = GameWidth / 2f, Y = 325f };
    private readonly HudLabel _promptText = new() { Text = "Click, tap, or press Space to try again", FontSize = 24f, Color = AccentColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 395f };

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.DrawRect(0, 0, GameWidth, GameHeight, _overlayPaint);

        _titleText.Draw(canvas);

        _scoreText.Text = $"Score: {state.Score}";
        _scoreText.Draw(canvas);

        _promptText.Draw(canvas);
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<StartScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<StartScreen>(new DissolveCurtain());
    }
}
