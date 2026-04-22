using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>
/// Victory scene drawn on top of the frozen play scene.
/// Does not clear the canvas — relies on the base play scene being drawn first.
/// </summary>
internal sealed class VictoryScreen(CastleAttackGameState state, IDirector director) : Scene
{
    private static readonly SKPaint OverlayPaint = new();

    private readonly HudLabel _victoryText = new() { Text = "VICTORY!", FontSize = 72f, Color = ColGold, Align = TextAlign.Center };
    private readonly HudLabel _scoreText = new() { FontSize = 32f, Color = ColHud, Align = TextAlign.Center };
    private readonly HudLabel _keepText = new() { Text = "The keep is complete!", FontSize = 22f, Color = ColAccent, Align = TextAlign.Center };
    private readonly HudLabel _playAgainText = new() { Text = "Click or Tap to Play Again", FontSize = 22f, Color = ColDim, Align = TextAlign.Center };

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        OverlayPaint.Color = SKColors.Black.WithAlpha((byte)(255 * 0.75f));
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GameHeight), OverlayPaint);

        float cx = GameWidth / 2f;
        canvas.Save(); canvas.Translate(cx, 250f); _victoryText.Draw(canvas); canvas.Restore();
        _scoreText.Text = $"Score: {state.Score}";
        canvas.Save(); canvas.Translate(cx, 315f); _scoreText.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(cx, 360f); _keepText.Draw(canvas); canvas.Restore();
        canvas.Save(); canvas.Translate(cx, 395f); _playAgainText.Draw(canvas); canvas.Restore();
    }

    public override void OnPointerDown(float x, float y)
        => director.TransitionTo<StartScreen>(new DissolveCurtain());

    public override void OnKeyDown(string key)
    {
        if (key is " " or "Enter")
            director.TransitionTo<StartScreen>(new DissolveCurtain());
    }
}
