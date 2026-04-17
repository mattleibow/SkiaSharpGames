using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// A themed switch entity with built-in collision and readable state.
/// Set <see cref="Entity.Appearance"/> to <see cref="UiToggleButtonAppearance"/>
/// for a toggle-button variant, or leave default for a sliding switch.
/// <example><code>
/// var sw = new UiSwitch(110f, 42f, themeProvider);
/// sw.X = 95f; sw.Y = 289f;
/// controls.AddChild(sw);
///
/// // On pointer down:
/// if (controls.FindChildCollision(pointer, out _) is UiSwitch s)
///     s.IsOn = !s.IsOn;
///
/// // Read state:
/// if (sw.IsOn) { /* … */ }
/// </code></example>
/// </summary>
public class UiSwitch : Entity
{
    /// <summary>
    /// Creates a new switch entity with the given dimensions and theme.
    /// </summary>
    /// <param name="width">Switch width in game-space units.</param>
    /// <param name="height">Switch height in game-space units.</param>
    /// <param name="themeProvider">Provides the active UI theme for rendering.</param>
    /// <param name="variant">Visual variant — sliding track or toggle button.</param>
    public UiSwitch(float width, float height, UiTheme theme)
    {
        Width = width;
        Height = height;
        Theme = theme;
        Collider = new RectCollider { Width = width, Height = height };
    }

    /// <summary>The theme used for rendering.</summary>
    public UiTheme Theme { get; }

    /// <summary>Switch width in game-space units.</summary>
    public float Width { get; }

    /// <summary>Switch height in game-space units.</summary>
    public float Height { get; }

    /// <summary>Whether the switch is currently on.</summary>
    public bool IsOn { get; set; }

    /// <summary>
    /// Optional per-switch appearance override. When null, uses the theme's default.
    /// </summary>
    public UiAppearance<UiSwitch>? Appearance { get; set; }

    /// <summary>The local-space bounding rectangle centred at the origin.</summary>
    public SKRect LocalRect => SKRect.Create(-Width / 2f, -Height / 2f, Width, Height);

    /// <inheritdoc />
    protected override void OnDraw(SKCanvas canvas)
    {
        (Appearance ?? Theme.Switch).Draw(canvas, this);
    }
}
