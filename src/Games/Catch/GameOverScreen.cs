using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Catch.CatchConstants;

namespace SkiaSharpGames.Catch;

internal sealed class GameOverScreen(CatchGameState state, IDirector coordinator) : Scene
{
    private static readonly SKPaint _overlayPaint = new() { Color = SKColors.Black.WithAlpha((byte)(255 * 0.8f)) };

    private readonly UiLabel _titleText = new() { Text = "GAME OVER", FontSize = 72f, Color = DangerColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 250f };
    private readonly UiLabel _scoreText = new() { FontSize = 34f, Align = TextAlign.Center, X = GameWidth / 2f, Y = 325f };
    private readonly UiLabel _promptText = new() { Text = "Click, tap, or press Space to try again", FontSize = 24f, Color = AccentColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 395f };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.DrawRect(0, 0, GameWidth, GameHeight, _overlayPaint);

        _titleText.Draw(canvas);

        _scoreText.Text = $"Score: {state.Score}";
        _scoreText.Draw(canvas);

        _promptText.Draw(canvas);
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<StartScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<StartScreen>(new DissolveCurtain());
    }
}
