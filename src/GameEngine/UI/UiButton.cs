using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// A themed button entity with built-in collision, sprite, and readable state.
/// <para>
/// Add to any entity group and use <see cref="Entity.FindChildCollision"/> for
/// hit-testing. Read <see cref="IsPressed"/> to determine the current state.
/// </para>
/// <example><code>
/// var button = new UiButton(190f, 56f, themeProvider) { Label = "Start" };
/// button.X = 200f; button.Y = 150f;
/// controls.AddChild(button);
///
/// // On pointer down — use collision to find the hit control:
/// if (controls.FindChildCollision(pointer, out _) is UiButton btn)
///     btn.IsPressed = true;
///
/// // Read state any time:
/// if (button.IsPressed) { /* … */ }
/// </code></example>
/// </summary>
public class UiButton : Entity
{
    /// <summary>
    /// Creates a new button entity with the given dimensions and theme.
    /// The entity is positioned at its center; the collider and sprite
    /// are configured automatically.
    /// </summary>
    /// <param name="width">Button width in game-space units.</param>
    /// <param name="height">Button height in game-space units.</param>
    /// <param name="themeProvider">Provides the active UI theme for rendering.</param>
    public UiButton(float width, float height, IUiThemeProvider themeProvider)
    {
        Width = width;
        Height = height;
        ThemeProvider = themeProvider;
        Collider = new RectCollider { Width = width, Height = height };
    }

    /// <summary>The theme provider used for rendering.</summary>
    public IUiThemeProvider ThemeProvider { get; }

    /// <summary>Button width in game-space units.</summary>
    public float Width { get; }

    /// <summary>Button height in game-space units.</summary>
    public float Height { get; }

    /// <summary>Text displayed on the button.</summary>
    public string Label { get; set; } = "";

    /// <summary>Whether the button is currently pressed/down.</summary>
    public bool IsPressed { get; set; }

    /// <summary>Whether the button is enabled (when false, draws at reduced opacity).</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Font size for the label text.</summary>
    public float FontSize { get; set; } = 18f;

    /// <summary>
    /// Optional per-button style override. When null, uses the theme's default button style.
    /// </summary>
    public UiButtonStyle? StyleOverride { get; set; }

    /// <summary>
    /// Optional custom draw callback. When set, replaces the default button rendering entirely.
    /// Parameters: canvas, local rect, effective style, isPressed, isEnabled.
    /// </summary>
    public Action<SKCanvas, SKRect, UiButtonStyle, bool, bool>? CustomDraw { get; set; }

    /// <summary>Returns the active button style (override or theme default).</summary>
    public UiButtonStyle EffectiveStyle => StyleOverride ?? ThemeProvider.Theme.Button;

    internal SKRect LocalRect => SKRect.Create(-Width / 2f, -Height / 2f, Width, Height);

    /// <inheritdoc />
    protected override void OnDraw(SKCanvas canvas)
    {
        UiControls.DrawButton(canvas, LocalRect, Label,
            EffectiveStyle, IsPressed, IsEnabled,
            FontSize, CustomDraw);
    }
}
