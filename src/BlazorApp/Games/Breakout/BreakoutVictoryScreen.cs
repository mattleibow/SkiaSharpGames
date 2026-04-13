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

        DrawHelper.DrawOverlay(canvas, GameWidth, GameHeight);
        DrawHelper.DrawCenteredText(canvas, "YOU WIN!", 64f, new SKColor(0xFF, 0xD6, 0x0A), GameWidth / 2f, 270f);
        DrawHelper.DrawCenteredText(canvas, $"Final Score: {_state.Score}", 32f, SKColors.White, GameWidth / 2f, 335f);
        DrawHelper.DrawCenteredText(canvas, "Click or Tap to Play Again", 24f, AccentColor, GameWidth / 2f, 395f);

        canvas.Restore();
    }

    public override void OnPointerDown(float x, float y)
        => Coordinator?.TransitionTo<BreakoutStartScreen>(new DissolveTransition());
}
