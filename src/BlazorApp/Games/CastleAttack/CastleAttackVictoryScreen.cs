using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.BlazorApp.Games.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.BlazorApp.Games.CastleAttack;

/// <summary>
/// Victory overlay drawn on top of the frozen play screen.
/// Does not clear the canvas — relies on the base play screen being drawn first.
/// </summary>
internal sealed class CastleAttackVictoryScreen : GameScreenBase
{
    private readonly CastleAttackGameState _state;

    public CastleAttackVictoryScreen(CastleAttackGameState state) => _state = state;

    public override (int width, int height) GameDimensions => (GameWidth, GameHeight);

    public override void Update(float deltaTime) { }

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        float scale   = MathF.Min(width / (float)GameWidth, height / (float)GameHeight);
        float offsetX = (width  - GameWidth  * scale) / 2f;
        float offsetY = (height - GameHeight * scale) / 2f;

        canvas.Save();
        canvas.Translate(offsetX, offsetY);
        canvas.Scale(scale, scale);

        DrawHelper.DrawOverlay(canvas, GameWidth, GameHeight, 0.75f);
        DrawHelper.DrawCenteredText(canvas, "VICTORY!", 72f, ColGold, GameWidth / 2f, 250f);
        DrawHelper.DrawCenteredText(canvas, $"Score: {_state.Score}", 32f, ColHud, GameWidth / 2f, 315f);
        DrawHelper.DrawCenteredText(canvas, "The keep is complete!", 22f, ColAccent, GameWidth / 2f, 360f);
        DrawHelper.DrawCenteredText(canvas, "Click or Tap to Play Again", 22f, ColDim, GameWidth / 2f, 395f);

        canvas.Restore();
    }

    public override void OnPointerDown(float x, float y)
        => Game?.TransitionTo<CastleAttackStartScreen>(new DissolveTransition());
}
