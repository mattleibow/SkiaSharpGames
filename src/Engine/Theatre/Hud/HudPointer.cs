using SkiaSharp;

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
/// <c>Pointer.Appearance = DefaultDotAppearance.Default;</c>
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
    /// Optional per-pointer appearance override. When null, uses the theme's default or
    /// <see cref="DefaultPointerAppearance.Default"/>.
    /// </summary>
    public HudAppearance<HudPointer>? Appearance { get; set; }

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
        (Appearance ?? ResolvedHudTheme?.Pointer ?? DefaultPointerAppearance.Default).Draw(canvas, this);
    }
}
