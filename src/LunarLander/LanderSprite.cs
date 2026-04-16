using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.LunarLander.LunarLanderConstants;

namespace SkiaSharpGames.LunarLander;

/// <summary>Draws a simple triangular lander hull centred at origin.</summary>
internal sealed class LanderSprite : Sprite
{
    private readonly SKPaint _bodyPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _legPaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
    private readonly SKPaint _windowPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };

    public SKColor Color { get; set; } = LanderColor;

    public override void Draw(SKCanvas canvas)
    {
        if (!Visible || Alpha <= 0f)
            return;

        byte alpha = (byte)(255 * Alpha);
        float halfW = LanderWidth / 2f;
        float halfH = LanderHeight / 2f;

        // Body — a trapezoid/triangle shape pointing up
        _bodyPaint.Color = Color.WithAlpha(alpha);
        using var bodyPath = new SKPath();
        bodyPath.MoveTo(0, -halfH);                 // top (nose)
        bodyPath.LineTo(halfW, halfH);               // bottom-right
        bodyPath.LineTo(-halfW, halfH);              // bottom-left
        bodyPath.Close();
        canvas.DrawPath(bodyPath, _bodyPaint);

        // Window — small circle near the top
        _windowPaint.Color = AccentColor.WithAlpha(alpha);
        canvas.DrawCircle(0, -halfH + 8f, 4f, _windowPaint);

        // Landing legs
        _legPaint.Color = Color.WithAlpha(alpha);
        float legExtend = 6f;
        canvas.DrawLine(-halfW, halfH, -halfW - 4f, halfH + legExtend, _legPaint);
        canvas.DrawLine(halfW, halfH, halfW + 4f, halfH + legExtend, _legPaint);
        // Horizontal foot pads
        canvas.DrawLine(-halfW - 6f, halfH + legExtend, -halfW - 2f, halfH + legExtend, _legPaint);
        canvas.DrawLine(halfW + 2f, halfH + legExtend, halfW + 6f, halfH + legExtend, _legPaint);
    }
}
