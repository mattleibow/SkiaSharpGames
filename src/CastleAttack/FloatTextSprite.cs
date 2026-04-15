using SkiaSharp;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Draws centred floating text with alpha fade.</summary>
internal sealed class FloatTextSprite : Sprite
{
    public string Text = "";
    public SKColor Color = SKColors.White;
    public float Life;
    public float MaxLife = 1.4f;

    public override void Draw(SKCanvas canvas, float x, float y)
    {
        if (!Visible) return;
        float alpha = Math.Clamp(Life / MaxLife, 0f, 1f);
        TextRenderer.DrawCenteredText(canvas, Text, 16f, Color, x, y, alpha);
    }
}
