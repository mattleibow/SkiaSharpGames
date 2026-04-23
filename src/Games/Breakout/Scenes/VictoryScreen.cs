using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.Breakout.BreakoutConstants;

namespace SkiaSharpGames.Breakout;

/// <summary>
/// Victory scene drawn on top of the frozen play scene.
/// Does not clear the canvas — relies on the base play scene being drawn first.
/// </summary>
internal sealed class VictoryScreen(BreakoutGameState state, IDirector director) : Scene
{
    private static readonly SKPaint _overlayPaint = new() { Color = SKColors.Black.WithAlpha(186) };

    private readonly HudLabel _titleText = new() { Text = "YOU WIN!", FontSize = 64f, Color = new SKColor(0xFF, 0xD6, 0x0A), Align = TextAlign.Center };
    private readonly HudLabel _scoreText = new() { FontSize = 32f, Color = SKColors.White, Align = TextAlign.Center };
    private readonly HudLabel _promptText = new() { Text = "Click or Tap to Play Again", FontSize = 24f, Color = AccentColor, Align = TextAlign.Center };

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.DrawRect(0, 0, GameWidth, GameHeight, _overlayPaint);

        canvas.Save(); canvas.Translate(GameWidth / 2f, 270f); _titleText.Draw(canvas); canvas.Restore();
        _scoreText.Text = $"Final Score: {state.Score}";
        canvas.Save(); canvas.Translate(GameWidth / 2f, 335f); _scoreText.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(GameWidth / 2f, 395f); _promptText.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y)
        => director.TransitionTo<StartScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<StartScreen>(new DissolveCurtain());
    }
}
