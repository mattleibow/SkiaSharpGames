using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class StartScreen(IDirector coordinator) : Scene
{
    private readonly UiLabel _title = new() { Text = "SPACE INVADERS", FontSize = 72f, Color = AccentColor, Align = TextAlign.Center };
    private readonly UiLabel _subtitle = new() { Text = "Classic arcade invasion defense", FontSize = 26f, Color = SKColors.White, Align = TextAlign.Center };
    private readonly UiLabel _moveHint = new() { Text = "Move: Arrow keys or mouse / touch", FontSize = 22f, Color = HudDimColor, Align = TextAlign.Center };
    private readonly UiLabel _fireHint = new() { Text = "Fire: Space, Enter, or tap", FontSize = 22f, Color = HudDimColor, Align = TextAlign.Center };
    private readonly UiLabel _goal = new() { Text = "Stop the aliens before they land", FontSize = 24f, Color = SKColors.White, Align = TextAlign.Center };
    private readonly UiLabel _startPrompt = new() { Text = "Click, tap, or press Space to start", FontSize = 24f, Color = AccentColor, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        canvas.Save(); canvas.Translate(GameWidth / 2f, 215f); _title.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 265f); _subtitle.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 340f); _moveHint.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 372f); _fireHint.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 430f); _goal.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 485f); _startPrompt.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y) =>
        coordinator.TransitionTo<PlayScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<PlayScreen>(new DissolveCurtain());
    }
}
