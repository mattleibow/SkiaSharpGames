using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Appearance for <see cref="UiPointer"/>. Handles Crosshair, Dot, and Ring
/// cursor styles based on the entity's <see cref="UiPointer.Style"/> property.
/// </summary>
public record UiPointerAppearance : UiAppearance<UiPointer>
{
    private static readonly SKPaint StrokeDark = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 2.5f,
        Color = new SKColor(0, 0, 0, 180)
    };

    private static readonly SKPaint StrokeLight = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1.5f,
        Color = SKColors.White
    };

    private static readonly SKPaint FillLight = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
        Color = SKColors.White
    };

    private static readonly SKPaint FillDark = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
        Color = new SKColor(0, 0, 0, 180)
    };

    public static UiPointerAppearance Default => new();

    /// <inheritdoc />
    public override void Draw(SKCanvas canvas, UiPointer pointer)
    {
        switch (pointer.Style)
        {
            case UiPointerStyle.Crosshair:
                DrawCrosshair(canvas, pointer.IsDown);
                break;
            case UiPointerStyle.Dot:
                DrawDot(canvas, pointer.IsDown);
                break;
            case UiPointerStyle.Ring:
                DrawRing(canvas, pointer.IsDown);
                break;
        }
    }

    private static void DrawCrosshair(SKCanvas canvas, bool isDown)
    {
        float size = isDown ? 6f : 8f;
        float gap = isDown ? 1.5f : 2.5f;

        canvas.DrawLine(-size, 0, -gap, 0, StrokeDark);
        canvas.DrawLine(gap, 0, size, 0, StrokeDark);
        canvas.DrawLine(0, -size, 0, -gap, StrokeDark);
        canvas.DrawLine(0, gap, 0, size, StrokeDark);

        canvas.DrawLine(-size, 0, -gap, 0, StrokeLight);
        canvas.DrawLine(gap, 0, size, 0, StrokeLight);
        canvas.DrawLine(0, -size, 0, -gap, StrokeLight);
        canvas.DrawLine(0, gap, 0, size, StrokeLight);
    }

    private static void DrawDot(SKCanvas canvas, bool isDown)
    {
        float r = isDown ? 2f : 3f;
        canvas.DrawCircle(0, 0, r + 1f, FillDark);
        canvas.DrawCircle(0, 0, r, FillLight);
    }

    private static void DrawRing(SKCanvas canvas, bool isDown)
    {
        float r = isDown ? 4f : 6f;
        canvas.DrawCircle(0, 0, r, StrokeDark);
        canvas.DrawCircle(0, 0, r, StrokeLight);
    }
}
