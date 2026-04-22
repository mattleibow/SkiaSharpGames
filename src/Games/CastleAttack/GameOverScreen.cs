using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>
/// Defeat overlay drawn on top of the frozen play screen.
/// Does not clear the canvas — relies on the base play screen being drawn first.
/// </summary>
internal sealed class GameOverScreen(CastleAttackGameState state, IDirector coordinator) : Scene
{
    private static readonly SKPaint OverlayPaint = new();

    private readonly UiLabel _defeatText = new() { Text = "DEFEAT", FontSize = 72f, Color = ColRed, Align = TextAlign.Center };
    private readonly UiLabel _scoreText = new() { FontSize = 32f, Color = ColHud, Align = TextAlign.Center };
    private readonly UiLabel _retryText = new() { Text = "Click or Tap to Try Again", FontSize = 22f, Color = ColAccent, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        OverlayPaint.Color = SKColors.Black.WithAlpha((byte)(255 * 0.75f));
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), OverlayPaint);

        float cx = GameWidth / 2f;
        canvas.Save(); canvas.Translate(cx, 250f); _defeatText.Draw(canvas); canvas.Restore();
        _scoreText.Text = $"Score: {state.Score}";
        canvas.Save(); canvas.Translate(cx, 315f); _scoreText.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(cx, 370f); _retryText.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y)
        => coordinator.TransitionTo<StartScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            coordinator.TransitionTo<StartScreen>(new DissolveCurtain());
    }
}
