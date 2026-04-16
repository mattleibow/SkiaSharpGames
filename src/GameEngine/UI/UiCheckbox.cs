using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

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
public class UiCheckbox : Entity
{
    /// <summary>
    /// Creates a new checkbox entity with the given dimensions and theme.
    /// </summary>
    /// <param name="width">Checkbox width in game-space units.</param>
    /// <param name="height">Checkbox height in game-space units.</param>
    /// <param name="themeProvider">Provides the active UI theme for rendering.</param>
    public UiCheckbox(float width, float height, IUiThemeProvider themeProvider)
    {
        Width = width;
        Height = height;
        ThemeProvider = themeProvider;
        Collider = new RectCollider { Width = width, Height = height };
        Sprite = new UiCheckboxSprite(this);
    }

    /// <summary>The theme provider used for rendering.</summary>
    public IUiThemeProvider ThemeProvider { get; }

    /// <summary>Checkbox width in game-space units.</summary>
    public float Width { get; }

    /// <summary>Checkbox height in game-space units.</summary>
    public float Height { get; }

    /// <summary>Whether the checkbox is currently checked.</summary>
    public bool IsChecked { get; set; }

    /// <summary>
    /// Optional per-checkbox style override. When null, uses the theme's default checkbox style.
    /// </summary>
    public UiCheckboxStyle? StyleOverride { get; set; }

    /// <summary>
    /// Optional custom draw callback. When set, replaces the default rendering entirely.
    /// </summary>
    public Action<SKCanvas, SKRect, UiCheckboxStyle, bool>? CustomDraw { get; set; }

    /// <summary>Returns the active checkbox style (override or theme default).</summary>
    public UiCheckboxStyle EffectiveStyle => StyleOverride ?? ThemeProvider.Theme.Checkbox;

    internal SKRect LocalRect => SKRect.Create(-Width / 2f, -Height / 2f, Width, Height);

    private sealed class UiCheckboxSprite(UiCheckbox checkbox) : GameEngine.Sprite
    {
        public override void Draw(SKCanvas canvas)
        {
            UiControls.DrawCheckbox(canvas, checkbox.LocalRect, checkbox.IsChecked,
                checkbox.EffectiveStyle, checkbox.CustomDraw);
        }
    }
}
