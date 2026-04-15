using SkiaSharp;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Draws centred floating text with alpha fade.</summary>
internal sealed class FloatTextSprite : Sprite
{
    private readonly TextSprite _text = new() { Size = 16f, Align = TextAlign.Center };

    public string Text { get => _text.Text; set => _text.Text = value; }
    public SKColor Color { get => _text.Color; set => _text.Color = value; }
    public float Life { get; set; }
    public float MaxLife { get; set; } = 1.4f;

    public override void Draw(SKCanvas canvas)
    {
        if (!Visible || string.IsNullOrEmpty(Text)) return;
        _text.Alpha = Math.Clamp(Life / MaxLife, 0f, 1f);
        _text.Draw(canvas);
    }
}
