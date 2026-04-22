using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Snake.SnakeConstants;

namespace SkiaSharpGames.Snake;

internal sealed class GameOverScreen(SnakeGameState state, IDirector coordinator) : Scene
{
    private static readonly SKPaint _overlayPaint = new() { Color = SKColors.Black.WithAlpha((byte)(255 * 0.8f)) };

    private readonly UiLabel _titleText = new() { Text = "GAME OVER", FontSize = 72f, Color = DangerColor, Align = TextAlign.Center };
    private readonly UiLabel _scoreText = new() { FontSize = 34f, Align = TextAlign.Center };
    private readonly UiLabel _highScoreText = new() { FontSize = 24f, Color = DimColor, Align = TextAlign.Center };
    private readonly UiLabel _promptText = new() { Text = "Click, tap, or press Space to try again", FontSize = 24f, Color = AccentColor, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.DrawRect(0, 0, GameWidth, GameHeight, _overlayPaint);

        float cx = GameWidth / 2f;

        canvas.Save(); canvas.Translate(cx, 220f); _titleText.Draw(canvas); canvas.Restore();

        _scoreText.Text = $"Score: {state.Score}";
        canvas.Save(); canvas.Translate(cx, 300f); _scoreText.Draw(canvas); canvas.Restore();

        if (state.HighScore > 0)
        {
            _highScoreText.Text = $"Best: {state.HighScore}";
            canvas.Save(); canvas.Translate(cx, 345f); _highScoreText.Draw(canvas); canvas.Restore();
        }

        canvas.Save(); canvas.Translate(cx, 420f); _promptText.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<StartScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<StartScreen>(new DissolveCurtain());
    }
}
