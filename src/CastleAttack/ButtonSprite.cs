using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Draws a touch button with label text.</summary>
internal sealed class ButtonSprite : Sprite
{
    public SKRect Rect;
    public string Label = "";
    public bool Enabled;
    public SKColor LabelColor;
    public bool Pressed;
    public bool Large;

    private static readonly SKPaint BgPaint = new() { IsAntialias = true };
    private static readonly SKPaint BorderPaint = new() { Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, IsAntialias = true };

    /// <summary>
    /// Draws the button. The <paramref name="x"/> and <paramref name="y"/> parameters are ignored;
    /// the button position comes from <see cref="Rect"/>.
    /// </summary>
    public override void Draw(SKCanvas canvas, float x, float y)
    {
        if (!Visible) return;

        float alpha = Enabled ? 1f : 0.4f;
        byte bgA = Pressed ? (byte)180 : (byte)110;
        SKColor bg = Pressed ? new SKColor(0xFF, 0xFF, 0xFF, bgA)
                               : new SKColor(0x22, 0x22, 0x33, bgA);
        BgPaint.Color = bg;
        canvas.DrawRoundRect(Rect, BtnR, BtnR, BgPaint);

        SKColor border = Enabled
            ? (Pressed ? SKColors.White : new SKColor(0x88, 0x88, 0xAA))
            : new SKColor(0x44, 0x44, 0x55);
        BorderPaint.Color = border.WithAlpha((byte)(200 * alpha));
        canvas.DrawRoundRect(Rect, BtnR, BtnR, BorderPaint);

        float fontSize = Large ? 16f : 13f;
        SKColor col = Pressed ? SKColors.Black : LabelColor;
        TextRenderer.DrawCenteredText(canvas, Label, fontSize, col, Rect.MidX, Rect.MidY + fontSize * 0.38f, alpha);
    }
}
