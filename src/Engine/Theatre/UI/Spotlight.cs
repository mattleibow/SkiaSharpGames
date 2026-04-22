using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// A visible pointer/cursor entity with a point collider for hit testing.
/// <para>
/// Opt in by setting <see cref="Scene.Spotlight"/> on any screen.
/// The engine automatically updates position from pointer events.
/// Draw it last in your screen's <see cref="Scene.Draw"/> to keep it on top.
/// </para>
/// <para>
/// The default appearance is a crosshair. Swap via <see cref="Appearance"/>:
/// <c>Pointer.Appearance = UiDotAppearance.Default;</c>
/// </para>
/// <example><code>
/// // In your screen constructor:
/// Pointer = new Spotlight();
///
/// // In OnPointerDown:
/// var hit = Pointer!.FindHit(_controls);
/// if (hit is UiButton btn) btn.IsPressed = true;
///
/// // At end of Draw:
/// Spotlight?.Draw(canvas);
/// </code></example>
/// </summary>
public class Spotlight : UiActor
{
    /// <summary>
    /// Creates a new pointer entity with a point collider.
    /// Initially invisible — becomes visible on first pointer event.
    /// </summary>
    /// <param name="theme">
    /// Optional theme for resolving the pointer appearance.
    /// When null, falls back to <see cref="SpotlightAppearance.Default"/>.
    /// </param>
    public Spotlight(UiTheme? theme = null) : base(theme)
    {
        Collider = new CircleCollider { Radius = 1f };
        Visible = false;
    }

    /// <summary>Whether the pointer is currently pressed/down.</summary>
    public bool IsDown { get; set; }

    /// <summary>
    /// Optional per-pointer appearance override. When null, uses the theme's default or
    /// <see cref="SpotlightAppearance.Default"/>.
    /// </summary>
    public UiAppearance<Spotlight>? Appearance { get; set; }

    /// <summary>
    /// Finds the first child of <paramref name="container"/> that the pointer overlaps.
    /// </summary>
    public Actor? FindHit(Actor container)
        => container.FindChildCollision(this, out _);

    /// <summary>
    /// Finds the first child of <paramref name="container"/> that the pointer overlaps,
    /// returning collision details.
    /// </summary>
    public Actor? FindHit(Actor container, out CollisionHit hit)
        => container.FindChildCollision(this, out hit);

    /// <inheritdoc />
    protected override void OnDraw(SKCanvas canvas)
    {
        (Appearance ?? Theme?.Spotlight ?? SpotlightAppearance.Default).Draw(canvas, this);
    }
}
