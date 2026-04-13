using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.BlazorApp.Games.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.BlazorApp.Games.CastleAttack;

/// <summary>Start/title screen for Castle Attack. Shows the background and instructions.</summary>
internal sealed class CastleAttackStartScreen : GameScreenBase
{
    public override (int width, int height) GameDimensions => (GameWidth, GameHeight);

    public override void Update(float deltaTime) { }

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        float scale   = MathF.Min(width / (float)GameWidth, height / (float)GameHeight);
        float offsetX = (width  - GameWidth  * scale) / 2f;
        float offsetY = (height - GameHeight * scale) / 2f;

        canvas.Clear(ColSky);
        canvas.Save();
        canvas.Translate(offsetX, offsetY);
        canvas.Scale(scale, scale);

        DrawBackground(canvas);

        DrawHelper.DrawOverlay(canvas, GameWidth, GameHeight, 0.6f);
        DrawHelper.DrawCenteredText(canvas, "CASTLE ATTACK", 68f, ColGold, GameWidth / 2f, 190f);
        DrawHelper.DrawCenteredText(canvas, "Defend the castle until the keep is complete!", 22f, ColHud, GameWidth / 2f, 258f);

        float y = 308f;
        DrawHelper.DrawCenteredText(canvas, "Tap the battlefield to aim & fire", 17f, ColAccent, GameWidth / 2f, y);
        y += 26f;
        DrawHelper.DrawCenteredText(canvas, "Use the on-screen buttons at the bottom for all actions", 16f, ColDim, GameWidth / 2f, y);
        y += 26f;
        DrawHelper.DrawCenteredText(canvas, "Keyboard: ← → aim  |  SPACE fire  |  ↑↓ convert  |  Z X C weapons", 14f, ColDim, GameWidth / 2f, y);

        DrawHelper.DrawCenteredText(canvas, "Tap or Click to Start", 24f, ColAccent, GameWidth / 2f, 420f);

        canvas.Restore();
    }

    public override void OnPointerDown(float x, float y)
        => Coordinator?.TransitionTo<CastleAttackPlayScreen>(new DissolveTransition());

    private static void DrawBackground(SKCanvas canvas)
    {
        using var skyShader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(0, GroundY),
            [ColSky, ColHorizon], [0f, 1f], SKShaderTileMode.Clamp);
        using var skyPaint = new SKPaint { Shader = skyShader };
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GroundY), skyPaint);

        using var gp = new SKPaint { Color = ColGround };
        canvas.DrawRect(SKRect.Create(0, GroundY, GameWidth, GameHeight - GroundY), gp);
        using var gep = new SKPaint { Color = ColGroundEdge };
        canvas.DrawRect(SKRect.Create(0, GroundY, GameWidth, 4f), gep);

        using var hillPaint = new SKPaint { Color = new SKColor(0x2A, 0x20, 0x12), IsAntialias = true };
        using var hillPath  = new SKPath();
        hillPath.MoveTo(0, GroundY);
        hillPath.CubicTo(200, GroundY - 60, 400, GroundY - 80, 600, GroundY - 40);
        hillPath.CubicTo(800, GroundY,      900, GroundY - 50, 1100, GroundY - 30);
        hillPath.LineTo(GameWidth, GroundY);
        hillPath.LineTo(0, GroundY);
        hillPath.Close();
        canvas.DrawPath(hillPath, hillPaint);
    }
}
