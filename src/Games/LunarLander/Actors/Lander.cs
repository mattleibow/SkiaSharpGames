using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.LunarLander.LunarLanderConstants;

namespace SkiaSharpGames.LunarLander;

/// <summary>
/// The player's lander actor. Demonstrates rotation and child actors:
/// flame actors are children that rotate with the lander automatically.
/// </summary>
internal sealed class Lander : Actor
{
    private readonly SKPaint _bodyPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _legPaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 2f,
    };
    private readonly SKPaint _windowPaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };

    public Lander()
    {
        Collider = new RectCollider { Width = LanderWidth, Height = LanderHeight };
        Rigidbody = new Rigidbody2D();

        // Child actors for thruster flames — they orbit with lander rotation!
        MainFlame = new FlameEntity { Y = LanderHeight / 2f + 2f, Visible = false };
        LeftFlame = new FlameEntity
        {
            X = -LanderWidth / 2f,
            Y = -4f,
            Rotation = 0.5f,
            Intensity = 0.5f,
            Visible = false,
        };
        RightFlame = new FlameEntity
        {
            X = LanderWidth / 2f,
            Y = -4f,
            Rotation = -0.5f,
            Intensity = 0.5f,
            Visible = false,
        };

        Children.Add(MainFlame);
        Children.Add(LeftFlame);
        Children.Add(RightFlame);
    }

    public FlameEntity MainFlame { get; }
    public FlameEntity LeftFlame { get; }
    public FlameEntity RightFlame { get; }

    public SKColor Color { get; set; } = LanderColor;

    public new Rigidbody2D Rigidbody
    {
        get => (Rigidbody2D)base.Rigidbody!;
        init => base.Rigidbody = value;
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        float halfW = LanderWidth / 2f;
        float halfH = LanderHeight / 2f;

        // Body — a trapezoid/triangle shape pointing up
        _bodyPaint.Color = Color;
        using var bodyPath = new SKPath();
        bodyPath.MoveTo(0, -halfH); // top (nose)
        bodyPath.LineTo(halfW, halfH); // bottom-right
        bodyPath.LineTo(-halfW, halfH); // bottom-left
        bodyPath.Close();
        canvas.DrawPath(bodyPath, _bodyPaint);

        // Window — small circle near the top
        _windowPaint.Color = AccentColor;
        canvas.DrawCircle(0, -halfH + 8f, 4f, _windowPaint);

        // Landing legs
        _legPaint.Color = Color;
        float legExtend = 6f;
        canvas.DrawLine(-halfW, halfH, -halfW - 4f, halfH + legExtend, _legPaint);
        canvas.DrawLine(halfW, halfH, halfW + 4f, halfH + legExtend, _legPaint);
        // Horizontal foot pads
        canvas.DrawLine(-halfW - 6f, halfH + legExtend, -halfW - 2f, halfH + legExtend, _legPaint);
        canvas.DrawLine(halfW + 2f, halfH + legExtend, halfW + 6f, halfH + legExtend, _legPaint);
    }
}
