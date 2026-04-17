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
    /// <summary>
    /// Creates a new pointer entity with a point collider.
    /// Initially invisible — becomes visible on first pointer event.
    /// </summary>
    /// <param name="themeProvider">
    /// Optional theme provider for resolving the pointer appearance.
    /// When null, falls back to <see cref="UiPointerAppearance.Default"/>.
    /// </param>
    public UiPointer(IUiThemeProvider? themeProvider = null)
    {
        ThemeProvider = themeProvider;
        Collider = new CircleCollider { Radius = 1f };
        Visible = false;
    }

    /// <summary>The theme provider used for rendering (may be null).</summary>
    public IUiThemeProvider? ThemeProvider { get; }

    /// <summary>Whether the pointer is currently pressed/down.</summary>
    public bool IsDown { get; set; }

    /// <summary>Visual style of the cursor.</summary>
    public UiPointerStyle Style { get; set; } = UiPointerStyle.Crosshair;

    /// <summary>
    /// Optional per-pointer appearance override. When null, uses the theme's default or
    /// <see cref="UiPointerAppearance.Default"/>.
    /// </summary>
    public UiAppearance<UiPointer>? Appearance { get; set; }

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
        (Appearance ?? ThemeProvider?.Theme.Pointer ?? UiPointerAppearance.Default).Draw(canvas, this);
    }
}
