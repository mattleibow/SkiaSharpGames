using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class VictoryScreen(SpaceInvadersGameState state, IScreenCoordinator coordinator) : GameScreen
{
    private static readonly SKPaint _overlayPaint = new() { Color = new SKColor(0x00, 0x1A, 0x10).WithAlpha((byte)(255 * 0.8f)), IsAntialias = true };

    private readonly TextSprite _titleText = new() { Text = "WAVE CLEARED", Size = 72f, Color = AccentColor, Align = TextAlign.Center };
    private readonly TextSprite _scoreText = new() { Size = 32f, Color = SKColors.White, Align = TextAlign.Center };
    private readonly TextSprite _restartText = new() { Text = "Click, tap, or press Space to defend again", Size = 24f, Color = HudDimColor, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), _overlayPaint);

        _titleText.Draw(canvas, GameWidth / 2f, 250f);

        _scoreText.Text = $"Score: {state.Score}";
        _scoreText.Draw(canvas, GameWidth / 2f, 320f);

        _restartText.Draw(canvas, GameWidth / 2f, 410f);
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<StartScreen>(new DissolveTransition());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<StartScreen>(new DissolveTransition());
    }
}
