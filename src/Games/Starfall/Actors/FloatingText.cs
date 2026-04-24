using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Floating score/combo text that drifts upward and fades.
/// </summary>
internal sealed class FloatingText : Actor
{
    private readonly string _text;
    private readonly SKColor _color;
    private readonly float _lifetime;
    private float _remaining;

    public FloatingText(float x, float y, string text, SKColor color, float lifetime = 0.8f)
    {
        X = x;
        Y = y;
        _text = text;
        _color = color;
        _lifetime = lifetime;
        _remaining = lifetime;
    }

    protected override void OnUpdate(float deltaTime)
    {
        _remaining -= deltaTime;
        if (_remaining <= 0f)
        {
            Active = false;
            return;
        }
        Y -= StarfallConstants.FloatingTextSpeed * deltaTime;
        Alpha = _remaining / _lifetime;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        using var paint = new SKPaint
        {
            Color = _color,
            IsAntialias = true,
        };
        var font = new SKFont { Size = 18f };
        float w = font.MeasureText(_text);
        canvas.DrawText(_text, -w / 2f, 0, font, paint);
    }
}
