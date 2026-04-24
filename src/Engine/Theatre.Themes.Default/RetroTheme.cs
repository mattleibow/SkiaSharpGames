using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharp.Theatre.Themes.Default;

/// <summary>
/// Factory for the retro UI theme — an earthy green/gold colour variation of the default appearances.
/// </summary>
public static class RetroTheme
{
    /// <summary>Creates a new <see cref="HudTheme"/> with retro colour overrides.</summary>
    public static HudTheme Create() =>
        new()
        {
            Button = DefaultButtonAppearance.Default with
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
            Checkbox = DefaultCheckboxAppearance.Default with
            {
                FillColor = new SKColor(0x1F, 0x2B, 0x20),
                BorderColor = new SKColor(0xD4, 0xB0, 0x65),
                CheckColor = new SKColor(0xD2, 0xF8, 0x6D),
                CornerRadius = 2f,
            },
            Switch = DefaultSwitchAppearance.Default with
            {
                TrackOffColor = new SKColor(0x2A, 0x35, 0x2B),
                TrackOnColor = new SKColor(0x5F, 0x86, 0x48),
                KnobColor = new SKColor(0xE8, 0xD6, 0x9C),
                BorderColor = new SKColor(0x12, 0x1C, 0x13),
                CornerRadius = 4f,
            },
            Slider = DefaultSliderAppearance.Default with
            {
                TrackColor = new SKColor(0x2B, 0x3A, 0x2D),
                FillColor = new SKColor(0xA7, 0xD1, 0x5B),
                KnobColor = new SKColor(0xE8, 0xD6, 0x9C),
                BorderColor = new SKColor(0x10, 0x18, 0x10),
                TrackHeight = 8f,
                KnobRadius = 9f,
                BorderWidth = 2f,
            },
            Joystick = DefaultJoystickAppearance.Default with
            {
                BaseColor = new SKColor(0x1E, 0x2A, 0x20, 175),
                BaseBorderColor = new SKColor(0xD4, 0xB0, 0x65, 210),
                KnobColor = new SKColor(0xD9, 0xC1, 0x87, 235),
                KnobBorderColor = new SKColor(0x11, 0x18, 0x10),
                BorderWidth = 2f,
            },
            Pointer = DefaultPointerAppearance.Default,
            Label = DefaultLabelAppearance.Default,
        };
}
