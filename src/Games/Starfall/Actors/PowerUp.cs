using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

internal enum PowerUpType { Health, RapidFire, SpreadShot, Bomb, Shield }

/// <summary>
/// Collectible power-up dropped by destroyed enemies.
/// </summary>
internal sealed class PowerUp : Actor
{
    private static readonly Dictionary<PowerUpType, SKColor> _colors = new()
    {
        [PowerUpType.Health] = PowerUpHealthColor,
        [PowerUpType.RapidFire] = PowerUpRapidColor,
        [PowerUpType.SpreadShot] = PowerUpSpreadColor,
        [PowerUpType.Bomb] = PowerUpBombColor,
        [PowerUpType.Shield] = PowerUpShieldColor,
    };

    private static readonly Dictionary<PowerUpType, string> _symbols = new()
    {
        [PowerUpType.Health] = "+",
        [PowerUpType.RapidFire] = "R",
        [PowerUpType.SpreadShot] = "S",
        [PowerUpType.Bomb] = "B",
        [PowerUpType.Shield] = "◆",
    };

    public PowerUpType Type { get; }
    private float _time;

    /// <summary>Player position for magnetism effect.</summary>
    public float PlayerX { get; set; }
    public float PlayerY { get; set; }

    public PowerUp(float x, float y, PowerUpType type)
    {
        X = x;
        Y = y;
        Type = type;
        Collider = new CircleCollider(PowerUpRadius);
        Rigidbody = new Rigidbody2D();
        Rigidbody.SetVelocity(0, PowerUpSpeed);
    }

    protected override void OnUpdate(float deltaTime)
    {
        _time += deltaTime;

        // Magnetism: drift toward player when within range
        float dx = PlayerX - X;
        float dy = PlayerY - Y;
        float dist = MathF.Sqrt(dx * dx + dy * dy);
        if (dist < 100f && dist > 1f)
        {
            float magnetStrength = (100f - dist) / 100f * 300f;
            X += dx / dist * magnetStrength * deltaTime;
            Y += dy / dist * magnetStrength * deltaTime;
        }

        if (Y > GameHeight + PowerUpRadius * 2)
            Active = false;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        var color = _colors[Type];
        float pulse = 0.7f + 0.3f * MathF.Sin(_time * 6f);

        // Outer glow
        using var glowPaint = new SKPaint
        {
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4f),
            Color = color.WithAlpha((byte)(80 * pulse)),
        };
        canvas.DrawCircle(0, 0, PowerUpRadius + 4f, glowPaint);

        // Body
        using var paint = new SKPaint
        {
            Color = color.WithAlpha((byte)(220 * pulse)),
            IsAntialias = true,
        };
        canvas.DrawCircle(0, 0, PowerUpRadius, paint);

        // Border
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 1.5f;
        paint.Color = SKColors.White.WithAlpha(200);
        canvas.DrawCircle(0, 0, PowerUpRadius, paint);

        // Symbol
        paint.Style = SKPaintStyle.Fill;
        paint.Color = SKColors.White;
        var font = new SKFont { Size = 14f };
        string symbol = _symbols[Type];
        float textWidth = font.MeasureText(symbol);
        canvas.DrawText(symbol, -textWidth / 2f, 5f, font, paint);
    }
}
