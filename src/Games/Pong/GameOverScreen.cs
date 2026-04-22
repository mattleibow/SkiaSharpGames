using SkiaSharp;
using SkiaSharpGames.GameEngine;
using SkiaSharpGames.GameEngine.UI;
using static SkiaSharpGames.Pong.PongConstants;

namespace SkiaSharpGames.Pong;

internal sealed class GameOverScreen(PongGameState state, IDirector coordinator) : Scene
{
    private static readonly SKPaint _overlayPaint = new() { IsAntialias = true };

    private readonly UiLabel _winnerText = new() { FontSize = 58f, Color = AccentColor, Align = TextAlign.Center };
    private readonly UiLabel _scoreText = new() { FontSize = 42f, Align = TextAlign.Center };
    private readonly UiLabel _restartText = new() { Text = "Click, tap, or press Space to play again", FontSize = 24f, Color = DimColor, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        _overlayPaint.Color = SKColors.Black.WithAlpha((byte)(255 * 0.76f));
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), _overlayPaint);

        _winnerText.Text = state.WinnerText;
        canvas.Save(); canvas.Translate(GameWidth / 2f, 260f); _winnerText.Draw(canvas); canvas.Restore();

        _scoreText.Text = $"{state.LeftScore} : {state.RightScore}";
        canvas.Save(); canvas.Translate(GameWidth / 2f, 325f); _scoreText.Draw(canvas); canvas.Restore();

        canvas.Save(); canvas.Translate(GameWidth / 2f, 395f); _restartText.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<StartScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<StartScreen>(new DissolveCurtain());
    }
}
