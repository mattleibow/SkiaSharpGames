using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// A themed checkbox actor with built-in collision, sprite, and readable state.
/// <para>
/// Toggle <see cref="IsChecked"/> on pointer-down to flip the checkbox.
/// </para>
/// <example><code>
/// var checkbox = new HudCheckbox(34f, 34f);
/// checkbox.X = 57f; checkbox.Y = 221f;
/// controls.Children.Add(checkbox);
///
/// // On pointer down:
/// if (controls.FindChildCollision(pointer, out _) is HudCheckbox cb)
///     cb.IsChecked = !cb.IsChecked;
///
/// // Read state:
/// if (checkbox.IsChecked) { /* … */ }
/// </code></example>
/// </summary>
public class HudCheckbox : HudControl
{
    /// <summary>
    /// Creates a new checkbox actor with the given dimensions.
    /// </summary>
    /// <param name="width">Checkbox width in game-space units.</param>
    /// <param name="height">Checkbox height in game-space units.</param>
    public HudCheckbox(float width, float height) : base(width, height) { }

    /// <summary>Whether the checkbox is currently checked.</summary>
    public bool IsChecked { get; set; }

    /// <summary>
    /// Optional per-checkbox appearance override. When null, uses the theme's default.
    /// </summary>
    public HudAppearance<HudCheckbox>? Appearance { get; set; }

    /// <inheritdoc />
    protected override void OnDraw(SKCanvas canvas)
    {
        (Appearance ?? ResolvedHudTheme?.Checkbox)?.Draw(canvas, this);
    }
}
