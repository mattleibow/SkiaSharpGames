using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>Visual style for the <see cref="UiPointer"/> cursor.</summary>
public enum UiPointerStyle
{
    /// <summary>Two thin lines forming a crosshair with a gap in the center.</summary>
    Crosshair,

    /// <summary>Small filled circle with a contrasting ring.</summary>
    Dot,

    /// <summary>Hollow circle that scales down on press.</summary>
    Ring
}

/// <summary>
/// A visible pointer/cursor entity with a point collider for hit testing.
/// <para>
/// Opt in by setting <see cref="GameScreen.Pointer"/> on any screen.
/// The engine automatically updates position from pointer events.
/// Draw it last in your screen's <see cref="GameScreen.Draw"/> to keep it on top.
/// </para>
/// <example><code>
/// // In your screen constructor:
/// Pointer = new UiPointer();
///
/// // In OnPointerDown:
/// var hit = Pointer!.FindHit(_controls);
/// if (hit is UiButton btn) btn.IsPressed = true;
///
/// // At end of Draw:
/// Pointer?.Draw(canvas);
/// </code></example>
/// </summary>
public class UiPointer : Entity
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

    /// <summary>
    /// Creates a new pointer entity with a point collider.
    /// Initially invisible — becomes visible on first pointer event.
    /// </summary>
    public UiPointer()
    {
        Collider = new CircleCollider { Radius = 1f };
        Visible = false;
    }

    /// <summary>Whether the pointer is currently pressed/down.</summary>
    public bool IsDown { get; set; }

    /// <summary>Visual style of the cursor.</summary>
    public UiPointerStyle Style { get; set; } = UiPointerStyle.Crosshair;

    /// <summary>
    /// Finds the first child of <paramref name="container"/> that the pointer overlaps.
    /// </summary>
    public Entity? FindHit(Entity container)
        => container.FindChildCollision(this, out _);

    /// <summary>
    /// Finds the first child of <paramref name="container"/> that the pointer overlaps,
    /// returning collision details.
    /// </summary>
    public Entity? FindHit(Entity container, out CollisionHit hit)
        => container.FindChildCollision(this, out hit);

    /// <inheritdoc />
    protected override void OnDraw(SKCanvas canvas)
    {
        switch (Style)
        {
            case UiPointerStyle.Crosshair:
                DrawCrosshair(canvas);
                break;
            case UiPointerStyle.Dot:
                DrawDot(canvas);
                break;
            case UiPointerStyle.Ring:
                DrawRing(canvas);
                break;
        }
    }

    private void DrawCrosshair(SKCanvas canvas)
    {
        float size = IsDown ? 6f : 8f;
        float gap = IsDown ? 1.5f : 2.5f;

        // Dark outline pass
        canvas.DrawLine(-size, 0, -gap, 0, StrokeDark);
        canvas.DrawLine(gap, 0, size, 0, StrokeDark);
        canvas.DrawLine(0, -size, 0, -gap, StrokeDark);
        canvas.DrawLine(0, gap, 0, size, StrokeDark);

        // Light inner pass
        canvas.DrawLine(-size, 0, -gap, 0, StrokeLight);
        canvas.DrawLine(gap, 0, size, 0, StrokeLight);
        canvas.DrawLine(0, -size, 0, -gap, StrokeLight);
        canvas.DrawLine(0, gap, 0, size, StrokeLight);
    }

    private void DrawDot(SKCanvas canvas)
    {
        float r = IsDown ? 2f : 3f;
        canvas.DrawCircle(0, 0, r + 1f, FillDark);
        canvas.DrawCircle(0, 0, r, FillLight);
    }

    private void DrawRing(SKCanvas canvas)
    {
        float r = IsDown ? 4f : 6f;
        canvas.DrawCircle(0, 0, r, StrokeDark);
        canvas.DrawCircle(0, 0, r, StrokeLight);
    }
}
