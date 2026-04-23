using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class StartScreen(IDirector director) : Scene
{
    private readonly HudLabel _title = new() { Text = "SPACE INVADERS", FontSize = 72f, Color = AccentColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 215f };
    private readonly HudLabel _subtitle = new() { Text = "Classic arcade invasion defense", FontSize = 26f, Color = SKColors.White, Align = TextAlign.Center, X = GameWidth / 2f, Y = 265f };
    private readonly HudLabel _moveHint = new() { Text = "Move: Arrow keys or mouse / touch", FontSize = 22f, Color = HudDimColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 340f };
    private readonly HudLabel _fireHint = new() { Text = "Fire: Space, Enter, or tap", FontSize = 22f, Color = HudDimColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 372f };
    private readonly HudLabel _goal = new() { Text = "Stop the aliens before they land", FontSize = 24f, Color = SKColors.White, Align = TextAlign.Center, X = GameWidth / 2f, Y = 430f };
    private readonly HudLabel _startPrompt = new() { Text = "Click, tap, or press Space to start", FontSize = 24f, Color = AccentColor, Align = TextAlign.Center, X = GameWidth / 2f, Y = 485f };

    public override void OnActivating()
    {
        if (ChildCount == 0)
        {
            Children.Add(_title);
            Children.Add(_subtitle);
            Children.Add(_moveHint);
            Children.Add(_fireHint);
            Children.Add(_goal);
            Children.Add(_startPrompt);
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(BackgroundColor);
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<PlayScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<PlayScreen>(new DissolveCurtain());
    }
}
