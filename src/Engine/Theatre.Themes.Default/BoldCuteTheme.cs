using SkiaSharp;
using SkiaSharp.Theatre;

namespace SkiaSharp.Theatre.Themes.Default;

/// <summary>
/// Factory for the bold/cute UI theme — a pink, rounded colour variation of the default appearances.
/// </summary>
public static class BoldCuteTheme
{
    /// <summary>Creates a new <see cref="HudTheme"/> with bold/cute colour overrides.</summary>
    public static HudTheme Create() =>
        new()
        {
            Button = DefaultButtonAppearance.Default with
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
            Checkbox = DefaultCheckboxAppearance.Default with
            {
                FillColor = new SKColor(0x4D, 0x2F, 0x5B),
                BorderColor = new SKColor(0xFF, 0xB7, 0xDF),
                CheckColor = new SKColor(0x8D, 0xFF, 0xB0),
                CornerRadius = 8f,
            },
            Switch = DefaultSwitchAppearance.Default with
            {
                TrackOffColor = new SKColor(0x6A, 0x43, 0x7F),
                TrackOnColor = new SKColor(0xFF, 0x92, 0xCF),
                KnobColor = new SKColor(0xFF, 0xF2, 0xF9),
                BorderColor = new SKColor(0x3B, 0x1F, 0x47),
                CornerRadius = 18f,
                BorderWidth = 3f,
            },
            Slider = DefaultSliderAppearance.Default with
            {
                TrackColor = new SKColor(0x6A, 0x43, 0x7F),
                FillColor = new SKColor(0xFF, 0xA2, 0xD6),
                KnobColor = new SKColor(0xFF, 0xF6, 0xFC),
                TrackHeight = 12f,
                KnobRadius = 13f,
                BorderWidth = 3f,
            },
            Joystick = DefaultJoystickAppearance.Default with
            {
                BaseColor = new SKColor(0x69, 0x3A, 0x80, 180),
                BaseBorderColor = new SKColor(0xFF, 0xD8, 0xEC, 220),
                KnobColor = new SKColor(0xFF, 0xF0, 0xF8, 235),
                KnobBorderColor = new SKColor(0x50, 0x1B, 0x63),
                BorderWidth = 3f,
            },
            Pointer = DefaultPointerAppearance.Default,
            Label = DefaultLabelAppearance.Default,
        };
}