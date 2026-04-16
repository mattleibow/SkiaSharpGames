using SkiaSharp;

namespace SkiaSharpGames.GameEngine.UI;

public sealed class UiTheme
{
    public required string Name { get; init; }
    public UiButtonStyle Button { get; init; } = UiButtonStyle.Default;
    public UiCheckboxStyle Checkbox { get; init; } = UiCheckboxStyle.Default;
    public UiSwitchStyle Switch { get; init; } = UiSwitchStyle.Default;
    public UiSliderStyle Slider { get; init; } = UiSliderStyle.Default;
    public UiJoystickStyle Joystick { get; init; } = UiJoystickStyle.Default;
}

public readonly record struct UiButtonStyle(
    SKColor FillColor,
    SKColor PressedFillColor,
    SKColor TextColor,
    SKColor BorderColor,
    SKColor BevelLightColor,
    SKColor BevelShadowColor,
    float CornerRadius,
    float BorderWidth,
    float BevelSize,
    byte DisabledAlpha)
{
    public static UiButtonStyle Default => new(
        FillColor: new SKColor(0x36, 0x44, 0x56),
        PressedFillColor: new SKColor(0x22, 0x2D, 0x3D),
        TextColor: SKColors.White,
        BorderColor: new SKColor(0x95, 0xA5, 0xB5),
        BevelLightColor: new SKColor(0x95, 0xB6, 0xD5),
        BevelShadowColor: new SKColor(0x10, 0x18, 0x23),
        CornerRadius: 10f,
        BorderWidth: 2f,
        BevelSize: 2f,
        DisabledAlpha: 110);
}

public readonly record struct UiCheckboxStyle(
    SKColor FillColor,
    SKColor BorderColor,
    SKColor CheckColor,
    float CornerRadius,
    float BorderWidth)
{
    public static UiCheckboxStyle Default => new(
        FillColor: new SKColor(0x22, 0x2A, 0x35),
        BorderColor: new SKColor(0x8D, 0xA2, 0xB8),
        CheckColor: new SKColor(0x61, 0xD0, 0x7D),
        CornerRadius: 6f,
        BorderWidth: 2f);
}

public enum UiSwitchVariant
{
    Sliding,
    ToggleButton,
}

public readonly record struct UiSwitchStyle(
    SKColor TrackOffColor,
    SKColor TrackOnColor,
    SKColor KnobColor,
    SKColor BorderColor,
    SKColor TextColor,
    float CornerRadius,
    float BorderWidth)
{
    public static UiSwitchStyle Default => new(
        TrackOffColor: new SKColor(0x44, 0x4F, 0x5E),
        TrackOnColor: new SKColor(0x46, 0xA4, 0xF6),
        KnobColor: SKColors.White,
        BorderColor: new SKColor(0x15, 0x1D, 0x27),
        TextColor: SKColors.White,
        CornerRadius: 14f,
        BorderWidth: 2f);
}

public readonly record struct UiSliderStyle(
    SKColor TrackColor,
    SKColor FillColor,
    SKColor KnobColor,
    SKColor BorderColor,
    float TrackHeight,
    float KnobRadius,
    float BorderWidth)
{
    public static UiSliderStyle Default => new(
        TrackColor: new SKColor(0x35, 0x3F, 0x4E),
        FillColor: new SKColor(0x5A, 0xB5, 0xFF),
        KnobColor: SKColors.White,
        BorderColor: new SKColor(0x17, 0x1D, 0x27),
        TrackHeight: 10f,
        KnobRadius: 11f,
        BorderWidth: 2f);
}

public readonly record struct UiJoystickStyle(
    SKColor BaseColor,
    SKColor BaseBorderColor,
    SKColor KnobColor,
    SKColor KnobBorderColor,
    float BorderWidth)
{
    public static UiJoystickStyle Default => new(
        BaseColor: new SKColor(0x2A, 0x34, 0x44, 170),
        BaseBorderColor: new SKColor(0xA0, 0xB6, 0xCC, 200),
        KnobColor: new SKColor(0xE8, 0xF2, 0xFF, 230),
        KnobBorderColor: new SKColor(0x10, 0x16, 0x20),
        BorderWidth: 2f);
}

public static class UiThemes
{
    public static UiTheme Simple { get; } = new()
    {
        Name = "Simple",
        Button = UiButtonStyle.Default,
        Checkbox = UiCheckboxStyle.Default,
        Switch = UiSwitchStyle.Default,
        Slider = UiSliderStyle.Default,
        Joystick = UiJoystickStyle.Default,
    };

    public static UiTheme BoldCute { get; } = new()
    {
        Name = "Bold/Cute",
        Button = UiButtonStyle.Default with
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
        Checkbox = UiCheckboxStyle.Default with
        {
            FillColor = new SKColor(0x4D, 0x2F, 0x5B),
            BorderColor = new SKColor(0xFF, 0xB7, 0xDF),
            CheckColor = new SKColor(0x8D, 0xFF, 0xB0),
            CornerRadius = 8f,
        },
        Switch = UiSwitchStyle.Default with
        {
            TrackOffColor = new SKColor(0x6A, 0x43, 0x7F),
            TrackOnColor = new SKColor(0xFF, 0x92, 0xCF),
            KnobColor = new SKColor(0xFF, 0xF2, 0xF9),
            BorderColor = new SKColor(0x3B, 0x1F, 0x47),
            CornerRadius = 18f,
            BorderWidth = 3f,
        },
        Slider = UiSliderStyle.Default with
        {
            TrackColor = new SKColor(0x6A, 0x43, 0x7F),
            FillColor = new SKColor(0xFF, 0xA2, 0xD6),
            KnobColor = new SKColor(0xFF, 0xF6, 0xFC),
            TrackHeight = 12f,
            KnobRadius = 13f,
            BorderWidth = 3f,
        },
        Joystick = UiJoystickStyle.Default with
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
        Name = "Retro",
        Button = UiButtonStyle.Default with
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
        Checkbox = UiCheckboxStyle.Default with
        {
            FillColor = new SKColor(0x1F, 0x2B, 0x20),
            BorderColor = new SKColor(0xD4, 0xB0, 0x65),
            CheckColor = new SKColor(0xD2, 0xF8, 0x6D),
            CornerRadius = 2f,
        },
        Switch = UiSwitchStyle.Default with
        {
            TrackOffColor = new SKColor(0x2A, 0x35, 0x2B),
            TrackOnColor = new SKColor(0x5F, 0x86, 0x48),
            KnobColor = new SKColor(0xE8, 0xD6, 0x9C),
            BorderColor = new SKColor(0x12, 0x1C, 0x13),
            TextColor = new SKColor(0xE8, 0xD6, 0x9C),
            CornerRadius = 4f,
        },
        Slider = UiSliderStyle.Default with
        {
            TrackColor = new SKColor(0x2B, 0x3A, 0x2D),
            FillColor = new SKColor(0xA7, 0xD1, 0x5B),
            KnobColor = new SKColor(0xE8, 0xD6, 0x9C),
            BorderColor = new SKColor(0x10, 0x18, 0x10),
            TrackHeight = 8f,
            KnobRadius = 9f,
            BorderWidth = 2f,
        },
        Joystick = UiJoystickStyle.Default with
        {
            BaseColor = new SKColor(0x1E, 0x2A, 0x20, 175),
            BaseBorderColor = new SKColor(0xD4, 0xB0, 0x65, 210),
            KnobColor = new SKColor(0xD9, 0xC1, 0x87, 235),
            KnobBorderColor = new SKColor(0x11, 0x18, 0x10),
            BorderWidth = 2f,
        },
    };
}
