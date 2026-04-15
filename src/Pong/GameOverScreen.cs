using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Pong.PongConstants;

namespace SkiaSharpGames.Pong;

internal sealed class GameOverScreen(PongGameState state, IScreenCoordinator coordinator) : GameScreen
{
    private static readonly SKPaint _overlayPaint = new() { IsAntialias = true };

    private readonly TextSprite _winnerText = new() { Size = 58f, Color = AccentColor, Align = TextAlign.Center };
    private readonly TextSprite _scoreText = new() { Size = 42f, Align = TextAlign.Center };
    private readonly TextSprite _restartText = new() { Text = "Click, tap, or press Space to play again", Size = 24f, Color = DimColor, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        _overlayPaint.Color = SKColors.Black.WithAlpha((byte)(255 * 0.76f));
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), _overlayPaint);

        _winnerText.Text = state.WinnerText;
        _winnerText.Draw(canvas, GameWidth / 2f, 260f);

        _scoreText.Text = $"{state.LeftScore} : {state.RightScore}";
        _scoreText.Draw(canvas, GameWidth / 2f, 325f);

        _restartText.Draw(canvas, GameWidth / 2f, 395f);
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<StartScreen>(new DissolveTransition());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<StartScreen>(new DissolveTransition());
    }
}
