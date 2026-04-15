using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.TwoZeroFourEight.TwoZeroFourEightConstants;

namespace SkiaSharpGames.TwoZeroFourEight;

internal sealed class StartScreen(IScreenCoordinator coordinator) : GameScreen
{
    private readonly TextSprite _title = new() { Text = "2048", Size = 92f, Color = HeaderColor, Align = TextAlign.Center };
    private readonly TextSprite _subtitle = new() { Text = "Slide tiles. Merge equal numbers. Reach 2048.", Size = 28f, Color = HeaderColor, Align = TextAlign.Center };
    private readonly TextSprite _controls = new() { Text = "Arrow keys or swipe", Size = 24f, Color = HeaderColor, Align = TextAlign.Center };
    private readonly TextSprite _startPrompt = new() { Text = "Click, tap, Enter, or Space to begin", Size = 24f, Color = HeaderColor, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        _title.Draw(canvas, GameWidth / 2f, 180f);
        _subtitle.Draw(canvas, GameWidth / 2f, 250f);
        _controls.Draw(canvas, GameWidth / 2f, 320f);
        _startPrompt.Draw(canvas, GameWidth / 2f, 360f);
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<PlayScreen>(new DissolveTransition());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<PlayScreen>(new DissolveTransition());
    }
}
