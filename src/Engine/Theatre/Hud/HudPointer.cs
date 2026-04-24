namespace SkiaSharp.Theatre;

/// <summary>
/// A visible pointer/cursor actor with a point collider for hit testing.
/// <para>
/// Opt in by setting <see cref="Scene.Pointer"/> on any scene.
/// The engine automatically updates position from pointer events.
/// Draw it last in your scene's <see cref="Scene.Draw"/> to keep it on top.
/// </para>
/// <para>
/// The default appearance is a crosshair. Swap via <see cref="Appearance"/>:
/// <c>Pointer.Appearance = myCustomAppearance;</c>
/// </para>
/// <example><code>
/// // In your scene constructor:
/// Pointer = new HudPointer();
///
/// // In OnPointerDown:
/// var hit = Pointer!.FindHit(_controls);
/// if (hit is HudButton btn) btn.IsPressed = true;
///
/// // At end of Draw:
/// HudPointer?.Draw(canvas);
/// </code></example>
/// </summary>
public class HudPointer : HudActor
{
    /// <summary>
    /// Creates a new pointer actor with a point collider.
    /// Initially invisible — becomes visible on first pointer event.
    /// </summary>
    public HudPointer()
    {
        Collider = new CircleCollider { Radius = 1f };
        Visible = false;
    }

    /// <summary>Whether the pointer is currently pressed/down.</summary>
    public bool IsDown { get; set; }

    /// <summary>
    /// Optional per-pointer appearance override. When null, uses the theme's pointer
    /// appearance or a built-in crosshair fallback.
    /// </summary>
    public HudAppearance<HudPointer>? Appearance { get; set; }

    /// <summary>
    /// Finds the first child of <paramref name="container"/> that the pointer overlaps.
    /// </summary>
    public Actor? FindHit(Actor container) => container.FindChildCollision(this, out _);

    /// <summary>
    /// Finds the first child of <paramref name="container"/> that the pointer overlaps,
    /// returning collision details.
    /// </summary>
    public Actor? FindHit(Actor container, out CollisionHit hit) =>
        container.FindChildCollision(this, out hit);

    /// <inheritdoc />
    protected override void OnDraw(SKCanvas canvas)
    {
        var appearance = Appearance ?? ResolvedHudTheme?.Pointer;
        if (appearance is not null)
        {
            appearance.Draw(canvas, this);
            return;
        }

        // Minimal fallback crosshair when no theme/appearance is configured
        byte alpha = (byte)(255 * Math.Clamp(Alpha, 0f, 1f));
        if (alpha == 0)
            return;

        float size = IsDown ? 6f : 8f;
        float gap = IsDown ? 1.5f : 2.5f;

        using var dark = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2.5f,
            Color = new SKColor(0, 0, 0, (byte)(180 * alpha / 255)),
        };
        canvas.DrawLine(-size, 0, -gap, 0, dark);
        canvas.DrawLine(gap, 0, size, 0, dark);
        canvas.DrawLine(0, -size, 0, -gap, dark);
        canvas.DrawLine(0, gap, 0, size, dark);

        using var light = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            Color = SKColors.White.WithAlpha(alpha),
        };
        canvas.DrawLine(-size, 0, -gap, 0, light);
        canvas.DrawLine(gap, 0, size, 0, light);
        canvas.DrawLine(0, -size, 0, -gap, light);
        canvas.DrawLine(0, gap, 0, size, light);
    }
}