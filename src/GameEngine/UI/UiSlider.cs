using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// A themed slider entity with built-in collision, sprite, and readable state.
/// <para>
/// Drag the slider to change its <see cref="Value"/> (0 – 1). Call
/// <see cref="UpdateValueFromPointer"/> during pointer-move to track the thumb.
/// </para>
/// <example><code>
/// var slider = new UiSlider(320f, 26f, themeProvider);
/// slider.X = 200f; slider.Y = 363f;
/// controls.AddChild(slider);
///
/// // On pointer down / move when dragging:
/// slider.UpdateValueFromPointer(worldX);
///
/// // Read state:
/// int percent = (int)(slider.Value * 100f); // 0..100
/// </code></example>
/// </summary>
public class UiSlider : Entity
{
    /// <summary>
    /// Creates a new slider entity with the given dimensions and theme.
    /// </summary>
    /// <param name="width">Slider track width in game-space units.</param>
    /// <param name="height">Slider hit-area height in game-space units.</param>
    /// <param name="themeProvider">Provides the active UI theme for rendering.</param>
    public UiSlider(float width, float height, IUiThemeProvider themeProvider)
    {
        Width = width;
        Height = height;
        ThemeProvider = themeProvider;
        Collider = new RectCollider { Width = width, Height = height };
    }

    /// <summary>The theme provider used for rendering.</summary>
    public IUiThemeProvider ThemeProvider { get; }

    /// <summary>Slider track width in game-space units.</summary>
    public float Width { get; }

    /// <summary>Slider hit-area height in game-space units.</summary>
    public float Height { get; }

    /// <summary>Current slider value in the range 0 – 1.</summary>
    public float Value { get; set; } = 0.5f;

    /// <summary>
    /// Optional per-slider style override. When null, uses the theme's default slider style.
    /// </summary>
    public UiSliderStyle? StyleOverride { get; set; }

    /// <summary>
    /// Optional custom draw callback. When set, replaces the default rendering entirely.
    /// </summary>
    public Action<SKCanvas, SKRect, UiSliderStyle, float>? CustomDraw { get; set; }

    /// <summary>Returns the active slider style (override or theme default).</summary>
    public UiSliderStyle EffectiveStyle => StyleOverride ?? ThemeProvider.Theme.Slider;

    internal SKRect LocalRect => SKRect.Create(-Width / 2f, -Height / 2f, Width, Height);

    /// <summary>
    /// Updates <see cref="Value"/> based on a world-space pointer X coordinate.
    /// Call this during pointer-down and pointer-move while dragging.
    /// </summary>
    /// <param name="worldX">The pointer's X position in world space.</param>
    public void UpdateValueFromPointer(float worldX)
    {
        float left = WorldX - Width / 2f;
        Value = Math.Clamp((worldX - left) / Width, 0f, 1f);
    }

    /// <inheritdoc />
    protected override void OnDraw(SKCanvas canvas)
    {
        UiControls.DrawSlider(canvas, LocalRect, Value,
            EffectiveStyle, CustomDraw);
    }
}
