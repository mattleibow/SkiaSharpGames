using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// A themed switch entity with built-in collision, sprite, and readable state.
/// Supports both <see cref="UiSwitchVariant.Sliding"/> and
/// <see cref="UiSwitchVariant.ToggleButton"/> variants.
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
    public UiSwitch(float width, float height, IUiThemeProvider themeProvider, UiSwitchVariant variant = UiSwitchVariant.Sliding)
    {
        Width = width;
        Height = height;
        ThemeProvider = themeProvider;
        Variant = variant;
        Collider = new RectCollider { Width = width, Height = height };
    }

    /// <summary>The theme provider used for rendering.</summary>
    public IUiThemeProvider ThemeProvider { get; }

    /// <summary>Switch width in game-space units.</summary>
    public float Width { get; }

    /// <summary>Switch height in game-space units.</summary>
    public float Height { get; }

    /// <summary>Visual variant of the switch.</summary>
    public UiSwitchVariant Variant { get; set; }

    /// <summary>Whether the switch is currently on.</summary>
    public bool IsOn { get; set; }

    /// <summary>
    /// Optional per-switch style override. When null, uses the theme's default switch style.
    /// </summary>
    public UiSwitchStyle? StyleOverride { get; set; }

    /// <summary>
    /// Optional custom draw callback. When set, replaces the default rendering entirely.
    /// </summary>
    public Action<SKCanvas, SKRect, UiSwitchStyle, bool, UiSwitchVariant>? CustomDraw { get; set; }

    /// <summary>Returns the active switch style (override or theme default).</summary>
    public UiSwitchStyle EffectiveStyle => StyleOverride ?? ThemeProvider.Theme.Switch;

    internal SKRect LocalRect => SKRect.Create(-Width / 2f, -Height / 2f, Width, Height);

    /// <inheritdoc />
    protected override void OnDraw(SKCanvas canvas)
    {
        UiControls.DrawSwitch(canvas, LocalRect, IsOn,
            EffectiveStyle, Variant, CustomDraw);
    }
}
