using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// The player's ship. Smoothly follows a target position (pointer).
/// Renders as a sleek triangular ship with engine glow.
/// </summary>
internal sealed class PlayerShip : Actor
{
    private static readonly SKPaint _shipPaint = new()
    {
        Color = PlayerColor,
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
    };
    private static readonly SKPaint _shipOutline = new()
    {
        Color = SKColors.White.WithAlpha(180),
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1f,
    };
    private static readonly SKPaint _enginePaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
    };

    private static readonly SKPath _shipPath;

    static PlayerShip()
    {
        _shipPath = new SKPath();
        _shipPath.MoveTo(0, -PlayerShipRadius);
        _shipPath.LineTo(-PlayerShipRadius * 0.8f, PlayerShipRadius * 0.8f);
        _shipPath.LineTo(-PlayerShipRadius * 0.3f, PlayerShipRadius * 0.4f);
        _shipPath.LineTo(0, PlayerShipRadius * 0.55f);
        _shipPath.LineTo(PlayerShipRadius * 0.3f, PlayerShipRadius * 0.4f);
        _shipPath.LineTo(PlayerShipRadius * 0.8f, PlayerShipRadius * 0.8f);
        _shipPath.Close();
    }

    public float TargetX { get; set; }
    public float TargetY { get; set; }
    public float InvincibleTimer { get; set; }
    public bool IsInvincible => InvincibleTimer > 0f;

    private float _engineFlicker;

    public PlayerShip()
    {
        Name = "player";
        Collider = new CircleCollider(PlayerShipRadius * 0.7f);
        X = GameWidth / 2f;
        Y = GameHeight * 0.8f;
        TargetX = X;
        TargetY = Y;
    }

    protected override void OnUpdate(float deltaTime)
    {
        // Smooth follow toward target
        float lerp = 1f - MathF.Pow(0.001f, deltaTime * PlayerSpeed);
        X += (TargetX - X) * lerp;
        Y += (TargetY - Y) * lerp;

        // Clamp to game area
        X = Math.Clamp(X, PlayerShipRadius, GameWidth - PlayerShipRadius);
        Y = Math.Clamp(Y, PlayerShipRadius * 2, GameHeight - PlayerShipRadius);

        if (InvincibleTimer > 0f)
            InvincibleTimer -= deltaTime;

        _engineFlicker += deltaTime * 20f;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        // Invincibility blink
        if (IsInvincible && MathF.Sin(InvincibleTimer * 15f) < 0f)
            return;

        // Engine glow
        float flicker = 0.6f + 0.4f * MathF.Sin(_engineFlicker);
        float glowRadius = 6f + 3f * flicker;
        _enginePaint.Shader = SKShader.CreateRadialGradient(
            new SKPoint(0, PlayerShipRadius * 0.5f),
            glowRadius,
            [CyanAccent.WithAlpha((byte)(180 * flicker)), SKColors.Transparent],
            SKShaderTileMode.Clamp);
        canvas.DrawCircle(0, PlayerShipRadius * 0.5f, glowRadius, _enginePaint);

        // Ship body
        canvas.DrawPath(_shipPath, _shipPaint);
        canvas.DrawPath(_shipPath, _shipOutline);

        // Shield visual when active
        // (drawn from PlayScreen for proper layering)
    }
}
