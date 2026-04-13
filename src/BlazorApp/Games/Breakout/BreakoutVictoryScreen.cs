using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.BlazorApp.Games.Breakout.BreakoutConstants;

namespace SkiaSharpGames.BlazorApp.Games.Breakout;

/// <summary>
/// Victory overlay drawn on top of the frozen play screen.
/// Does not clear the canvas — relies on the base play screen being drawn first.
/// </summary>
internal sealed class BreakoutVictoryScreen : GameScreenBase
{
    private readonly BreakoutGameState _state;

    public BreakoutVictoryScreen(BreakoutGameState state) => _state = state;

    public override void Update(float deltaTime) { }

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        DrawHelper.DrawOverlay(canvas, GameWidth, GameHeight);
        DrawHelper.DrawCenteredText(canvas, "YOU WIN!", 64f, new SKColor(0xFF, 0xD6, 0x0A), GameWidth / 2f, 270f);
        DrawHelper.DrawCenteredText(canvas, $"Final Score: {_state.Score}", 32f, SKColors.White, GameWidth / 2f, 335f);
        DrawHelper.DrawCenteredText(canvas, "Click or Tap to Play Again", 24f, AccentColor, GameWidth / 2f, 395f);
    }

    public override void OnPointerDown(float x, float y)
        => Game?.TransitionTo<BreakoutStartScreen>(new DissolveTransition());
}
