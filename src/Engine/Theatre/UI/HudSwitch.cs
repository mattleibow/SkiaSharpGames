using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// A themed switch actor with built-in collision and readable state.
/// <example><code>
/// var sw = new HudSwitch(110f, 42f, theme);
/// sw.X = 95f; sw.Y = 289f;
/// controls.AddChild(sw);
///
/// // On pointer down:
/// if (controls.FindChildCollision(pointer, out _) is HudSwitch s)
///     s.IsOn = !s.IsOn;
///
/// // Read state:
/// if (sw.IsOn) { /* … */ }
/// </code></example>
/// </summary>
public class HudSwitch : HudControl
{
    /// <summary>
    /// Creates a new switch actor with the given dimensions and theme.
    /// </summary>
    /// <param name="width">Switch width in game-space units.</param>
    /// <param name="height">Switch height in game-space units.</param>
    /// <param name="theme">Provides the active UI theme for rendering.</param>
    public HudSwitch(float width, float height, HudTheme theme) : base(width, height, theme) { }

    /// <summary>Whether the switch is currently on.</summary>
    public bool IsOn { get; set; }

    /// <summary>
    /// Optional per-switch appearance override. When null, uses the theme's default.
    /// </summary>
    public HudAppearance<HudSwitch>? Appearance { get; set; }

    /// <inheritdoc />
    protected override void OnDraw(SKCanvas canvas)
    {
        (Appearance ?? Theme.Switch).Draw(canvas, this);
    }
}
