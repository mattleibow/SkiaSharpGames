using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.GhostLight.GhostLightConstants;

namespace SkiaSharpGames.GhostLight;

/// <summary>A fog container with animated alpha that breathes over time.</summary>
internal sealed class FogLayer : Actor
{
    private readonly SKPaint _paint = new() { IsAntialias = true };
    private float _breathTimer;
    private readonly float _breathPhase;

    public float BaseAlpha { get; set; }
    public SKColor FogColor { get; set; } = new SKColor(0x15, 0x10, 0x25);

    public FogLayer(float baseAlpha, float breathPhase = 0f)
    {
        BaseAlpha = baseAlpha;
        _breathPhase = breathPhase;
        Alpha = baseAlpha;
    }

    protected override void OnUpdate(float deltaTime)
    {
        _breathTimer += deltaTime;
        float breath = MathF.Sin((_breathTimer + _breathPhase) * MathF.PI * 2f / FogBreathPeriod);
        Alpha = Math.Clamp(BaseAlpha + breath * FogBreathAmount, 0.05f, 0.95f);
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        _paint.Color = FogColor;
        canvas.DrawRect(0, 0, GameWidth, GameHeight, _paint);
    }
}
