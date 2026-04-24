namespace SkiaSharp.Theatre;

/// <summary>
/// A themed switch actor with built-in collision and readable state.
/// <example><code>
/// var sw = new HudSwitch(110f, 42f);
/// sw.X = 95f; sw.Y = 289f;
/// controls.Children.Add(sw);
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
    /// Creates a new switch actor with the given dimensions.
    /// </summary>
    /// <param name="width">Switch width in game-space units.</param>
    /// <param name="height">Switch height in game-space units.</param>
    public HudSwitch(float width, float height)
        : base(width, height) { }

    /// <summary>Whether the switch is currently on.</summary>
    public bool IsOn { get; set; }

    /// <summary>
    /// Optional per-switch appearance override. When null, uses the theme's default.
    /// </summary>
    public HudAppearance<HudSwitch>? Appearance { get; set; }

    /// <inheritdoc />
    protected override void OnDraw(SKCanvas canvas)
    {
        (Appearance ?? ResolvedHudTheme?.Switch)?.Draw(canvas, this);
    }
}