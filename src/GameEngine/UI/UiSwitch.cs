using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// A themed switch entity with built-in collision and readable state.
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
public class UiSwitch : UiControl
{
    /// <summary>
    /// Creates a new switch entity with the given dimensions and theme.
    /// </summary>
    /// <param name="width">Switch width in game-space units.</param>
    /// <param name="height">Switch height in game-space units.</param>
    /// <param name="theme">Provides the active UI theme for rendering.</param>
    public UiSwitch(float width, float height, UiTheme theme) : base(width, height, theme) { }

    /// <summary>Whether the switch is currently on.</summary>
    public bool IsOn { get; set; }

    /// <summary>
    /// Optional per-switch appearance override. When null, uses the theme's default.
    /// </summary>
    public UiAppearance<UiSwitch>? Appearance { get; set; }

    /// <inheritdoc />
    protected override void OnDraw(SKCanvas canvas)
    {
        (Appearance ?? Theme.Switch).Draw(canvas, this);
    }
}
