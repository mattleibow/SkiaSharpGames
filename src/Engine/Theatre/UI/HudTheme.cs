namespace SkiaSharp.Theatre;

/// <summary>
/// Mutable theme that controls read each frame to resolve their default appearance.
/// Register as a singleton and pass to controls. Switch themes at runtime via
/// <see cref="ApplyFrom"/>.
/// </summary>
public sealed class HudTheme
{
    public HudAppearance<HudButton> Button { get; set; } = DefaultButtonAppearance.Default;
    public HudAppearance<HudCheckbox> Checkbox { get; set; } = DefaultCheckboxAppearance.Default;
    public HudAppearance<HudSwitch> Switch { get; set; } = DefaultSwitchAppearance.Default;
    public HudAppearance<HudSlider> Slider { get; set; } = DefaultSliderAppearance.Default;
    public HudAppearance<HudJoystick> Joystick { get; set; } = DefaultJoystickAppearance.Default;
    public HudAppearance<HudPointer> Pointer { get; set; } = DefaultPointerAppearance.Default;
    public HudAppearance<HudLabel> Label { get; set; } = DefaultLabelAppearance.Default;

    /// <summary>
    /// Copies all appearances from <paramref name="other"/> into this theme.
    /// All controls sharing this theme instance see the change immediately.
    /// </summary>
    public void ApplyFrom(HudTheme other)
    {
        Button = other.Button;
        Checkbox = other.Checkbox;
        Switch = other.Switch;
        Slider = other.Slider;
        Joystick = other.Joystick;
        Pointer = other.Pointer;
        Label = other.Label;
    }
}
