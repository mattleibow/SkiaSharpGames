using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Pong.PongConstants;

namespace SkiaSharpGames.Pong;

internal sealed class StartScreen(IDirector director) : Scene
{
    private readonly HudLabel _title = new() { Text = "2 PLAYER PONG", FontSize = 70f, Align = TextAlign.Center };
    private readonly HudLabel _leftControls = new() { Text = "Left: W / S", FontSize = 26f, Color = LeftPaddleColor, Align = TextAlign.Center };
    private readonly HudLabel _rightControls = new() { Text = "Right: Up / Dn", FontSize = 26f, Color = RightPaddleColor, Align = TextAlign.Center };
    private readonly HudLabel _touchHint = new() { Text = "Touch or mouse: move the paddle on that side", FontSize = 20f, Color = DimColor, Align = TextAlign.Center };
    private readonly HudLabel _scoreHint = new() { Text = "First to 7 points wins", FontSize = 22f, Color = AccentColor, Align = TextAlign.Center };
    private readonly HudLabel _startPrompt = new() { Text = "Click, tap, or press Space to start", FontSize = 24f, Align = TextAlign.Center };

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(BackgroundColor);

        canvas.Save(); canvas.Translate(GameWidth / 2f, 235f); _title.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 305f); _leftControls.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 340f); _rightControls.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 392f); _touchHint.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 430f); _scoreHint.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 480f); _startPrompt.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y) =>
        director.TransitionTo<PlayScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<PlayScreen>(new DissolveCurtain());
    }
}
