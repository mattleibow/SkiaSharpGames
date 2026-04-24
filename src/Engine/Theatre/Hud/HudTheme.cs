namespace SkiaSharp.Theatre;

/// <summary>
/// Immutable theme that controls read each frame to resolve their default appearance.
/// Set on <see cref="Stage.HudTheme"/>, <see cref="Scene.HudTheme"/>, or any
/// <see cref="SceneNode.HudTheme"/> for per-node overrides.
/// </summary>
public sealed class HudTheme
{
    /// <summary>Font configuration for the theme.</summary>
    public FontProvider Fonts { get; init; } = new();

    public HudAppearance<HudButton>? Button { get; init; }
    public HudAppearance<HudCheckbox>? Checkbox { get; init; }
    public HudAppearance<HudSwitch>? Switch { get; init; }
    public HudAppearance<HudSlider>? Slider { get; init; }
    public HudAppearance<HudJoystick>? Joystick { get; init; }
    public HudAppearance<HudPointer>? Pointer { get; init; }
    public HudAppearance<HudLabel>? Label { get; init; }
}
