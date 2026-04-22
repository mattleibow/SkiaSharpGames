using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// A themed slider actor with built-in collision, sprite, and readable state.
/// <para>
/// Drag the slider to change its <see cref="Value"/> (0 – 1). Call
/// <see cref="UpdateValueFromPointer"/> during pointer-move to track the thumb.
/// </para>
/// <example><code>
/// var slider = new UiSlider(320f, 26f, theme);
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
public class UiSlider : UiControl
{
    /// <summary>
    /// Creates a new slider actor with the given dimensions and theme.
    /// </summary>
    /// <param name="width">Slider track width in game-space units.</param>
    /// <param name="height">Slider hit-area height in game-space units.</param>
    /// <param name="theme">Provides the active UI theme for rendering.</param>
    public UiSlider(float width, float height, UiTheme theme) : base(width, height, theme) { }

    /// <summary>Current slider value in the range 0 – 1.</summary>
    public float Value { get; set; } = 0.5f;

    /// <summary>
    /// Optional per-slider appearance override. When null, uses the theme's default.
    /// </summary>
    public UiAppearance<UiSlider>? Appearance { get; set; }

    /// <summary>
    /// Updates <see cref="Value"/> based on a world-space pointer X coordinate.
    /// Call this during pointer-down and pointer-move while dragging.
    /// </summary>
    /// <param name="worldX">The pointer's X position in world space.</param>
    public void UpdateValueFromPointer(float worldX)
    {
        if (Width <= 0f) { Value = 0f; return; }
        float left = WorldX - Width / 2f;
        Value = Math.Clamp((worldX - left) / Width, 0f, 1f);
    }

    /// <inheritdoc />
    protected override void OnDraw(SKCanvas canvas)
    {
        (Appearance ?? Theme.Slider).Draw(canvas, this);
    }
}
