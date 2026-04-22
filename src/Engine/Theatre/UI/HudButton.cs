using SkiaSharp;

namespace SkiaSharp.Theatre;

/// <summary>
/// A themed button actor with built-in collision, sprite, and readable state.
/// <para>
/// Add to any actor group and use <see cref="Actor.FindChildCollision"/> for
/// hit-testing. Read <see cref="IsPressed"/> to determine the current state.
/// </para>
/// <example><code>
/// var button = new HudButton(190f, 56f, theme) { Label = "Start" };
/// button.X = 200f; button.Y = 150f;
/// controls.AddChild(button);
///
/// // On pointer down — use collision to find the hit control:
/// if (controls.FindChildCollision(pointer, out _) is HudButton btn)
///     btn.IsPressed = true;
///
/// // Read state any time:
/// if (button.IsPressed) { /* … */ }
/// </code></example>
/// </summary>
public class HudButton : HudControl
{
    /// <summary>
    /// Creates a new button actor with the given dimensions and theme.
    /// The actor is positioned at its center; the collider and sprite
    /// are configured automatically.
    /// </summary>
    /// <param name="width">Button width in game-space units.</param>
    /// <param name="height">Button height in game-space units.</param>
    /// <param name="theme">Provides the active UI theme for rendering.</param>
    public HudButton(float width, float height, HudTheme theme) : base(width, height, theme) { }

    /// <summary>Text displayed on the button.</summary>
    public string Label { get; set; } = "";

    /// <summary>Whether the button is currently pressed/down.</summary>
    public bool IsPressed { get; set; }

    /// <summary>Whether the button is enabled (when false, draws at reduced opacity).</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Whether this button behaves as a toggle (on/off).</summary>
    public bool IsToggle { get; set; }

    /// <summary>Whether the toggle button is currently on (only meaningful when <see cref="IsToggle"/> is true).</summary>
    public bool IsOn { get; set; }

    /// <summary>Font size for the label text.</summary>
    public float FontSize { get; set; } = 18f;

    /// <summary>
    /// Optional per-button appearance override. When null, uses the theme's default.
    /// </summary>
    public HudAppearance<HudButton>? Appearance { get; set; }

    /// <inheritdoc />
    protected override void OnDraw(SKCanvas canvas)
    {
        (Appearance ?? Theme.Button).Draw(canvas, this);
    }
}
