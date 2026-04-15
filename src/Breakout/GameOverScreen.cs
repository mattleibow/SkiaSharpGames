using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

/// <summary>
/// Game-over overlay drawn on top of the frozen play screen.
/// Does not clear the canvas — relies on the base play screen being drawn first.
/// </summary>
internal sealed class GameOverScreen(BreakoutGameState state, IScreenCoordinator coordinator) : GameScreen
{
    private static readonly SKPaint _overlayPaint = new() { Color = SKColors.Black.WithAlpha(186) };

    private readonly TextSprite _titleText = new() { Text = "GAME OVER", Size = 64f, Color = new SKColor(0xFF, 0x2D, 0x55), Align = TextAlign.Center };
    private readonly TextSprite _scoreText = new() { Size = 32f, Color = SKColors.White, Align = TextAlign.Center };
    private readonly TextSprite _promptText = new() { Text = "Click or Tap to Play Again", Size = 24f, Color = AccentColor, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.DrawRect(0, 0, GameWidth, GameHeight, _overlayPaint);

        _titleText.Draw(canvas, GameWidth / 2f, 270f);
        _scoreText.Text = $"Score: {state.Score}";
        _scoreText.Draw(canvas, GameWidth / 2f, 335f);
        _promptText.Draw(canvas, GameWidth / 2f, 395f);
    }

    public override void OnPointerDown(float x, float y)
        => coordinator.TransitionTo<StartScreen>(new DissolveTransition());
}
