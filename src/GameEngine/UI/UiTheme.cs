using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

/// <summary>
/// Mutable theme that controls read each frame to resolve their default appearance.
/// Register as a singleton and pass to controls. Switch themes at runtime via
/// <see cref="ApplyFrom"/>.
/// </summary>
public sealed class UiTheme
{
    public UiAppearance<UiButton> Button { get; set; } = UiButtonAppearance.Default;
    public UiAppearance<UiCheckbox> Checkbox { get; set; } = UiCheckboxAppearance.Default;
    public UiAppearance<UiSwitch> Switch { get; set; } = UiSwitchAppearance.Default;
    public UiAppearance<UiSlider> Slider { get; set; } = UiSliderAppearance.Default;
    public UiAppearance<UiJoystick> Joystick { get; set; } = UiJoystickAppearance.Default;
    public UiAppearance<UiPointer> Pointer { get; set; } = UiPointerAppearance.Default;

    /// <summary>
    /// Copies all appearances from <paramref name="other"/> into this theme.
    /// All controls sharing this theme instance see the change immediately.
    /// </summary>
    public void ApplyFrom(UiTheme other)
    {
        Button = other.Button;
        Checkbox = other.Checkbox;
        Switch = other.Switch;
        Slider = other.Slider;
        Joystick = other.Joystick;
        Pointer = other.Pointer;
    }
}

public static class UiThemes
{
    public static UiTheme Simple { get; } = new()
    {
        Button = UiButtonAppearance.Default,
        Checkbox = UiCheckboxAppearance.Default,
        Switch = UiSwitchAppearance.Default,
        Slider = UiSliderAppearance.Default,
        Joystick = UiJoystickAppearance.Default,
        Pointer = UiPointerAppearance.Default,
    };

    public static UiTheme BoldCute { get; } = new()
    {
        Button = UiButtonAppearance.Default with
        {
            FillColor = new SKColor(0xFF, 0x77, 0xB4),
            PressedFillColor = new SKColor(0xE1, 0x4B, 0x95),
            BorderColor = new SKColor(0xFF, 0xE1, 0xF0),
            BevelLightColor = new SKColor(0xFF, 0xD7, 0xEA),
            BevelShadowColor = new SKColor(0xAA, 0x2B, 0x69),
            CornerRadius = 16f,
            BorderWidth = 3f,
            BevelSize = 3f,
        },
        Checkbox = UiCheckboxAppearance.Default with
        {
            FillColor = new SKColor(0x4D, 0x2F, 0x5B),
            BorderColor = new SKColor(0xFF, 0xB7, 0xDF),
            CheckColor = new SKColor(0x8D, 0xFF, 0xB0),
            CornerRadius = 8f,
        },
        Switch = UiSwitchAppearance.Default with
        {
            TrackOffColor = new SKColor(0x6A, 0x43, 0x7F),
            TrackOnColor = new SKColor(0xFF, 0x92, 0xCF),
            KnobColor = new SKColor(0xFF, 0xF2, 0xF9),
            BorderColor = new SKColor(0x3B, 0x1F, 0x47),
            CornerRadius = 18f,
            BorderWidth = 3f,
        },
        Slider = UiSliderAppearance.Default with
        {
            TrackColor = new SKColor(0x6A, 0x43, 0x7F),
            FillColor = new SKColor(0xFF, 0xA2, 0xD6),
            KnobColor = new SKColor(0xFF, 0xF6, 0xFC),
            TrackHeight = 12f,
            KnobRadius = 13f,
            BorderWidth = 3f,
        },
        Joystick = UiJoystickAppearance.Default with
        {
            BaseColor = new SKColor(0x69, 0x3A, 0x80, 180),
            BaseBorderColor = new SKColor(0xFF, 0xD8, 0xEC, 220),
            KnobColor = new SKColor(0xFF, 0xF0, 0xF8, 235),
            KnobBorderColor = new SKColor(0x50, 0x1B, 0x63),
            BorderWidth = 3f,
        },
    };

    public static UiTheme Retro { get; } = new()
    {
        Button = UiButtonAppearance.Default with
        {
            FillColor = new SKColor(0x35, 0x4A, 0x3A),
            PressedFillColor = new SKColor(0x28, 0x37, 0x2B),
            TextColor = new SKColor(0xE8, 0xD6, 0x9C),
            BorderColor = new SKColor(0xD4, 0xB0, 0x65),
            BevelLightColor = new SKColor(0x72, 0x92, 0x69),
            BevelShadowColor = new SKColor(0x11, 0x18, 0x10),
            CornerRadius = 4f,
            BorderWidth = 2f,
            BevelSize = 2f,
        },
        Checkbox = UiCheckboxAppearance.Default with
        {
            FillColor = new SKColor(0x1F, 0x2B, 0x20),
            BorderColor = new SKColor(0xD4, 0xB0, 0x65),
            CheckColor = new SKColor(0xD2, 0xF8, 0x6D),
            CornerRadius = 2f,
        },
        Switch = UiSwitchAppearance.Default with
        {
            TrackOffColor = new SKColor(0x2A, 0x35, 0x2B),
            TrackOnColor = new SKColor(0x5F, 0x86, 0x48),
            KnobColor = new SKColor(0xE8, 0xD6, 0x9C),
            BorderColor = new SKColor(0x12, 0x1C, 0x13),
            CornerRadius = 4f,
        },
        Slider = UiSliderAppearance.Default with
        {
            TrackColor = new SKColor(0x2B, 0x3A, 0x2D),
            FillColor = new SKColor(0xA7, 0xD1, 0x5B),
            KnobColor = new SKColor(0xE8, 0xD6, 0x9C),
            BorderColor = new SKColor(0x10, 0x18, 0x10),
            TrackHeight = 8f,
            KnobRadius = 9f,
            BorderWidth = 2f,
        },
        Joystick = UiJoystickAppearance.Default with
        {
            BaseColor = new SKColor(0x1E, 0x2A, 0x20, 175),
            BaseBorderColor = new SKColor(0xD4, 0xB0, 0x65, 210),
            KnobColor = new SKColor(0xD9, 0xC1, 0x87, 235),
            KnobBorderColor = new SKColor(0x11, 0x18, 0x10),
            BorderWidth = 2f,
        },
    };
}
