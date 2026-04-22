using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// A themed checkbox entity with built-in collision, sprite, and readable state.
/// <para>
/// Toggle <see cref="IsChecked"/> on pointer-down to flip the checkbox.
/// </para>
/// <example><code>
/// var checkbox = new UiCheckbox(34f, 34f, themeProvider);
/// checkbox.X = 57f; checkbox.Y = 221f;
/// controls.AddChild(checkbox);
///
/// // On pointer down:
/// if (controls.FindChildCollision(pointer, out _) is UiCheckbox cb)
///     cb.IsChecked = !cb.IsChecked;
///
/// // Read state:
/// if (checkbox.IsChecked) { /* … */ }
/// </code></example>
/// </summary>
public class UiCheckbox : UiControl
{
    /// <summary>
    /// Creates a new checkbox entity with the given dimensions and theme.
    /// </summary>
    /// <param name="width">Checkbox width in game-space units.</param>
    /// <param name="height">Checkbox height in game-space units.</param>
    /// <param name="theme">Provides the active UI theme for rendering.</param>
    public UiCheckbox(float width, float height, UiTheme theme) : base(width, height, theme) { }

    /// <summary>Whether the checkbox is currently checked.</summary>
    public bool IsChecked { get; set; }

    /// <summary>
    /// Optional per-checkbox appearance override. When null, uses the theme's default.
    /// </summary>
    public UiAppearance<UiCheckbox>? Appearance { get; set; }

    /// <inheritdoc />
    protected override void OnDraw(SKCanvas canvas)
    {
        (Appearance ?? Theme.Checkbox).Draw(canvas, this);
    }
}
