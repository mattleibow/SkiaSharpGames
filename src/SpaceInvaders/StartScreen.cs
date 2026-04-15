using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class StartScreen(IScreenCoordinator coordinator) : GameScreen
{
    private readonly TextSprite _title = new() { Text = "SPACE INVADERS", Size = 72f, Color = AccentColor, Align = TextAlign.Center };
    private readonly TextSprite _subtitle = new() { Text = "Classic arcade invasion defense", Size = 26f, Color = SKColors.White, Align = TextAlign.Center };
    private readonly TextSprite _moveHint = new() { Text = "Move: Arrow keys or mouse / touch", Size = 22f, Color = HudDimColor, Align = TextAlign.Center };
    private readonly TextSprite _fireHint = new() { Text = "Fire: Space, Enter, or tap", Size = 22f, Color = HudDimColor, Align = TextAlign.Center };
    private readonly TextSprite _goal = new() { Text = "Stop the aliens before they land", Size = 24f, Color = SKColors.White, Align = TextAlign.Center };
    private readonly TextSprite _startPrompt = new() { Text = "Click, tap, or press Space to start", Size = 24f, Color = AccentColor, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        _title.Draw(canvas, GameWidth / 2f, 215f);
        _subtitle.Draw(canvas, GameWidth / 2f, 265f);
        _moveHint.Draw(canvas, GameWidth / 2f, 340f);
        _fireHint.Draw(canvas, GameWidth / 2f, 372f);
        _goal.Draw(canvas, GameWidth / 2f, 430f);
        _startPrompt.Draw(canvas, GameWidth / 2f, 485f);
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<PlayScreen>(new DissolveTransition());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<PlayScreen>(new DissolveTransition());
    }
}
